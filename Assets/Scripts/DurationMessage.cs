using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DurationMessage : MonoBehaviour
{
    [SerializeField] GameObject nextMessage;
    [SerializeField] float duration;
    [SerializeField] bool[] actionRestrict = new bool[4];

    [SerializeField] float clipBuffer = 0.5f;

    private AudioSource message;
    

    void OnEnable() {
        message = gameObject.GetComponent<AudioSource>();
        if (message != null)
            duration = message.clip.length + clipBuffer;

        //GameObject[] sailSayings = GameObject.FindGameObjectsWithTag("S.A.I.L");

        StartCoroutine(WaitForNext());
    }
    
    IEnumerator WaitForNext() {
        if (message != null)
            message.Play();

        yield return new WaitForSeconds(duration);

        Dictionary<string, bool> activeActions = GameObject.FindGameObjectWithTag("Player").GetComponent<OceanaController>().activeActions;

        activeActions["Thrust"] = actionRestrict[0];
        activeActions["Pitch"] = actionRestrict[1];
        activeActions["Yaw"] = actionRestrict[2];
        activeActions["Shoot"] = actionRestrict[3];

        //this.gameObject.SetActive(false);
        //nextMessage.SetActive(true);
        GameManager.Instance.ChangeUI(nextMessage);

        yield return null;

    }

}
