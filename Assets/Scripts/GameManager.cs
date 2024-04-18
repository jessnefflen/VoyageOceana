using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    private static GameManager instance;
    [SerializeField]
    [Tooltip("Player Object Reference")]
    private GameObject player;
    [SerializeField]
    [Tooltip("First Creature to track")]
    private GameObject firstCreature;
    [SerializeField]
    [Tooltip("Current UI screen")]
    private GameObject uiScreen;

    [Header("Creature Information")]
    [SerializeField]
    [Tooltip("Number of creatures tagged per voyage")]
    private int numToTag = 2;
    [SerializeField]
    [Tooltip("Array of creatures to be tracked on this voyage")]
    //Array Version of creaturesList to be easily seen
    private GameObject[] creaturesArr;

    //List version of creaturesArr for easier functionality
    private List<GameObject> creaturesList;
    [SerializeField]
    [Tooltip("Total number of tagged creatures")]
    private int totalTagged = 0;

    public static GameManager Instance{
        get {
            if (instance == null)
                Debug.LogError("Error: No Game Manager in scene.");

            return instance;
        }
    }

    public GameObject Player{
        get { return player; 
        }
    }

    private void Awake() => instance = this;

    public void QuitGame() => Application.Quit();

    public void EndGame() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    void Start() {
        player = GameObject.FindGameObjectWithTag("Player");
        PickCreatures();
        uiScreen = GameObject.FindGameObjectWithTag("S.A.I.L");
        
    }

    private void PickCreatures() {
        creaturesArr = new GameObject[numToTag];

        List<GameObject> tmpCreatureList = GameObject.FindGameObjectsWithTag("Creature").ToList<GameObject>();
        creaturesArr[0] = firstCreature;
        tmpCreatureList.Remove(firstCreature);

        for (int i = 1; i < numToTag; i++) {
            GameObject ChosenCreature = tmpCreatureList[UnityEngine.Random.Range(0, tmpCreatureList.Count)];
            creaturesArr[i] = ChosenCreature;
            tmpCreatureList.Remove(ChosenCreature);
        }

        creaturesList = creaturesArr.ToList<GameObject>();
    }
    
    public GameObject TrackedCreature() {
        GameObject creatureToTrack = null;

        if (creaturesList[0] != null) {
            creatureToTrack = creaturesList[0];
            creatureToTrack.GetComponent<CreatureToTrack>().TrackCreature();
        }

        return creatureToTrack;
        /**GameObject creature = creaturesList[0];
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
        }**/
    }


    public GameObject TaggedCreature() {
        GameObject tagged = creaturesList[0];
        tagged.GetComponent<CreatureToTrack>().TagCreature();
        creaturesList.RemoveAt(0);
        totalTagged ++;

        return tagged;
    }

    public void ChangeUI(GameObject newMessage) {
        uiScreen.SetActive(false);
        newMessage.SetActive(true);
        uiScreen = newMessage;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
