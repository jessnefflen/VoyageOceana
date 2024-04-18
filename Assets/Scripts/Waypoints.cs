using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class Waypoints : MonoBehaviour
{
    [SerializeField]private Transform lastWayPoint;

    public Transform LastWayPoint {
        get { return lastWayPoint; }
    }
    private void OnDrawGizmos() {
        foreach(Transform t in transform){
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(t.position, 1);
        }

        Gizmos.color = Color.red;

        for(int i = 0; i < transform.childCount - 1; i++) {
            Gizmos.DrawLine(transform.GetChild(i).position, transform.GetChild(i + 1).position);
        }
    }
    void Start() {
        lastWayPoint = transform.GetChild(transform.childCount-1);
    }

    public Transform GetNextWayPoint(Transform currentWP) {
        if(currentWP == null) {
            return transform.GetChild(0);
        }

        if(currentWP.GetSiblingIndex() < transform.childCount - 1) {
            return transform.GetChild(currentWP.GetSiblingIndex() + 1);
        } else {
            return null;
        }

        //return null;
    }


}
