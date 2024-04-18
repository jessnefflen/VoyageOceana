using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Unity.Burst;

namespace CorgiFogGradient
{
    public class RenderPassFogGradient : ScriptableRenderPass
    {
        private const string _profilerTag = "RenderPassFogGradient";

        private RenderFeatureFogGradient.FogGradientSettings settings;
        private Texture2D _gradientTexture;
        private bool _textureNeedsUpdate;

        private static readonly int _FogGradientTexture = Shader.PropertyToID("_FogGradientTexture"); 
        private static readonly int _FogGradientData = Shader.PropertyToID("_FogGradientData");

        private static readonly int _CorgiGrabpass = Shader.PropertyToID("_CorgiGrabpass");
        private static readonly int _CorgiDepthGrabpass = Shader.PropertyToID("_CorgiDepthGrabpass");
        private static readonly int _CopyBlitTex = Shader.PropertyToID("_CopyBlitTex");

#if UNITY_2021_1_OR_NEWER
        private static readonly GlobalKeyword _FOG_GRADIENT_ON = GlobalKeyword.Create("_FOG_GRADIENT_ON");
        private static readonly GlobalKeyword _FOG_GRADIENT_HSV_BLEND_ENABLED = GlobalKeyword.Create("_FOG_GRADIENT_HSV_BLEND_ENABLED");
#endif

        private ScriptableRenderer _renderer;

        public void Setup(RenderFeatureFogGradient.FogGradientSettings settings, ScriptableRenderer renderer)
        {
            this.settings = settings;
            this._renderer = renderer;

            if (settings.renderMode == RenderFeatureFogGradient.FogGradientRenderMode.MaterialsApplyFog)
            {
                renderPassEvent = RenderPassEvent.BeforeRendering;
                ConfigureInput(ScriptableRenderPassInput.Color);
            }
            else
            {
                renderPassEvent = settings.RenderAfterTransparents ? RenderPassEvent.AfterRenderingTransparents : RenderPassEvent.AfterRenderingSkybox;
                ConfigureInput(ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Depth);
            }
        }

        public void TriggerUpdateTexture()
        {
            _textureNeedsUpdate = true;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if(settings == null)
            {
                return;
            }

            if(!settings.enabled)
            {
                OnDisable();
                return;
            }

            // command start 
            var cmd = CommandBufferPool.Get(_profilerTag);
                cmd.Clear();

#if UNITY_EDITOR
            if (UnityEditor.SceneView.lastActiveSceneView != null)
            {
                var renderingCamera = renderingData.cameraData.camera;
                if(renderingCamera == UnityEditor.SceneView.lastActiveSceneView.camera)
                {
                    var fogDisabled = !UnityEditor.SceneView.lastActiveSceneView.sceneViewState.fogEnabled;
                    if (fogDisabled)
                    {
#if UNITY_2021_1_OR_NEWER
                        cmd.DisableKeyword(_FOG_GRADIENT_ON);
#else
                        cmd.DisableShaderKeyword("_FOG_GRADIENT_ON");
#endif

                        context.ExecuteCommandBuffer(cmd);
                        CommandBufferPool.Release(cmd);
                        return;
                    }
                }
            }
#endif

                        EnsureRT();

            if(settings.UpdateInternalTextureEveryFrame)
            {
                _textureNeedsUpdate = true;
            }

#if UNITY_EDITOR
            _textureNeedsUpdate = true;
#endif

            if(_textureNeedsUpdate)
            {
                _textureNeedsUpdate = false;
                UpdateGradientTexture(settings);
            }

            var volumeSettings = VolumeManager.instance.stack.GetComponent<FogGradientVolume>();

            // configure the global fog gradient texture 
            cmd.SetGlobalTexture(_FogGradientTexture, _gradientTexture);
            cmd.SetGlobalVector(_FogGradientData, new Vector4(volumeSettings.minDistance.value, volumeSettings.maxDistance.value, 
                volumeSettings.minHeightDistance.value, volumeSettings.maxHeightDistance.value));

#if UNITY_2022_1_OR_NEWER
            var cameraColorTargetHandle = _renderer.cameraColorTargetHandle;
            var cameraDepthTargetHandle = _renderer.cameraDepthTargetHandle;
#else
            var cameraColorTargetHandle = _renderer.cameraColorTarget;
            var cameraDepthTargetHandle = _renderer.cameraDepthTarget;
#endif

            if (settings.renderMode == RenderFeatureFogGradient.FogGradientRenderMode.PostProcessApplyFog)
            {
                var camera = renderingData.cameraData.camera;
                var projection = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);
                var inverseProjection = projection.inverse;

                cmd.SetGlobalMatrix("corgi_InverseProjection", inverseProjection);
                cmd.SetGlobalMatrix("corgi_CameraToWorld", camera.cameraToWorldMatrix);

                // grabpass 
                cmd.GetTemporaryRT(_CorgiGrabpass, renderingData.cameraData.cameraTargetDescriptor);
                cmd.SetGlobalTexture(_CopyBlitTex, cameraColorTargetHandle);
                cmd.SetRenderTarget(_CorgiGrabpass, 0, CubemapFace.Unknown, -1);
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, settings.data.GrabpassMaterial, 0, 0);

                // depth grabpass  (full res)
                if(settings.ManualDepthBlit)
                {
                    var colorTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
                    var depthTargetDescriptorFullRes = colorTargetDescriptor;
                    depthTargetDescriptorFullRes.colorFormat = RenderTextureFormat.RFloat;

                    cmd.EnableShaderKeyword("_FOG_GRADIENT_USE_CORGI_DEPTH_BLIT");
                    cmd.SetGlobalTexture(_CopyBlitTex, cameraDepthTargetHandle);
                    cmd.GetTemporaryRT(_CorgiDepthGrabpass, depthTargetDescriptorFullRes);
                    cmd.SetRenderTarget(_CorgiDepthGrabpass, 0, CubemapFace.Unknown, -1);
                    cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, settings.data.DepthGrabpassMaterial, 0, 0);
                    cmd.SetGlobalTexture(_CorgiDepthGrabpass, _CorgiDepthGrabpass);
                }
                else
                {
                    cmd.DisableShaderKeyword("_FOG_GRADIENT_USE_CORGI_DEPTH_BLIT");
                }

                // post process 
                cmd.SetRenderTarget(cameraColorTargetHandle, 0, CubemapFace.Unknown, -1);
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, settings.data.BlitMaterial, 0, 0);

                // if we're post processing, don't enable the fog gradient keywords
#if UNITY_2021_1_OR_NEWER
                cmd.DisableKeyword(_FOG_GRADIENT_ON);
#else
                cmd.DisableShaderKeyword("_FOG_GRADIENT_ON");
#endif
            }
            else
            {
                // if we're drawing fog per material, keep the keyword enabled 
#if UNITY_2021_1_OR_NEWER
                cmd.EnableKeyword(_FOG_GRADIENT_ON);
#else
                cmd.EnableShaderKeyword("_FOG_GRADIENT_ON");
#endif
            }

            if (settings.applyBlendMode == RenderFeatureFogGradient.FogGradientBlendMode.HSV_Blend)
            {
#if UNITY_2021_1_OR_NEWER
                cmd.EnableKeyword(_FOG_GRADIENT_HSV_BLEND_ENABLED);
#else
                cmd.EnableShaderKeyword("_FOG_GRADIENT_HSV_BLEND_ENABLED");
#endif
            }
            else
            {
#if UNITY_2021_1_OR_NEWER
                cmd.DisableKeyword(_FOG_GRADIENT_HSV_BLEND_ENABLED);
#else
                 cmd.DisableShaderKeyword("_FOG_GRADIENT_HSV_BLEND_ENABLED");
#endif
            }

            // finish up 
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        static private NativeArray<GradientColorKey> _distanceGradientColorKeys;
        static private NativeArray<GradientAlphaKey> _distanceGradientAlphaKeys;
        static private NativeArray<GradientColorKey> _heightGradientColorKeys;
        static private NativeArray<GradientAlphaKey> _heightGradientAlphaKeys;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void StaticDiposeInitialization()
        {
            if (_distanceGradientColorKeys.IsCreated)
            {
                _distanceGradientColorKeys.Dispose();
                _distanceGradientAlphaKeys.Dispose();
                _heightGradientColorKeys.Dispose();
                _heightGradientAlphaKeys.Dispose();
            }
        }

        public void EnsureRT()
        {
            if (_gradientTexture == null)
            {
                _gradientTexture = new Texture2D(settings.quality, settings.quality, TextureFormat.RGBAHalf, false, true);
                TriggerUpdateTexture(); 
            }

            if (!_distanceGradientColorKeys.IsCreated)
            {
                var start_size = 16;
                _distanceGradientColorKeys = new NativeArray<GradientColorKey>(start_size, Allocator.Persistent, NativeArrayOptions.ClearMemory);
                _distanceGradientAlphaKeys = new NativeArray<GradientAlphaKey>(start_size, Allocator.Persistent, NativeArrayOptions.ClearMemory);
                _heightGradientColorKeys = new NativeArray<GradientColorKey>(start_size, Allocator.Persistent, NativeArrayOptions.ClearMemory);
                _heightGradientAlphaKeys = new NativeArray<GradientAlphaKey>(start_size, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            }
        }

        private void UpdateGradientTexture(RenderFeatureFogGradient.FogGradientSettings settings)
        {
            var volumeSettings = VolumeManager.instance.stack.GetComponent<FogGradientVolume>();
            var pixelData = _gradientTexture.GetPixelData<half4>(0);

            if(!pixelData.IsCreated)
            {
                Debug.LogError("no pixel data?");
                return;
            }

            var volume = settings.quality * settings.quality;
            var distanceGradient = volumeSettings.colorGradientDistance.value;
            var heightGradient = volumeSettings.colorGradientHeight.value;
            if (_distanceGradientColorKeys.Length != distanceGradient.colorKeys.Length)
            {
                _distanceGradientColorKeys.Dispose();
                _distanceGradientColorKeys = new NativeArray<GradientColorKey>(distanceGradient.colorKeys.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            }

            if (_distanceGradientAlphaKeys.Length != distanceGradient.alphaKeys.Length)
            {
                _distanceGradientAlphaKeys.Dispose();
                _distanceGradientAlphaKeys = new NativeArray<GradientAlphaKey>(distanceGradient.alphaKeys.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            }

            if (_heightGradientColorKeys.Length != heightGradient.colorKeys.Length)
            {
                _heightGradientColorKeys.Dispose();
                _heightGradientColorKeys = new NativeArray<GradientColorKey>(heightGradient.colorKeys.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            }

            if (_heightGradientAlphaKeys.Length != heightGradient.alphaKeys.Length)
            {
                _heightGradientAlphaKeys.Dispose();
                _heightGradientAlphaKeys = new NativeArray<GradientAlphaKey>(heightGradient.alphaKeys.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            }

            _distanceGradientColorKeys.CopyFrom(distanceGradient.colorKeys);
            _distanceGradientAlphaKeys.CopyFrom(distanceGradient.alphaKeys);
            _heightGradientColorKeys.CopyFrom(heightGradient.colorKeys);
            _heightGradientAlphaKeys.CopyFrom(heightGradient.alphaKeys);

            var job = new GenerateFogTextureJob()
            {
                output = pixelData,
                volume = volume,
                quality = settings.quality,
                blendMode = settings.gradientBlendMode,

                distanceGradientColorKeys = _distanceGradientColorKeys.Slice(0, distanceGradient.colorKeys.Length),
                distanceGradientAlphaKeys = _distanceGradientAlphaKeys.Slice(0, distanceGradient.alphaKeys.Length),

                heightGradientColorKeys = _heightGradientColorKeys.Slice(0, heightGradient.colorKeys.Length),
                heightGradientAlphaKeys = _heightGradientAlphaKeys.Slice(0, heightGradient.alphaKeys.Length),
            };

            var handle = job.Schedule(volume, settings.quality);
                handle.Complete();

            _gradientTexture.Apply();
        }

        public void Dispose()
        {
            if(_gradientTexture != null)
            {
#if UNITY_EDITOR
                if(!Application.isPlaying)
                {
                    Texture2D.DestroyImmediate(_gradientTexture);
                }
                else
                {
                    Texture2D.Destroy(_gradientTexture);
                }
#else
                Texture2D.Destroy(_gradientTexture);
#endif

                _gradientTexture = null; 
            }

            if (_distanceGradientColorKeys.IsCreated)
            {
                _distanceGradientColorKeys.Dispose();
                _distanceGradientAlphaKeys.Dispose();
                _heightGradientColorKeys.Dispose();
                _heightGradientAlphaKeys.Dispose();
            }
        }

        public void OnDisable()
        {
            Dispose();


#if UNITY_2021_1_OR_NEWER
            Shader.DisableKeyword(_FOG_GRADIENT_ON); 
#else
            Shader.DisableKeyword("_FOG_GRADIENT_ON"); 
#endif
        }

        [BurstCompile] 
        private struct GenerateFogTextureJob : IJobParallelFor
        {
            [WriteOnly] public NativeArray<half4> output;

            [ReadOnly] public NativeSlice<GradientColorKey> distanceGradientColorKeys;
            [ReadOnly] public NativeSlice<GradientAlphaKey> distanceGradientAlphaKeys;

            [ReadOnly] public NativeSlice<GradientColorKey> heightGradientColorKeys;
            [ReadOnly] public NativeSlice<GradientAlphaKey> heightGradientAlphaKeys;

            [ReadOnly] public int quality;
            [ReadOnly] public int volume;
            [ReadOnly] public RenderFeatureFogGradient.FogGradientBlendMode blendMode;

            public static int2 Get1DTo2D(int index, int width)
            {
                var x = index % width;
                var y = index / width;

                var pos = new int2(x, y);

                return pos;
            }

            public void Execute(int index)
            {
                var pos = Get1DTo2D(index, quality);

                var height_t = (float) pos.y / quality;
                var distance_t = (float) pos.x / quality;
                var blend_t = height_t;

                var height_color = EvaluateGradientColor(height_t, heightGradientColorKeys);
                var distance_color = EvaluateGradientColor(distance_t, distanceGradientColorKeys);

                height_color.a = EvaluateGradientAlpha(height_t, heightGradientAlphaKeys);
                distance_color.a = EvaluateGradientAlpha(distance_t, distanceGradientAlphaKeys);

                Color blended_color;
                if(blendMode == RenderFeatureFogGradient.FogGradientBlendMode.RGB_Lerp)
                {
                    blended_color = Color.Lerp(distance_color, height_color, blend_t);
                }
                else
                {
                    blended_color = FogGradientHSVBlend.BlendHSVSpace(distance_color, height_color, blend_t);
                }

                var result = new float4(blended_color.r, blended_color.g, blended_color.b, blended_color.a);
                output[index] = (half4) result;
            }

            // reworked this to work with hsv blends too: https://answers.unity.com/questions/1703579/how-does-gradientevaluate-work-internally.html
            private Color EvaluateGradientColor(float t, NativeSlice<GradientColorKey> keys)
            {
                if (t <= keys[0].time)
                {
                    return keys[0].color;
                }

                if (t >= keys[keys.Length - 1].time)
                {
                    return keys[keys.Length - 1].color;
                }

                for (int i = 0; i < keys.Length - 1; i++)
                {
                    var startKey = keys[i];
                    var endKey = keys[i + 1];

                    if (t < endKey.time)
                    {
                        var inner_t = Mathf.Clamp01((t - startKey.time) / (endKey.time - startKey.time));

                        if(blendMode == RenderFeatureFogGradient.FogGradientBlendMode.RGB_Lerp)
                        {
                            return Color.Lerp(startKey.color, endKey.color, inner_t);
                        }
                        else
                        {
                            return FogGradientHSVBlend.BlendHSVSpace(startKey.color, endKey.color, inner_t);
                        }
                    }
                }

                return keys[keys.Length - 1].color;
            }

            private float EvaluateGradientAlpha(float t, NativeSlice<GradientAlphaKey> keys)
            {
                if (t <= keys[0].time)
                {
                    return keys[0].alpha;
                }

                if (t >= keys[keys.Length - 1].time)
                {
                    return keys[keys.Length - 1].alpha;
                }

                for (int i = 0; i < keys.Length - 1; i++)
                {
                    var startKey = keys[i];
                    var endKey = keys[i + 1];

                    if (t < endKey.time)
                    {
                        var inner_t = Mathf.Clamp01((t - startKey.time) / (endKey.time - startKey.time));
                        return math.lerp(startKey.alpha, endKey.alpha, inner_t);
                    }
                }

                return keys[keys.Length - 1].alpha;
            }
        }
    }
}
