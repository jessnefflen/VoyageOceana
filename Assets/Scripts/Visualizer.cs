using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Visualizer : MonoBehaviour
{
    public RectTransform[] visualizerObjects;
    public AudioSource audioInput;
    public float minHeight = 10f;
    public float maxHeight = 100f;


    [Range(64,8192)]
    public int visualizerSamples = 64;
    public float smoothing = 0.05f;
    void Start()
    {
        if (audioInput == null) {
            audioInput = GetComponent<AudioSource>(); 
        }
        
        RectTransform[] tempObjects = GetComponentsInChildren<RectTransform>();

        visualizerObjects = new RectTransform[tempObjects.Length-1];

        for (int i = 0; i < visualizerObjects.Length; i++) {
            visualizerObjects[i] = tempObjects[i+1];
        }
    }

    
    void Update()
    {
        float[] samples = new float[visualizerSamples];
        int channel = 0;

        audioInput.GetOutputData(samples, channel); 

        for (int i = 0; i <visualizerObjects.Length; i++) {
            Vector2 newSize = visualizerObjects[i].rect.size;

            newSize.y = Mathf.Lerp(newSize.y, minHeight + (samples[i] * (maxHeight - minHeight) *4.0f), smoothing);

            visualizerObjects[i].sizeDelta = newSize;
        }
    }
}
