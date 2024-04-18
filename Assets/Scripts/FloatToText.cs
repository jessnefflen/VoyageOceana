using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FloatToText : MonoBehaviour
{
    public float pitchValue = 0;
    public float yawValue = 0;
    public TMP_Text pitchTarget;
    public TMP_Text yawTarget;

    private OceanaController controller;

    
    // Start is called before the first frame update
    void Start()
    {
        controller = gameObject.GetComponent<OceanaController>();
        pitchTarget.text = "PitchVal: " + pitchValue.ToString();
        yawTarget.text = "YawVal: " + yawValue.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        pitchValue = controller.pitch;
        yawValue = controller.yaw;
        pitchTarget.text = "PitchVal: " + pitchValue.ToString();
        yawTarget.text = "YawVal: " + yawValue.ToString();
    }
}
