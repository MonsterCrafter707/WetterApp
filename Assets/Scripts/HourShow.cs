using System;
using System.Xml;
using UnityEngine;
using System.Threading.Tasks;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using TMPro;
//using UnityEditor.Experimental.GraphView;
using Unity.VisualScripting;

public class HourShow : MonoBehaviour
{
    public TMP_Text[] times;

    void Start()
    {
        int counter = 1;
        for (int i = 0; i < times.Length; i++)
        {
            times[i].text = Convert.ToString(DateTime.Now.Hour + counter) + " Uhr";
            
            counter++;

            if (counter == 24 - DateTime.Now.Hour)
            {
                counter = 0 - DateTime.Now.Hour;
            }
        }
            
    }

}