using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class TextGeneratorClient : MonoBehaviour
{
    public InputField promptInputField; // Assign in the Inspector
    public Button sendPromptButton; // Assign in the Inspector

    private readonly string generateUrl = "http://localhost:5002/generate"; // Update with your server URL

    void OnEnable()
    {
        SpeechRecognitionManager.OnRecognizedSpeech += UseRecognizedTextAsPrompt;
    }

    void OnDisable()
    {
        SpeechRecognitionManager.OnRecognizedSpeech -= UseRecognizedTextAsPrompt;
    }

    void Start()
    {
        sendPromptButton.onClick.AddListener(() =>
        {
            if (!string.IsNullOrWhiteSpace(promptInputField.text))
            {
                StartCoroutine(SendPromptAndGetResponse(promptInputField.text));
            }
        });
    }

    IEnumerator SendPromptAndGetResponse(string prompt)
    {
        var requestJson = "{\"prompt\":\"" + prompt + "\"}";
        var uwr = new UnityWebRequest(generateUrl, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(requestJson);
        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");

        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + uwr.error);
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text);
            // Parse the JSON response and use the 'response' field as needed
        }
    }

    private void UseRecognizedTextAsPrompt(string recognizedText)
    {
        promptInputField.text = recognizedText; // Optional: automatically set the text field
        StartCoroutine(SendPromptAndGetResponse(recognizedText)); // Automatically send the recognized text
    }
}
