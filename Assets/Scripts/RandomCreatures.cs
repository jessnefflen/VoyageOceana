using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

public class RandomCreatures : MonoBehaviour
{
    public int numToTag = 2;

    public GameObject firstCreature;
    public GameObject[] creaturesArr;
    private List<GameObject> creaturesList;


    // Start is called before the first frame update
    void Start()
    {
        creaturesArr = new GameObject[numToTag];

        List<GameObject> tmpCreatures = GameObject.FindGameObjectsWithTag("Creature").ToList<GameObject>();
        creaturesArr[0] = firstCreature;
        tmpCreatures.Remove(firstCreature);
        
        for (int i = 1; i < numToTag; i++) {
            GameObject creature = tmpCreatures[Random.Range(0, tmpCreatures.Count)];
            creaturesArr[i] = creature;
            tmpCreatures.Remove(creature);
        }

        creaturesList = creaturesArr.ToList();
    }

    public bool CheckTagged() {
        bool allTagged = true;

        foreach(GameObject creature in creaturesList) {
            if (!creature.gameObject.CompareTag("Tagged Creature")) {
                allTagged = false;
            }
        }

        return allTagged;
    }

    public GameObject Track() {
        GameObject creature = creaturesList[0];
        int i = 0;

        while (creature.gameObject.CompareTag("Tagged Creature") && i < creaturesList.Count - 1) {
            i ++;
            creature = creaturesList[i];
        }

        if (creature.gameObject.CompareTag("Creature")) {
            creature.GetComponent<CreatureToTrack>().TrackCreature();
            return creature;
        }
        else if (creature.gameObject.CompareTag("Tracked Creature")) {
            return creature;
        }
        else {
            Debug.Log("No More Creatures");
            return null;
        }
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
