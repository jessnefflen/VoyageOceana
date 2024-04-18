using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidOrigin : MonoBehaviour
{
    [SerializeField] public GameObject entityPrefab;
    [SerializeField] public int numEntities;
    [SerializeField] public int maxDistance = 5;
    [SerializeField] public float speed = 1f;


    public GameObject[] allEntities;
    public Vector3 center;
    private GameObject obstacle;
    
    void Start()
    {
        center = GetComponent<Transform>().position;
        //obstacle = GameObject.Find("CubeOb");

        allEntities = new GameObject[numEntities];

        for ( int i = 0; i < numEntities; i++) {
            Vector3 pos = new Vector3(center.x + Random.Range(-maxDistance, maxDistance),
                center.y + Random.Range(0, maxDistance * 2),
                center.z + Random.Range(-maxDistance, maxDistance));

            allEntities[i] = Instantiate(entityPrefab, pos, Quaternion.identity, transform);
        }
    }

    void Update()
    {
        if (Random.Range(0,100) < 1) {
            center = new Vector3(transform.position.x + Random.Range(-maxDistance, maxDistance),
                transform.position.y + Random.Range(0, maxDistance * 2),
                transform.position.z + Random.Range(-maxDistance, maxDistance));

            /**while (Vector2.Distance(new Vector2(center.x, center.z), new Vector2(obstacle.transform.position.x, obstacle.transform.position.y)) < 2)
            {
                center = new Vector3(transform.position.x + Random.Range(-maxDistance, maxDistance),
                transform.position.y + Random.Range(0, maxDistance * 2),
                transform.position.z + Random.Range(-maxDistance, maxDistance));
            }**/
        }
    }
}
