using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CorgiFogGradient
{
    // [CreateAssetMenu(fileName = "NewRenderDataFogGradient", menuName = "RenderDataFogGradient")]
    public class RenderDataFogGradient : ScriptableObject
    {
        public Material BlitMaterial;
        public Material GrabpassMaterial;
        public Material DepthGrabpassMaterial;

#if UNITY_EDITOR
        public static RenderDataFogGradient FindData()
        {
            var guids = UnityEditor.AssetDatabase.FindAssets("t:RenderDataFogGradient");
            foreach(var guid in guids)
            {
                if (string.IsNullOrEmpty(guid)) continue;
                var assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(assetPath)) continue;
                var data = UnityEditor.AssetDatabase.LoadAssetAtPath<RenderDataFogGradient>(assetPath);
                if (data != null) return data; 
            }

            var newData = RenderDataFogGradient.CreateInstance<RenderDataFogGradient>();
            
            UnityEditor.AssetDatabase.CreateAsset(newData, "CorgiFogGradientData.asset");

            Debug.Log("RenderDataFogGradient not found, so one was created.", newData);

            return newData;
        }
#endif

    }
}