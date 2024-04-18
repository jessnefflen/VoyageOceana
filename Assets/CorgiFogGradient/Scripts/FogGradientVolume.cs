using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CorgiFogGradient
{
    [Serializable]
    public class GradientParameter : VolumeParameter<Gradient>
    {
        public GradientParameter(Gradient value, bool overrideState = false) : base(value, overrideState)
        {

        }

        public override void Interp(Gradient from, Gradient to, float t)
        {
            // combine 
            var colorKeys = new List<GradientColorKey>(from.colorKeys.Length + to.colorKeys.Length);
            var alphaKeys = new List<GradientAlphaKey>(from.alphaKeys.Length + to.alphaKeys.Length);
            
            for(var c = 0; c < from.colorKeys.Length; ++c)
            {
                var colorKey = from.colorKeys[c];

                var keyTime = colorKey.time;
                var fromValue = from.Evaluate(keyTime);
                var toValue = to.Evaluate(keyTime);
                colorKey.color = Color.Lerp(fromValue, toValue, t);

                colorKeys.Add(colorKey);
            }

            for (var c = 0; c < to.colorKeys.Length; ++c)
            {
                var colorKey = to.colorKeys[c];

                var keyTime = colorKey.time;
                var fromValue = from.Evaluate(keyTime);
                var toValue = to.Evaluate(keyTime);
                colorKey.color = Color.Lerp(fromValue, toValue, t);

                colorKeys.Add(colorKey); 
            }

            for (var c = 0; c < from.alphaKeys.Length; ++c)
            {
                var alphaKey = from.alphaKeys[c];

                var keyTime = alphaKey.time;
                var fromValue = from.Evaluate(keyTime);
                var toValue = to.Evaluate(keyTime);
                alphaKey.alpha = Mathf.Lerp(fromValue.a, toValue.a, t);

                alphaKeys.Add(alphaKey); 
            }

            for (var c = 0; c < to.alphaKeys.Length; ++c)
            {
                var alphaKey = to.alphaKeys[c];

                var keyTime = alphaKey.time;
                var fromValue = from.Evaluate(keyTime);
                var toValue = to.Evaluate(keyTime);
                alphaKey.alpha = Mathf.Lerp(fromValue.a, toValue.a, t);

                alphaKeys.Add(alphaKey); 
            }

            // reduce to fit in gradient limits 
            // note: things won't be quite right. just ensure blended ones never get over 8 
            while(colorKeys.Count > 8)
            {
                colorKeys.RemoveAt(colorKeys.Count - 2);
            }

            while (alphaKeys.Count > 8)
            {
                alphaKeys.RemoveAt(alphaKeys.Count - 2);
            }

            colorKeys.Sort((a, b) => a.time.CompareTo(b.time));
            alphaKeys.Sort((a, b) => a.time.CompareTo(b.time));

            // reconstruct 
            var result = new Gradient()
            {
                colorKeys = colorKeys.ToArray(),
                alphaKeys = alphaKeys.ToArray(),
                mode = GradientMode.Blend,
            };

            m_Value = result;
        }
    }

    // [VolumeComponentMenuForRenderPipeline("CorgiFog/Fog Gradient", typeof(UniversalRenderPipeline))]
    [Serializable]
    public class FogGradientVolume : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("Distance from camera for fog color gradient. Alpha controls intensity of fog.")]
        public GradientParameter colorGradientDistance = new GradientParameter(new Gradient()
        {
            alphaKeys = new GradientAlphaKey[]
             {
                  new GradientAlphaKey(0, 0),
                  new GradientAlphaKey(1, 1),
             },
            colorKeys = new GradientColorKey[]
             {
                 new GradientColorKey(Color.white, 0f),
                 new GradientColorKey(Color.white, 1f),
             }
        });

        [Tooltip("Height difference (absolute value) from camera for fog color gradient. Alpha controls intensity of fog.")]
        public GradientParameter colorGradientHeight = new GradientParameter(new Gradient()
        {
             alphaKeys = new GradientAlphaKey[]
             {
                  new GradientAlphaKey(0, 0),
                  new GradientAlphaKey(0, 1),
             },
             colorKeys = new GradientColorKey[]
             {
                 new GradientColorKey(Color.white, 0f),
                 new GradientColorKey(Color.white, 1f),
             }
        });

        [Tooltip("Minimum distance from camera before fog begins.")] 
        public FloatParameter minDistance = new FloatParameter(0f);

        [Tooltip("Maximum distance from camera until fog caps out.")] 
        public FloatParameter maxDistance = new FloatParameter(1000f);

        [Tooltip("Minimum height difference (absolute value) from camera before fog begins.")] 
        public FloatParameter minHeightDistance = new FloatParameter(0f);

        [Tooltip("Maximum height difference (absolute value) from camera until fog caps out.")] 
        public FloatParameter maxHeightDistance = new FloatParameter(1000f);

        public bool IsActive()
        {
            return true;
        }

        public bool IsTileCompatible()
        {
            return true;
        }
    }
}