using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureToTrack : MonoBehaviour
{
    public GameObject colliders;
    public GameObject successMessage;
    public GameObject failureMessage;

    public ICreature controller;

    public void TrackCreature() {
        if (colliders != null && !colliders.activeSelf)
            colliders.SetActive(true);
        //this.gameObject.tag = "Tracked Creature";
    }

    public void TagCreature() {
        controller.GetSpooked();
    }

    public void GetSpooked() {
        if (controller != null)
            controller.GetSpooked();
    }

    void Start() {
        controller = gameObject.GetComponent<ICreature>();
    }

    // Update is called once per frame
    /**void Update()
    {
        if (this.gameObject.CompareTag("Tagged Creature")) {
            colliders.SetActive(false);
        }
    }**/
}
