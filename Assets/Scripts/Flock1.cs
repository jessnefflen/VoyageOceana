using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock1 : MonoBehaviour
{
    [SerializeField] public float speed = 1f;
    [SerializeField] public float rotationSpeed = 4.0f;
    [SerializeField] public GameObject flockOrigin;

    private Vector3 averageHeading;
    private Vector3 averagePosition;
    private float neighbourDistance = 3.0f;
    private bool turning = false;
    void Start()
    {
        flockOrigin = GameObject.Find("Flock1");
        speed += Random.Range(0f, 0.3f);
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, flockOrigin.transform.position)
            >= flockOrigin.GetComponent<GlobalFlock>().tankSize)
        {
            turning = true;
        } else
        {
            turning = false;
        }

        if (turning)
        {
            Vector3 direction = flockOrigin.transform.position - transform.position;
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(direction), 
                rotationSpeed * Time.deltaTime);

            speed = Random.Range(1.0f, 2f);
        }
        else if(Random.Range(0, 5) < 1)
        {
            ApplyRules();
        }

        transform.Translate(0, 0, Time.deltaTime * speed);
    }

    void ApplyRules()
    {
        GameObject[] flock = flockOrigin.GetComponent<GlobalFlock>().allFish;
        Vector3 vcenter = Vector3.zero;
        Vector3 vavoid = Vector3.zero;

        float gspeed = 0.1f;

        int groupSize = 0;
        float dist;

        foreach (GameObject fish in flock)
        {
            if (fish != this.gameObject)
            {
                dist = Vector3.Distance(fish.transform.position, this.transform.position);
                if (dist <= neighbourDistance)
                {
                    vcenter += fish.transform.position;
                    groupSize++;

                    if (dist < 0.5f)
                    {
                        vavoid += (this.transform.position - fish.transform.position);
                    }

                    Flock1 anotherFlock = fish.GetComponent<Flock1>();
                    gspeed = gspeed + anotherFlock.speed;
                }
            }
        }

        if (groupSize > 0)
        {
            vcenter = vcenter / groupSize +
                (flockOrigin.GetComponent<GlobalFlock>().goalPos - this.transform.position);

            speed = gspeed/groupSize;

            Debug.Log(speed);

            Vector3 direction = (vcenter + vavoid) - transform.position;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
            }
        }

        
    }
}
