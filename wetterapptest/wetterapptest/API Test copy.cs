using System;
using System.Xml;
using UnityEngine;
using System.Threading.Tasks;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEditor.Experimental.GraphView;
using Unity.VisualScripting;

public class APITest2 : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //public Button fetchButton;  // Assign in Inspector

    public class Location
    {
        public string lat;
        public string lon;
    }

    [System.Serializable]
    public class LocationArray
    {
        public Location[] locations;
    }

    public TMP_Text[] mainText;
    public TMP_Text[] averageText;
    public TMP_Text[] maxText;
    public TMP_Text[] minText;

    public TMP_Text cityInput;

    public Image catImage;

    public Sprite wetCat;
    public Sprite wimdyCat;
    public Sprite hellCat;
    public Sprite mathCat;
    public Sprite happiCat;
    public Sprite cursedCat;
    public MonoBehaviour rain;
    public MonoBehaviour wind;
    // public MonoBehaviour CityCoordinatesFetcher;

    string url;

    void Start()
    {

    }



    public void OnFetchButtonClicked()
    {
        StartCoroutine(GetXMLValueFromUrl("value", cityInput.text));

        IEnumerator GetXMLValueFromUrl(string elementName, string cityName)
        {
            string urlLocation = $"https://nominatim.openstreetmap.org/search?q={UnityWebRequest.EscapeURL(cityName)}&format=json&limit=1";
            UnityWebRequest request = UnityWebRequest.Get(urlLocation);
            request.SetRequestHeader("User-Agent", "UnityApp/1.0");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error fetching coordinates: " + request.error);
                yield break;
            }

            string json = request.downloadHandler.text;

            // Safely parse using JsonHelper
            Location[] locations = JsonHelper.FromJson<Location>(json);
            // Wrap array manually to parse with JsonUtility
           // string wrappedJson = "{\"locations\":" + json + "}";

            //LocationArray locationArray = JsonUtility.FromJson<LocationArray>(wrappedJson);
            /*if (locationArray.locations.Length == 0)
            {
                Debug.LogWarning("City not found.");
                yield break;
            }*/

            float lat = float.Parse(locations[0].lat, System.Globalization.CultureInfo.InvariantCulture);
            float lon = float.Parse(locations[0].lon, System.Globalization.CultureInfo.InvariantCulture);


            url = "https://api.meteomatics.com/now--now+191H:PT1H/t_2m:C/" + lat + "," + lon + "/xml?source=mix"; // Your XML link
            Debug.Log(url);

            UnityWebRequest www = UnityWebRequest.Get(url);

            string auth = System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes("lavieggmbh_kaupert_alex:uCUJ5bw77Y"));
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

            // string[] values = new string[nodes.Count];

            for (int i = 0; i < 25; i++)
            {
                //Debug.Log($"{elementName} value: {nodes[i].InnerText}");
                mainText[i].text = Convert.ToString(nodes[i].InnerText) + "°C";
            }

            //rain rain = FindFirstObjectByType<rain>();
           // wind wind = FindFirstObjectByType<wind>();


            int time = 24 - DateTime.Now.Hour;

            float[] average = { 0, 0, 0, 0, 0, 0, 0 };
            float[] max = { 0, 0, 0, 0, 0, 0, 0 };
            float[] min = { 1000, 1000, 1000, 1000, 1000, 1000, 1000 };

            float[] values = new float[nodes.Count];

            for (int i = 0; i < nodes.Count; i++)
            {
                if (float.TryParse(nodes[i].InnerText, out float parsedValue))
                {
                    values[i] = parsedValue;
                    // Debug.Log($"Yay {i}");
                }
                else
                {
                    Debug.LogWarning($"Invalid number at index {i}: {nodes[i].InnerText}");
                    values[i] = 0; // Or handle differently, e.g., skip or set to -1
                }
            }
            // if (wind.wimdy > 100)
            {
                catImage.sprite = wimdyCat;
            }
            /*else*/
            if (rain.wetness > 50)
            {
                catImage.sprite = wetCat;
            }
            else if (values[0] > 250)
            {
                catImage.sprite = hellCat;
                // Debug.Log(values[0]);
            }
            else if (values[0] > 200)
            {
                catImage.sprite = mathCat;
            }
            else if (values[0] > 150)
            {
                catImage.sprite = happiCat;
            }


            int dayOne = time + 24;
            int dayTwo = time + 48;
            int dayThree = time + 72;
            int dayFour = time + 96;
            int dayFive = time + 120;
            int daySix = time + 144;
            int daySeven = time + 168;

            for (int i = time + 1; i < dayOne; i++)
            {
                //average[0] += values[i];
                if (values[i] > max[0])
                    max[0] = values[i];

                if (values[i] < min[0])
                    min[0] = values[i];
                //Debug.Log($"slay: { values[i]}");
            }
            for (int i = time + 24; i < dayTwo; i++)
            {
                //average[1] += values[i];
                if (values[i] > max[1])
                    max[1] = values[i];

                if (values[i] < min[1])
                    min[1] = values[i];
                //Debug.Log($"{elementName} average[1]: {average[1]}");
            }
            for (int i = time + 48; i < dayThree; i++)
            {
                //average[2] += values[i];
                if (values[i] > max[2])
                    max[2] = values[i];

                if (values[i] < min[2])
                    min[2] = values[i];
                //Debug.Log($"{elementName} average[2]: {average[2]}");
            }
            for (int i = time + 72; i < dayFour; i++)
            {
                //average[3] += values[i];

                if (values[i] > max[3])
                    max[3] = values[i];

                if (values[i] < min[3])
                    min[3] = values[i];

                //Debug.Log($"{elementName} average[3]: {average[3]}");
            }
            for (int i = time + 96; i < dayFive; i++)
            {
                //average[4] += values[i];

                if (values[i] > max[4])
                    max[4] = values[i];

                if (values[i] < min[4])
                    min[4] = values[i];
                //Debug.Log($"{elementName} average[4]: {average[4]}");
            }
            for (int i = time + 120; i < daySix; i++)
            {
                //average[5] += values[i];

                if (values[i] > max[5])
                    max[5] = values[i];

                if (values[i] < min[5])
                    min[5] = values[i];
                //Debug.Log($"{elementName} average[5]: {average[5]}");
            }
            for (int i = time + 144; i < daySeven; i++)
            {
                //average[6] += values[i];

                if (values[i] > max[6])
                    max[6] = values[i];

                if (values[i] < min[6])
                    min[6] = values[i];
                //Debug.Log($"{elementName} average[6]: {average[6]}");
            }

            for (int i = 0; i < 7; i++)
            {
                //average[i] = average[i] / 240;
                //averageText[i].text = Convert.ToString(average[i]);
                min[i] /= 10;
                max[i] /= 10;
                minText[i].text = Convert.ToString(min[i]) + "°C";
                maxText[i].text = Convert.ToString(max[i]) + "°C";

            }



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

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}

}
