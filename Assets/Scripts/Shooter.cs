using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    public Transform dartSpawnPos;
    public GameObject dartPrefab;
    public float dartSpeed = 10f;
    public int numDarts = 3;

    //public GameObject successMessage;
    //public GameObject failureMessage;

    private int remainingDarts;
    private bool holding;


    void Start() {
        remainingDarts = numDarts;
        holding = false;
    }

    public void Shoot() {
        if (remainingDarts > 0 && !holding) {
            holding = true;
            remainingDarts --;

            Debug.Log("Shooting");

            var dart = Instantiate(dartPrefab, this.transform);
            dart.GetComponent<Rigidbody>().velocity = dartSpawnPos.forward * dartSpeed;
        }
    }

    public int DartsRemaining() {
        return remainingDarts;
    }

    public void Success() {
        Debug.Log("Success");
        StartCoroutine(NextMessage(true));
    }

    public void Failure() {
        Debug.Log("Failure");

        if (remainingDarts <= 0) {
            StartCoroutine(NextMessage(false));
        } 
        else
            holding = false;
    }

    IEnumerator NextMessage(bool succeed) {

        yield return new WaitForSeconds(2.0f);

        //GameObject creature = GameObject.FindGameObjectWithTag("Tracked Creature");
        //creature.tag = "Tagged Creature";

        GameObject creature = GameManager.Instance.TaggedCreature();
        
        GameObject message = creature.GetComponent<CreatureToTrack>().failureMessage;

        if (succeed)
            message = creature.GetComponent<CreatureToTrack>().successMessage;

        GameObject[] sailSayings = GameObject.FindGameObjectsWithTag("S.A.I.L");

        foreach (GameObject saying in sailSayings) {
            saying.SetActive(false);
        }

        holding = false;
        message.SetActive(true);
        remainingDarts = numDarts;

        yield return null;
    }
    
}
