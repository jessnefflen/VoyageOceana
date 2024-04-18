using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dart : MonoBehaviour
{
    public float lifespan = 3.0f;
    public float gravityChange = 8.0f;
    private Rigidbody rb;
    private Shooter shooter;
    private bool success;
    private GameObject CreatureMessage;
   
   void Awake() {
    Destroy(gameObject, lifespan);
    shooter = gameObject.GetComponentInParent<Shooter>();
    rb = gameObject.GetComponent<Rigidbody>();
    success = false;
   }

   void FixedUpdate() {
        rb.AddForce(transform.up * gravityChange, ForceMode.Acceleration);
   }

   void OnCollisionEnter(Collision collision) {
        if (collision.gameObject == GameManager.Instance.TrackedCreature()) {
            success = true;
            Destroy(gameObject);
            //collision.gameObject.GetComponent<CreatureController>().GetSpooked();
        } 
        else if (!collision.gameObject.CompareTag("Player"))
            //shooter.Failure();
            Destroy(gameObject);
        
   }

   void OnDestroy() {
        if (success)
            shooter.Success();
        else
            shooter.Failure();
   }
}
