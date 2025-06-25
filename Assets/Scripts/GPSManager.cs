using UnityEngine;
using UnityEngine.Android;
using System.Collections;
using System;
using System.Globalization;

public class GPSManager : MonoBehaviour
{
    public string latitude;
    public string longitude;
    public bool IsReady { get; private set; }

    IEnumerator Start()
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("GPS not enabled");
            yield break;
        }

        Input.location.Start();

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (Input.location.status != LocationServiceStatus.Running)
        {
            Debug.Log("Unable to determine device location");
            yield break;
        }

        latitude = Input.location.lastData.latitude.ToString(CultureInfo.InvariantCulture);
        longitude = Input.location.lastData.longitude.ToString(CultureInfo.InvariantCulture);
        IsReady = true;
    }
}