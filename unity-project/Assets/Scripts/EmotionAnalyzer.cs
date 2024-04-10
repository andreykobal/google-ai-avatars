using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Collections;

public class EmotionAnalyzer : MonoBehaviour
{

    // Function to call the analyze_emotions endpoint and log the response
    public void AnalyzeEmotions(string text)
    {
        StartCoroutine(SendRequest(text));
    }

    private IEnumerator SendRequest(string text)
    {
        string url = "https://ailandtestnetai.top/analyze_emotions";

        // Create the request body
        string requestBody = "{\"text\": \"" + text + "\"}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestBody);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // Send the request and wait for the response
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
            }
            else
            {
                // Parse the JSON response
                var response = JObject.Parse(request.downloadHandler.text);

                Debug.Log("Emotion analysis results:" + response);
                // Log each emotion and its value
                foreach (var item in response)
                {
                    Debug.Log(item.Key + ": " + item.Value);
                }
            }
        }
    }
}
