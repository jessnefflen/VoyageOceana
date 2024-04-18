using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndMessage : MonoBehaviour
{
    public float duration;

    void OnEnable() {

    }

    IEnumerator End() {
        yield return new WaitForSeconds(duration);

        Application.Quit();
    }
    
}
