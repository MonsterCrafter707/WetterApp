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

public class uv : MonoBehaviour
{
    string url = "https://api.meteomatics.com/now/uv:idx/postal_DE38106/xml?source=mix"; // Your XML link
    public TMP_Text uvIdx;
    public void OnFetchButtonClicked()
    {
        StartCoroutine(GetXMLValueFromUrl(url, "value"));


        IEnumerator GetXMLValueFromUrl(string url, string elementName)
        {

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

            XmlNode node = xmlDoc.SelectSingleNode($"//{elementName}");

            uvIdx.text = "Index " + Convert.ToString(node.InnerText);

        }
    }
}
