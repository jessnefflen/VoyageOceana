using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wait : MonoBehaviour
{
    public float duration;
    public GameObject next;
   void OnEnable() {
        StartCoroutine(WaitForNext());
    }
    
    IEnumerator WaitForNext() {

        yield return new WaitForSeconds(duration);

       

        this.gameObject.SetActive(false);
        next.SetActive(true);

        yield return null;

    }
}
