using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

//using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal.Internal;

public class OceanaController : MonoBehaviour
{
    [Header("Oceana Stats")]
    [Tooltip("How fast Oceana moves forward or back")]
    [Range(0.0f,10.0f)]
    public float thrust = 5.0f;

    [Tooltip("How Quickly Oceana will turn or pitch")]
    [Range(0.0f, 20.0f)]
    public float responsiveness = 10f;

    [Tooltip("Max amount Oceana can pitch up and down in degrees")]
    [Range(0.0f, 90.0f)]
    public float pitchClamp = 45.0f;

    [Tooltip("At what joystick value the Oceana will turn or pitch (lower the value, more sensitive the input Action)")]
    [Range(0.0f, 1.0f)]
    public float inputSensitivity = 0.75f;

    [Tooltip("Max altitude Oceana can travel")]
    [Range(0.0f, 50.0f)]
    public float maxHeight = 25.0f;

    public Dictionary<string, bool> activeActions = new Dictionary<string, bool>() {
        {"Thrust", false},
        {"Pitch", false},
        {"Yaw", false},
        {"Shoot", false}
    };

    XRIDefaultInputActions inputActions;
    private float throttle;
    private float roll;

    [HideInInspector]
    public float pitch;
    [HideInInspector]
    public float yaw;
    private Rigidbody rb;
    private GameObject dartGun;
    

    private float responseMod {
        get {
            return (rb.mass / 10f) * responsiveness;
        }
    }

    private void HandleInputs() {
        float pitchValue = inputActions.OceanaShip.Pitch.ReadValue<float>();
        float yawValue = inputActions.OceanaShip.Yaw.ReadValue<float>();
        float throttleValue = inputActions.OceanaShip.Thrust.ReadValue<float>();

        /**ThrottleInput(throttleValue);

        if (curPhase > 0) {
            YawInput(yawValue);

            if (curPhase > 1)
               PitchInput(pitchValue); 
        }**/
        if (activeActions["Thrust"]) 
            ThrottleInput(throttleValue);
        else if (activeActions["Shoot"]) {
            CheckDirection(throttleValue);
        }
        else
            throttle = 0.0f;
            
        
        if (activeActions["Pitch"])
            PitchInput(pitchValue);
        else
            pitch = 0.0f;

        if (activeActions["Yaw"])
            YawInput(yawValue);
        else
            yaw = 0.0f;

        if (activeActions["Shoot"] && inputActions.OceanaShip.Shoot.WasPressedThisFrame())
            dartGun.GetComponent<Shooter>().Shoot();

        
    }

    private float CheckPitch() {
        var right = transform.right;
        right.y = 0;
        right *= Mathf.Sign(transform.up.y);
        var fwd = Vector3.Cross(right, Vector3.up).normalized;
        float pitchValue = Vector3.Angle(fwd, transform.forward) * Mathf.Sign(transform.forward.y);

        return pitchValue;
    }

    private void CheckDirection(float throttleValue) {
        

        if (throttleValue != 0.0f) {
            //GameObject poi = GameObject.FindGameObjectWithTag("Tracked Creature");
            GameObject poi = GameManager.Instance.TrackedCreature();

            if (Vector3.Distance(poi.transform.position, transform.position) > 20.0f) 
                ThrottleInput(throttleValue);

            else {
                //Vector3 heading = (GameObject.FindGameObjectWithTag("Tracked Creature").transform.position - transform.position).normalized;
                Vector3 heading = (poi.transform.position - transform.position).normalized;
                Vector3 direction = transform.forward * throttleValue;

                float dotProduct = Vector3.Dot(heading, direction);

                if (dotProduct < 0.0f ) 
                    ThrottleInput(throttleValue);
            
                else 
                    throttle = 0.0f;
            }
        } 
        else 
            throttle = 0.0f;
    }

    private void PitchInput(float pitchValue){
        float curPitch = CheckPitch();
        

        if(pitchValue >= inputSensitivity || pitchValue <= -inputSensitivity) {

            if (curPitch>= -pitchClamp  && curPitch <= pitchClamp)
                pitch = pitchValue/MathF.Abs(pitchValue);
            else if (curPitch <= -pitchClamp && pitchValue <= -inputSensitivity)
                pitch = pitchValue/Mathf.Abs(pitchValue);
            else if (curPitch >= pitchClamp && pitchValue >= inputSensitivity)
                pitch = pitchValue/Mathf.Abs(pitchValue);
            else 
                pitch = 0.0f;   
        }
        else {
            pitch = 0.0f;
        }
    }

    private void YawInput(float yawValue) {

        if(yawValue >= inputSensitivity || yawValue <= -inputSensitivity) {
            yaw = yawValue/Mathf.Abs(yawValue);
        }
        else {
            yaw = 0.0f;
        }
    }

    private void ThrottleInput(float throttleValue) {
        if( throttleValue > 0.0) {
            throttle = 1.0f;
        }
        else if (throttleValue < 0.0 && throttle > -1.0f) {
            throttle = -1.0f;
        }
        else {
            throttle = 0.0f;
        }
    }


    void Awake() {
        
        /**inputActions.OceanaShip.Pitch.Enable();
        inputActions.OceanaShip.Yaw.Enable();
        inputActions.OceanaShip.Thrust.Enable();
        inputActions.OceanaShip.Shoot.Enable();**/
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        dartGun = GameObject.FindGameObjectWithTag("Dart Gun");
        inputActions = new XRIDefaultInputActions();
        inputActions.OceanaShip.Enable();
    }

    void Update()
    {
        HandleInputs();

        if(gameObject.transform.position.y > maxHeight) 
            transform.position = new Vector3(transform.position.x, maxHeight, transform.position.z);

    }

    private void FixedUpdate(){
        rb.AddForce(transform.forward * thrust * throttle);
        rb.AddTorque(0.0f, yaw *responseMod, 0.0f);
        rb.AddTorque(transform.right * pitch * (responseMod/2));
    }

}
