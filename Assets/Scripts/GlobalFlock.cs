using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalFlock : MonoBehaviour
{
    [SerializeField] public GameObject fishPrefab;
    [SerializeField] public int numFish;
    [SerializeField] public int tankSize = 5;
    [SerializeField] public float flockSpeed = 1f;

    [SerializeField] public float xLowerLimit = -20;
    [SerializeField] public float xUpperLimit = 60;

    [SerializeField] public float zLowerLimit = -20;
    [SerializeField] public float zUpperLimit = 60;


    private Vector3 center;
    public Vector3 goalPos;
    public GameObject[] allFish;
    private Vector3 root;
    private bool turning = false;
    private bool moving = true;
    void Start()
    {
        root = GetComponent<Transform>().position;
        goalPos = root;
        allFish = new GameObject[numFish];
        center = new Vector3(xUpperLimit - xLowerLimit, transform.position.y,
            zUpperLimit - zLowerLimit);

        for(int i = 0; i < numFish; i++)
        {
            Vector3 pos = new Vector3(root.x + Random.Range(-tankSize, tankSize),
                root.y + Random.Range(0, tankSize * 2), 
                root.z + Random.Range(-tankSize, tankSize));

            allFish[i] =
            Instantiate(fishPrefab, pos, Quaternion.identity);
    
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Random.Range(0,1000) < 50)
        {
            goalPos = new Vector3(transform.position.x + Random.Range(-tankSize, tankSize),
                transform.position.y + Random.Range(0, 2*tankSize), 
                transform.position.z + Random.Range(-tankSize, tankSize));
        }

        if (Random.Range(0,100000) < 50)
        {
            Vector3 direction = new Vector3(Random.Range(xLowerLimit, xUpperLimit), 0,
                Random.Range(zLowerLimit, zUpperLimit));
            transform.rotation = 
                Quaternion.LookRotation(direction);
        }


        if (transform.position.z <= zLowerLimit || transform.position.z >= zUpperLimit 
            || transform.position.x <= xLowerLimit
            ||transform.position.x >= xUpperLimit)
           
        {
            turning = true;
        }
        else
        {
            turning = false;
        }

        if (turning)
        {
            Vector3 direction = center - transform.position;
            transform.rotation =
                Quaternion.LookRotation(direction);
                

        }
        if (Random.Range(0,100000) < 50)
        {
            moving = !moving;
        }

        if (moving) {
            transform.Translate(0, 0, Time.deltaTime * flockSpeed);
        } else
        {
            if (transform.position.y != 12.38)
            {
                transform.position.Set(transform.position.x, 12.38f, transform.position.z);
            }
        }
    }
}
