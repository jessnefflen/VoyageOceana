using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IntToText : MonoBehaviour
{
    public int value = 0;
    public TMP_Text textTarget;

    private Shooter shooter;

    public string valueLable = "Darts Remaining: ";
    // Start is called before the first frame update
    void Start()
    {
        shooter = gameObject.GetComponent<Shooter>();
        textTarget.text = valueLable + value.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        value = shooter.DartsRemaining();
        textTarget.text = valueLable + value.ToString();
    }
}
