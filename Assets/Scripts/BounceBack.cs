using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceBack : MonoBehaviour
{
    [SerializeField] string playerTag;
    [SerializeField] float bounceForce;
    private float thrustPause = 1.0f;
    
    private void OnCollisionEnter(Collision collision){
        if (collision.transform.tag == playerTag) {
            /**Rigidbody rb = collision.rigidbody;

            rb.freezeRotation = true;

            rb.AddExplosionForce(bounceForce, collision.contacts[0].point, 5);

            rb.freezeRotation = false;**/
            StartCoroutine(Bounce(collision));
        }
    }

     IEnumerator Bounce(Collision collision) {
        Dictionary<string, bool> activeActions = collision.gameObject.GetComponent<OceanaController>().activeActions;
        Rigidbody rb = collision.rigidbody;

        rb.freezeRotation = true;
        rb.AddExplosionForce(bounceForce, collision.contacts[0].point, 5);
        activeActions["Thrust"] = false;
        yield return new WaitForSeconds(thrustPause);

        rb.freezeRotation = false;
        activeActions["Thrust"] = true;
        yield return null;
    }
}
