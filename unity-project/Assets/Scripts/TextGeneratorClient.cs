using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Text.RegularExpressions;

public class TextGeneratorClient : MonoBehaviour
{
    public InputField promptInputField; // Assign in the Inspector
    public Button sendPromptButton; // Assign in the Inspector

    public TextToSpeechClient textToSpeechClient;

    public string characterName = "Carolina Bela";
    public string characterBio = "Carolina is from Brazil, 28 years old, she is a good friend and a warm hearted lover.";



    private readonly string generateUrl = "http://localhost:5002/generate"; // Update with your server URL

    private string conversationContext = "";
    private const int maxContextLength = 2048; // Adjust based on your backend model's limit

    public ChatHistoryManager chatHistoryManager;

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

    IEnumerator SendPromptAndGetResponse(string userPrompt) // Use 'userPrompt' for clarity
    {
        chatHistoryManager.AddUserMessage(userPrompt);
        // Include the conversation context with the new prompt
        string fullPrompt = $"You are {characterName}, your bio: {characterBio}. Behave like a human, respond to user prompts considering the conversation context: {conversationContext} User: {userPrompt} {characterName}:";

        fullPrompt = Regex.Replace(fullPrompt, @"\r\n?|\n", " ");

        Debug.Log("Sending prompt: " + fullPrompt);

        var requestJson = "{\"prompt\":\"" + fullPrompt + "\"}";
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
            var jsonResponse = JsonUtility.FromJson<Response>(uwr.downloadHandler.text);
            Debug.Log("Response: " + jsonResponse.response);

            // Append both the user's prompt and AI's response to the conversation context
            conversationContext += $"User: {userPrompt} {characterName}: {jsonResponse.response}";
            TrimConversationContext(); // Ensure the conversation context does not exceed the maximum length

            // Replace newline characters with a placeholder
            string responseWithPlaceholder = Regex.Replace(jsonResponse.response, @"\n", " ");
            responseWithPlaceholder = Regex.Replace(responseWithPlaceholder, @"[^\w\s.,!?]", "");

            chatHistoryManager.AddAvatarMessage(responseWithPlaceholder);

            // Call the text to speech client with the response
            textToSpeechClient.CallSynthesizeSpeech(responseWithPlaceholder);
        }
    }


    private void TrimConversationContext()
    {
        while (conversationContext.Length > maxContextLength)
        {
            int firstNewLineIndex = conversationContext.IndexOf('\n');
            if (firstNewLineIndex >= 0)
            {
                conversationContext = conversationContext.Substring(firstNewLineIndex + 1);
            }
            else
            {
                break; // Break the loop if no newline character is found
            }
        }
    }


    [Serializable]
    private class Response
    {
        public string response;
    }

    private void UseRecognizedTextAsPrompt(string recognizedText)
    {
        promptInputField.text = recognizedText; // Optional: automatically set the text field
        StartCoroutine(SendPromptAndGetResponse(recognizedText)); // Automatically send the recognized text
    }
}
