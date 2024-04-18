using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayClip : MonoBehaviour
{
    private AudioSource message;
    void Awake()
    {
        message = gameObject.GetComponent<AudioSource>();
    }

    void OnEnable() {
        message.Play();
    }
}
