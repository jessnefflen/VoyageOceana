using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
//using UnityEditor.EditorTools;
using UnityEngine;

public class Radar : MonoBehaviour
{

    [Tooltip("Object reference of the current Creature being tracked")]
    public GameObject target;
    [Tooltip("Object reference of sumbarine object")]
    public GameObject submarine;

    [Tooltip("Reference to current creatures list")]
    public RandomCreatures creaturesList;

    [Tooltip("Maximum Distance show on the radar")]
    public float maxDist = 150f;

    private float elapsedTime = 0.0f;

    void Start()
    {
        //creaturesList.Track();
        //target = GameObject.FindGameObjectWithTag("Tracked Creature");
        target = GameManager.Instance.TrackedCreature();
        submarine = GameManager.Instance.Player;
    }

    void OnEnable() {
        //creaturesList.Track();
        //GameObject tmpTarget = GameObject.FindGameObjectWithTag("Tracked Creature");

        GameObject tmpTarget = GameManager.Instance.TrackedCreature();

        if(tmpTarget != null) {
            target = tmpTarget;
        } 
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= 1.0f) {
            elapsedTime = 0.0f;
            Vector2 heading = new Vector2(target.transform.position.x - submarine.transform.position.x, 
                target.transform.position.z - submarine.transform.position.z);

            Vector2 forward = new Vector2(submarine.transform.forward.x, submarine.transform.forward.z);

            float angle = Vector2.SignedAngle(heading, forward);
            angle = (-angle + 360 + 90) % 360;

            float distance = heading.magnitude;

            float x = Mathf.Cos((angle * Mathf.PI)/180);
            float y = Mathf.Sin((angle * Mathf.PI)/180);

            Vector2 radarVector = new Vector2(x,y);

            if (distance < maxDist) 
                radarVector *= distance * 85/maxDist;
            else
                radarVector *= 85;
            
            RectTransform picture = GetComponent<RectTransform>();
            picture.anchoredPosition = radarVector;
        }
    }
}
