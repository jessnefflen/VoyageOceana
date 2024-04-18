using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
 
[RequireComponent(typeof(Rigidbody))]
public class WaypointMover : MonoBehaviour
{
    [SerializeField]
    private Waypoints waypoints;

    [SerializeField] private Waypoints[] paths;
    [SerializeField]
    private float speed = 5.0f;
    [SerializeField]
    private float rotationSpeed = 1.0f;

    [SerializeField]
    private float distThreshold = 0.1f;

    public Transform currentWP;
    public Transform nextWP;

    private Rigidbody rb;

    void OnEnable()
    {   rb = GetComponent<Rigidbody>(); 
        waypoints = PickPath();
        currentWP = waypoints.GetNextWayPoint(currentWP);
        //currentWP.position = transform.position;

        currentWP = waypoints.GetNextWayPoint(currentWP);
        nextWP = waypoints.GetNextWayPoint(currentWP);
        //transform.rotation = Quaternion.LookRotation(transform.position - currentWP.position);
    }

    private Waypoints PickPath() {
        Dictionary<Waypoints, float> valuedPaths = new Dictionary<Waypoints, float>();
        GameObject player = GameManager.Instance.Player;
        Waypoints chosenPath = null;

        foreach (Waypoints path in paths) {
            Vector3 heading = Vector3.Normalize(player.transform.position - path.LastWayPoint.position);
            float dot = Vector3.Dot(heading, player.transform.forward);

            valuedPaths.Add(path, dot);
        }
        
        foreach(Waypoints path in valuedPaths.Keys) {
            if (chosenPath == null) {
                chosenPath = path;
            } else {
                if (Mathf.Abs(valuedPaths[path]) < Mathf.Abs(valuedPaths[chosenPath])) {
                    chosenPath = path;
                }
            }
            
        }

        return chosenPath;
    }

    // Update is called once per frame
    void Update()
    {
        /**
        if (nextWP != null && Vector3.Distance(transform.position, currentWP.position) < 2.0f) {
            //var TargetRotation = Quaternion.LookRotation(transform.position - nextWP.position);
            var TargetRotation = Quaternion.LookRotation(nextWP.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation,TargetRotation, rotationSpeed * Time.deltaTime);
        } 
        else {
        //var TargetRotation = Quaternion.LookRotation(transform.position - currentWP.position);
        var TargetRotation = Quaternion.LookRotation(currentWP.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation,TargetRotation, rotationSpeed * Time.deltaTime);
        }
        
        transform.position = Vector3.MoveTowards(transform.position, currentWP.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, currentWP.position) < distThreshold) {
            //Transform tmpWP = waypoints.GetNextWayPoint(currentWP);

            if (nextWP != null) {
                currentWP = nextWP;
                nextWP= waypoints.GetNextWayPoint(currentWP);
                //transform.rotation = Quaternion.LookRotation(transform.position - currentWP.position);
            } 
            else {
                gameObject.SetActive(false);
            }
        }**/
        
    }

    void FixedUpdate() {
        var lookQ = Quaternion.LookRotation(currentWP.position - transform.position);
        lookQ = Quaternion.Slerp(transform.rotation, lookQ, rotationSpeed* Time.deltaTime);
        rb.MoveRotation(lookQ);

        rb.AddForce(transform.forward * speed);

        if (Vector3.Distance(transform.position, currentWP.position) < distThreshold) {
            //Transform tmpWP = waypoints.GetNextWayPoint(currentWP);

            if (nextWP != null) {
                currentWP = nextWP;
                nextWP= waypoints.GetNextWayPoint(currentWP);
                //transform.rotation = Quaternion.LookRotation(transform.position - currentWP.position);
            } 
            else {
                gameObject.SetActive(false);
            }
        }
    }
}
