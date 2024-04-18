using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureController : MonoBehaviour
{
    public Animator anim;
    private bool spooked = false;
    
    void Start()
    {
    }

    public void GetSpooked() {
        anim.SetTrigger("Spooked");
    }
   
    // Update is called once per frame
    void Update()
    {
        if (Random.Range(0,1000) < 1) {
                anim.SetTrigger("Peck");
            }
    }
}
