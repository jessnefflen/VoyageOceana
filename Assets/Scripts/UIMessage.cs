using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UIMessage : MonoBehaviour
{
    public GameObject message;
    public bool[] restictions = new bool[4];
    
    void OnTriggerEnter(Collider collider) {
        if (collider.gameObject.CompareTag("Player")){
            /**GameObject[] sailSayings = GameObject.FindGameObjectsWithTag("S.A.I.L");

            foreach (GameObject saying in sailSayings) {
                    saying.SetActive(false);
            }
                
            message.SetActive(true);**/
            GameManager.Instance.ChangeUI(message);
            Dictionary<string, bool> activeActions = collider.gameObject.GetComponent<OceanaController>().activeActions;
            activeActions["Thrust"] = restictions[0];
            activeActions["Pitch"] = restictions[1];
            activeActions["Yaw"] = restictions[2];
            activeActions["Shoot"] = restictions[3];
            
            Destroy(gameObject);
        }

    }
}
