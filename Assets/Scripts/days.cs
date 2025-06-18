using System;
using TMPro;
using UnityEngine;

public class days : MonoBehaviour
{
    public TMP_Text[] dayNames;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < 7; i++)
            dayNames[i].text = Convert.ToString(DateTime.Now.DayOfWeek + i);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
