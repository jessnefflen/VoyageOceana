using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidEntity : MonoBehaviour
{
    [SerializeField] public float speed = 1f;
    [SerializeField] public float rotationSpeed = 4.0f;
    [SerializeField] public GameObject origin;
    [SerializeField] public float avoidDist = 1.5f;
    [SerializeField] public float neighborDist = 3.0f;

    private GameObject obstacle;


    void Start()
    {
        //origin = transform.parent.gameObject;
        //obstacle = GameObject.Find("CubeOb");

    }

    void BoidRules()
    {
        GameObject[] entities = origin.GetComponent<BoidOrigin>().allEntities;
        Vector3 direction = Vector3.zero;
        Vector3 avoidForce = Vector3.zero;
        Vector3 groupCenter = Vector3.zero;
        
        float distance;
        //float groupSpeed = 0f;
        int neighborEntities = 0;

        foreach (GameObject entity in entities)
        {
            if (entity != this.gameObject)
            {
                distance = Vector3.Distance(entity.transform.position, this.transform.position);

                if (distance <= neighborDist)
                {
                    neighborEntities++;
                    groupCenter += entity.transform.position;

                    if (distance <= avoidDist)
                    {
                        avoidForce += (this.transform.position - entity.transform.position);
                    }

                    //groupSpeed += groupSpeed + entity.GetComponent<BoidEntity>().speed;
                }
            }
        }

        Vector2 entityPosXZ = new Vector2(this.transform.position.x, this.transform.position.z);
        //Vector2 obstaclePosXZ = new Vector2(obstacle.transform.position.x, obstacle.transform.position.z);

        /**distance = Vector2.Distance(entityPosXZ, obstaclePosXZ);
        if (distance <= 5)
        {
            avoidForce += new Vector3 (entityPosXZ.x - obstaclePosXZ.x, this.transform.position.y , entityPosXZ.y - obstaclePosXZ.y) * 3;
        }**/

        if (neighborEntities >  0) { 
            groupCenter = groupCenter / neighborEntities + 
                (origin.GetComponent<BoidOrigin>().center - this.transform.position);

            direction = (avoidForce + groupCenter) - this.transform.position;

            if (direction != Vector3.zero)
            {
                    transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        if ((Vector3.Distance(transform.position, origin.transform.position)) >= 
            origin.GetComponent<BoidOrigin>().maxDistance)
        {
            Vector3 direction = origin.transform.position - transform.position;
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(direction),
                rotationSpeed * Time.deltaTime);

            speed = Random.Range(1.0f, 2f);
        }
            if (Random.Range(0, 20) < 1)
        {
            BoidRules();
        }

        /**Vector2 entityPosXZ = new Vector2(this.transform.position.x, this.transform.position.z);
        Vector2 obstaclePosXZ = new Vector2(obstacle.transform.position.x, obstacle.transform.position.z);
        float distance = Vector2.Distance(entityPosXZ, obstaclePosXZ);
        if (distance <= 1.5)
        {
            Vector3 avoidForce = new Vector3(entityPosXZ.x - obstaclePosXZ.x, this.transform.position.y, entityPosXZ.y - obstaclePosXZ.y) * 2.5f;
            Vector3 direction = avoidForce - this.transform.position;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
            }
        }**/

            transform.Translate(0, 0, Time.deltaTime * speed);

    }
}
