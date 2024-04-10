using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Collections;
using Unity.VisualScripting;

public class EmotionAnalyzer : MonoBehaviour
{
    public EmotionManager emotionManager; 

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

                // Get the emotion with the highest value, the values are float that range from 0 to 1 and set the emotionManager's currentEmotion to it, handling the case where the response is empty or when there are two emotions with the same value then choose one
                
                if (response.Count > 0)
                {
                    float max = 0;
                    string emotion = "";
                    foreach (var item in response)
                    {
                        if (float.Parse(item.Value.ToString()) > max)
                        {
                            max = float.Parse(item.Value.ToString());
                            emotion = item.Key;
                        }
                    }
                    emotionManager.currentEmotion = emotion;
                }
                else
                {
                    emotionManager.currentEmotion = "neutral";
                }

            }
        }
    }
}
