using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class NextCreature : MonoBehaviour
{
    public RandomCreatures creaturesList;
    
    public GameObject moreCreatures;

    public GameObject noMoreCreatures;

    public float clipBuffer = 0.5f;

    public float duration;

    private AudioSource messageClip;

    void Awake() {
        messageClip = gameObject.GetComponent<AudioSource>();
    }
    void OnEnable() {
        duration = messageClip.clip.length + clipBuffer;

        if (GameManager.Instance.TrackedCreature() == null) {
            StartCoroutine(NextMessage(noMoreCreatures));
        }
        else
            StartCoroutine(NextMessage(moreCreatures));
    }

    IEnumerator NextMessage(GameObject message) {
        messageClip.Play();
        yield return new WaitForSeconds(duration);

        this.gameObject.SetActive(false);
        message.SetActive(true);
    }
}
