using System;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class APITest : MonoBehaviour
{
    [System.Serializable]
    public class Location
    {
        public string lat;
        public string lon;
    }

    public TMP_Text[] mainText;
    public TMP_Text[] averageText;
    public TMP_Text[] maxText;
    public TMP_Text[] minText;

    public TMP_Text rainAmount;
    public TMP_Text cityInput;
    public TMP_Text windspeed;
    public TMP_Text uvIdx;

    public Image catImage;

    public Sprite wetCat;
    public Sprite wimdyCat;
    public Sprite hellCat;
    public Sprite mathCat;
    public Sprite happiCat;
    public Sprite cursedCat;

    public string latitude;
    public string longitude;
    public bool IsReady { get; private set; }

    public MonoBehaviour currentGPS;

    //public rain rain;
    //public wind wind;

    string url;
    
    IEnumerator Start()
    {
        GPSManager currentGPS = FindFirstObjectByType<GPSManager>();

        if (currentGPS == null)
        {
            Debug.LogError("GPSManager not found!");
            yield break;
        }

        // Wait until GPS is ready
        while (!currentGPS.IsReady)
        {
            //Debug.Log("Waiting for GPS...");
            yield return null;
        }

        // Now that we have valid coordinates, start weather download
        StartCoroutine(StartGPS("value", currentGPS.latitude, currentGPS.longitude));
    }

    private IEnumerator StartGPS(string elementName, string lat, string lon)
    {
        string latlon = lat + "," + lon;

        url = $"https://api.meteomatics.com/now--now+191H:PT1H/t_2m:C/{latlon}/xml?source=mix";
        Debug.Log(url);

        //Uses coords

        UnityWebRequest www = UnityWebRequest.Get(url);
        string auth = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes("lavieggmbh_kaupert_alex:uCUJ5bw77Y"));
        www.SetRequestHeader("Authorization", "Basic " + auth);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error fetching XML: " + www.error);
            yield break;
        }

        string xmlText = www.downloadHandler.text;
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlText);

        XmlNodeList nodes = xmlDoc.GetElementsByTagName(elementName);
        int count = Mathf.Min(nodes.Count, mainText.Length);

        for (int i = 0; i < count; i++)
        {
            mainText[i].text = nodes[i].InnerText + "°C";
        }

        float[] values = new float[nodes.Count];
        for (int i = 0; i < nodes.Count; i++)
        {
            float.TryParse(nodes[i].InnerText, out values[i]);
        }


        //Rain Call

        string urlRain = $"https://api.meteomatics.com/now/precip_1h:mm/{latlon}/xml?source=mix"; // Your XML link
        www = UnityWebRequest.Get(urlRain);

        auth = System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes("lavieggmbh_kaupert_alex:uCUJ5bw77Y"));
        www.SetRequestHeader("Authorization", "Basic " + auth);

        yield return www.SendWebRequest();


        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error fetching XML: " + www.error);
            yield break;
        }

        xmlText = www.downloadHandler.text;
        xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlText);

        XmlNode nodeRain = xmlDoc.SelectSingleNode($"//{elementName}");

        rainAmount.text = Convert.ToString(nodeRain.InnerText) + " mm";

        //Wind Call
         string urlWind = $"https://api.meteomatics.com/now/wind_speed_10m:kmh/{latlon}/xml?source=mix"; // Your XML link
         www = UnityWebRequest.Get(urlWind);

         www.SetRequestHeader("Authorization", "Basic " + auth);

        yield return www.SendWebRequest();


        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error fetching XML: " + www.error);
            yield break;
        }

        xmlText = www.downloadHandler.text;
        xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlText);

       XmlNode nodeWind = xmlDoc.SelectSingleNode($"//{elementName}");

        windspeed.text = Convert.ToString(nodeWind.InnerText) + " km/h";

        //UV Call

        string urlUV = $"https://api.meteomatics.com/now/uv:idx/{latlon}/xml?source=mix"; // Your XML link
        www = UnityWebRequest.Get(urlUV);

        auth = System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes("lavieggmbh_kaupert_alex:uCUJ5bw77Y"));
        www.SetRequestHeader("Authorization", "Basic " + auth);

        yield return www.SendWebRequest();


        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error fetching XML: " + www.error);
            yield break;
        }

            xmlText = www.downloadHandler.text;
            xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlText);

            XmlNode node = xmlDoc.SelectSingleNode($"//{elementName}");

            uvIdx.text = "Index " + Convert.ToString(node.InnerText);

        if (values[0] > 300)
            catImage.sprite = cursedCat;
        else if (values[0] > 250)
            catImage.sprite = hellCat;
        else if (values[0] > 200)
            catImage.sprite = mathCat;
        else if (values[0] > 150)
            catImage.sprite = happiCat;
        

        // Daily min/max setup
        int time = 24 - DateTime.Now.Hour;
        float[] max = new float[7];
        float[] min = new float[7];
        for (int i = 0; i < 7; i++)
        {
            max[i] = float.MinValue;
            min[i] = float.MaxValue;

            int start = time + i * 24;
            int end = start + 24;

            for (int j = start; j < end && j < values.Length; j++)
            {
                if (values[j] > max[i]) max[i] = values[j];
                if (values[j] < min[i]) min[i] = values[j];
            }

            // Convert from tenths of a degree to degrees (if applicable)
            min[i] /= 10f;
            max[i] /= 10f;

            minText[i].text = min[i].ToString("0.#") + "°C";
            maxText[i].text = max[i].ToString("0.#") + "°C";
        }    
    }

    public void OnFetchButtonClicked()
    {
        StartCoroutine(GetXMLValueFromUrl("value", cityInput.text));
    }

    private IEnumerator GetXMLValueFromUrl(string elementName, string cityName)
    {
        string urlLocation = $"https://nominatim.openstreetmap.org/search?q={UnityWebRequest.EscapeURL(cityName)}&format=json&limit=1";
        UnityWebRequest request = UnityWebRequest.Get(urlLocation);
        request.SetRequestHeader("User-Agent", "UnityApp/1.0");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error fetching coordinates: " + request.error);
            Debug.Log("Requesting: " + urlLocation);
            yield break;
        }

        string json = request.downloadHandler.text;
        Location[] locations = JsonHelper.FromJson<Location>(json);

        if (locations == null || locations.Length == 0)
        {
            Debug.LogWarning("City not found or JSON invalid.");
            yield break;
        }

        
        float lat = float.Parse(locations[0].lat, System.Globalization.CultureInfo.InvariantCulture);
        float lon = float.Parse(locations[0].lon, System.Globalization.CultureInfo.InvariantCulture);

        string latlon = Convert.ToString(lat).Replace(",", ".") + "," + Convert.ToString(lon).Replace(",", ".");

        url = $"https://api.meteomatics.com/now--now+191H:PT1H/t_2m:C/{latlon}/xml?source=mix";
        Debug.Log(url);

        //Uses coords

        UnityWebRequest www = UnityWebRequest.Get(url);
        string auth = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes("lavieggmbh_kaupert_alex:uCUJ5bw77Y"));
        www.SetRequestHeader("Authorization", "Basic " + auth);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error fetching XML: " + www.error);
            yield break;
        }

        string xmlText = www.downloadHandler.text;
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlText);

        XmlNodeList nodes = xmlDoc.GetElementsByTagName(elementName);
        int count = Mathf.Min(nodes.Count, mainText.Length);

        for (int i = 0; i < count; i++)
        {
            mainText[i].text = nodes[i].InnerText + "°C";
        }

        float[] values = new float[nodes.Count];
        for (int i = 0; i < nodes.Count; i++)
        {
            float.TryParse(nodes[i].InnerText, out values[i]);
        }

        //Rain Call

        string urlRain = $"https://api.meteomatics.com/now/precip_1h:mm/{latlon}/xml?source=mix"; // Your XML link
        www = UnityWebRequest.Get(urlRain);

        auth = System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes("lavieggmbh_kaupert_alex:uCUJ5bw77Y"));
        www.SetRequestHeader("Authorization", "Basic " + auth);

        yield return www.SendWebRequest();


        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error fetching XML: " + www.error);
            yield break;
        }

        xmlText = www.downloadHandler.text;
        xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlText);

        XmlNode nodeRain = xmlDoc.SelectSingleNode($"//{elementName}");

        rainAmount.text = Convert.ToString(nodeRain.InnerText) + " mm";

        //Wind Call
         string urlWind = $"https://api.meteomatics.com/now/wind_speed_10m:kmh/{latlon}/xml?source=mix"; // Your XML link
         www = UnityWebRequest.Get(urlWind);

         www.SetRequestHeader("Authorization", "Basic " + auth);

        yield return www.SendWebRequest();


        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error fetching XML: " + www.error);
            yield break;
        }

        xmlText = www.downloadHandler.text;
        xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlText);

       XmlNode nodeWind = xmlDoc.SelectSingleNode($"//{elementName}");

        windspeed.text = Convert.ToString(nodeWind.InnerText) + " km/h";

        //UV Call

        string urlUV = $"https://api.meteomatics.com/now/uv:idx/{latlon}/xml?source=mix"; // Your XML link
        www = UnityWebRequest.Get(urlUV);

        auth = System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes("lavieggmbh_kaupert_alex:uCUJ5bw77Y"));
        www.SetRequestHeader("Authorization", "Basic " + auth);

        yield return www.SendWebRequest();


        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error fetching XML: " + www.error);
            yield break;
        }

            xmlText = www.downloadHandler.text;
            xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlText);

            XmlNode node = xmlDoc.SelectSingleNode($"//{elementName}");

            uvIdx.text = "Index " + Convert.ToString(node.InnerText);

        string winding = windspeed.text.Replace(" km/h", "");
        int windingInt = Convert.ToInt32(winding.Replace(".", ""));
        string raining = rainAmount.text.Replace("m", "");
        int rainingInt = Convert.ToInt32(raining.Replace(".", ""));

        // Weather logic
        if (windingInt > 100)
            catImage.sprite = wimdyCat;
        else if (rainingInt > 200)
            catImage.sprite = wetCat;
        else if (values[0] > 300)
            catImage.sprite = cursedCat;
        else if (values[0] > 250)
            catImage.sprite = hellCat;
        else if (values[0] > 200)
            catImage.sprite = mathCat;
        else if (values[0] > 150)
            catImage.sprite = happiCat;      
        

        // Daily min/max setup
        int time = 24 - DateTime.Now.Hour;
        float[] max = new float[7];
        float[] min = new float[7];
        for (int i = 0; i < 7; i++)
        {
            max[i] = float.MinValue;
            min[i] = float.MaxValue;

            int start = time + i * 24;
            int end = start + 24;

            for (int j = start; j < end && j < values.Length; j++)
            {
                if (values[j] > max[i]) max[i] = values[j];
                if (values[j] < min[i]) min[i] = values[j];
            }

            // Convert from tenths of a degree to degrees (if applicable)
            min[i] /= 10f;
            max[i] /= 10f;

            minText[i].text = min[i].ToString("0.#") + "°C";
            maxText[i].text = max[i].ToString("0.#") + "°C";
        }
    }

    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            string wrappedJson = "{\"Items\":" + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrappedJson);
            return wrapper.Items;
        }

        [Serializable]
        private class Wrapper<T>
        {
            public T[] Items;
        }
    }
}
