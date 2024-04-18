using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Parrotfish : MonoBehaviour, ICreature
{
    public Animator anim;
    public bool spooked = false;
    public bool pecking = false;
    private Rigidbody rb;
    // Start is called before the first frame update

    void Start() {
        rb = GetComponent<Rigidbody>();
    }

    public void GetSpooked() {
        spooked = true;
        if (GetComponent<WaypointMover>().enabled == false) {
            anim.SetTrigger("Spooked");
            WaypointMover mover = GetComponent<WaypointMover>();
            mover.enabled = true;
        }
    }

    public void Swim() {
        
    }

    public void Idle() {

        if (Random.Range(0,100) < 1 && !pecking) {
                anim.SetTrigger("Peck");
                pecking = true;
                StartCoroutine(Pecking());
            }
    }

IEnumerator Pecking() {
    float timer = 0.0f;
    Vector3 start = transform.position;
    Vector3 end = start + new Vector3( 0.0f, 0.0f, 1f);
    float dist = Vector3.Distance(start, end);

    //yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("PFIdle"));

    //pecking = false;

    yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("PFPeck"));

    while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.1f/**timer <= 0.21f**/) {
        timer += Time.deltaTime;
        yield return null;
    }

    while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.35f/**timer <= 0.9f**/) {
        timer += Time.deltaTime;
        rb.AddRelativeForce(transform.forward * 0.4f);
        yield return null;
    }

    while(anim.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.6f/**timer <= 2.05f**/) {
        timer += Time.deltaTime;
        yield return null;
    }

    while (Vector3.Distance(start, transform.position) >= 0.01f) {
        rb.velocity = (start - transform.position).normalized * 0.35f;
        timer += Time.deltaTime;
        yield return null;
    }
    
    rb.velocity = Vector3.zero;
    rb.MovePosition(start);

    pecking = false;

    //rb.velocity = (start - transform.position).normalized * 0.3f;
    //yield return new WaitUntil(() => Vector3.Distance(start, transform.position) >= 0.1f);
    //rb.velocity = Vector3.zero;


    /**yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("PFPeck")
        && !anim.IsInTransition(0));

    while (timer <= 0.75f) {
        timer += Time.deltaTime;
        yield return null;
    }

    timer = 0.0f;
    while (timer <= 1.0f) {
        timer += Time.deltaTime;
        transform.position = Vector3.Lerp(transform.position, end, timer/1.0f);
        yield return null;
    }

    yield return new WaitForSeconds(0.86f);
    timer += 0.86f;

    while (timer > 2.05f && timer <= 3.17f) {
        timer += Time.deltaTime;
        transform.position = Vector3.Lerp(transform.position, start, dist/1.11f);
        yield return null;
    }

    pecking = false;**/
}

    public void Update() {
        if (!spooked) {
            Idle();
        }
        if (spooked && pecking) {
            StopAllCoroutines();
            GetSpooked();
        } else if (spooked) {
            GetSpooked();
        }
    }
}
