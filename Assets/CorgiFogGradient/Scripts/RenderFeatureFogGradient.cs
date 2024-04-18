using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CorgiFogGradient
{
    public class RenderFeatureFogGradient : ScriptableRendererFeature
    {
        [System.Serializable]
        public enum FogGradientRenderMode
        {
            PostProcessApplyFog = 0,
            MaterialsApplyFog   = 1,
        }

        [System.Serializable]
        public enum FogGradientBlendMode
        {
            HSV_Blend = 0,
            RGB_Lerp  = 1,
        }

        [System.Serializable]
        public class FogGradientSettings
        {
            public bool enabled = true;

            [Tooltip("Controls the size of the generated rendertexture for the fog. Increase if quality is not up to par for the level of detail you need.")] 
            [Range(32, 512)] public int quality = 64;

            [Tooltip("Render mode for our RenderPass. PostProcessApplyFog: applies fog as a post process pass. MaterialsApplyFog: assumes your shaders are applying the fog from the _FogGradientTexture.")] 
            public FogGradientRenderMode renderMode = FogGradientRenderMode.PostProcessApplyFog;

            [Tooltip("Color blend mode applying the fog color. HSV blending can give you more of a dream-y look, but it's more expensive to compute on the GPU and it may not be what you want.")] 
            public FogGradientBlendMode applyBlendMode = FogGradientBlendMode.RGB_Lerp;

            [Tooltip("Color blend mode for fog gradient to texture conversion. HSV blending usually looks nicer, but it's more expensive to compute on the CPU and may not look like what you expect. RGB blending is more of a 'what you see is what you get' in the gradient view within Unity.")]
            public FogGradientBlendMode gradientBlendMode = FogGradientBlendMode.HSV_Blend;

            [Tooltip("(only for PostProcessApplyFog) If true, the effect will render after transparent things have been rendered. If false, it will render after opaques and the skybox have been rendered.")]
            public bool RenderAfterTransparents;

            [Tooltip("(only for builds) Update the internal render texture for the fog gradients every frame. In the editor, this is always true.")]
            public bool UpdateInternalTextureEveryFrame;

            [Tooltip("Instead of letting unity handle the depth blit, do one ourselves right after the opaque render pass. This may be necessary for compatibility with some SRP configs.")]
            public bool ManualDepthBlit;

            public RenderDataFogGradient data; 
        }

        public FogGradientSettings settings = new FogGradientSettings();
        private RenderPassFogGradient _renderPass;

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
#if UNITY_EDITOR
            if (settings.data == null)
            {
                settings.data = RenderDataFogGradient.FindData();
            }
#endif
            
            _renderPass.Setup(settings, renderer);
            
            renderer.EnqueuePass(_renderPass);
        }

        public override void Create()
        {
            _renderPass = new RenderPassFogGradient();

#if UNITY_EDITOR
            if (settings.data == null)
            {
                settings.data = RenderDataFogGradient.FindData();
            }
#endif

        }

        private static List<RenderFeatureFogGradient> _activeGradientRenderFeatures = new List<RenderFeatureFogGradient>();

        private void OnEnable()
        {
            _activeGradientRenderFeatures.Add(this);
        }

        private void OnDisable()
        {
            _activeGradientRenderFeatures.Remove(this);

            if (_renderPass != null)
            {
                _renderPass.OnDisable(); 
            }
        }

        private void OnValidate()
        {
            // force it to re-create the texture when any settings change 
            if(_renderPass != null)
            {
                _renderPass.Dispose();
            }
        }

        /// <summary>
        /// Call this function to trigger a texture refresh for all fog gradient render feature's render passes. 
        /// If you are not automatically updating the texture every frame, you can call this to trigger texture updates. 
        /// </summary>
        public static void TriggerTextureRefreshOnAllGradients()
        {
            foreach(var feature in _activeGradientRenderFeatures)
            {
                feature._renderPass.TriggerUpdateTexture(); 
            }
        }
    }
}
