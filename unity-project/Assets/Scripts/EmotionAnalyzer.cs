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
        string requestBody = "{\"text\": \"" + text + "\"}";
        UnityWebRequest request = WebRequestUtility.CreatePostRequest(url, requestBody);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
        }
        else
        {
            // Parsing and processing logic remains unchanged
            var response = JObject.Parse(request.downloadHandler.text);
            Debug.Log("Emotion analysis results:" + response);

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
