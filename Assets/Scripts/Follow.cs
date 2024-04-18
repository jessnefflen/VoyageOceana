using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    public GameObject leader;

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.position = leader.transform.position;
    }
}
