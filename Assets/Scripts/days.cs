using System;
using TMPro;
using UnityEngine;

public class days : MonoBehaviour
{
    public TMP_Text[] dayNames;
    string[] fullDays = new string[7];
    int counter;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < 8; i++)
        {
            fullDays[i] = DateTime.Today.AddDays(i + 1 ).ToString("dddd");
            
            if (dayNames != null && i < dayNames.Length)
            {
                dayNames[i].text = fullDays[i].Substring(0, 2);
            }
        }       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
