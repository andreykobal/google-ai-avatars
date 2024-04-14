using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text.RegularExpressions;
public class NPCManager : MonoBehaviour
{
    [SerializeField] private TextToSpeechClient npc1;
    [SerializeField] private TextToSpeechClient npc2;

    private string conversationContext = "";
    private const int maxContextLength = 4096;
    private const string apiUrl = "https://ailandtestnetai.top/generate";
    private TextToSpeechClient currentSpeaker, nextSpeaker;

    void Start()
    {
        currentSpeaker = npc1;
        nextSpeaker = npc2;
        npc1.OnSpeechEnded += SwitchSpeaker;
        npc2.OnSpeechEnded += SwitchSpeaker;
        StartCoroutine(NPCConversationLoop());
    }

    private void SwitchSpeaker()
    {
        // Swap speakers only if the last response was valid and not empty
        if (!string.IsNullOrWhiteSpace(conversationContext))
        {
            (currentSpeaker, nextSpeaker) = (nextSpeaker, currentSpeaker);
            StartCoroutine(GenerateDialogueResponse(conversationContext, currentSpeaker));
        }
    }

    IEnumerator NPCConversationLoop()
    {
        string initialPrompt = "Hi! Let's start our conversation.";
        yield return StartCoroutine(GenerateDialogueResponse(initialPrompt, currentSpeaker));
    }

    IEnumerator GenerateDialogueResponse(string prompt, TextToSpeechClient speaker)
    {
        string fullPrompt = $"NPC: {prompt} Context: {conversationContext}";
        fullPrompt = Regex.Replace(fullPrompt, @"\r\n?|\n", " "); // Remove newlines
        fullPrompt = Regex.Replace(fullPrompt, @"[^\w\s.,!?]", ""); // Remove special characters

        var requestJson = "{\"prompt\":\"" + fullPrompt + "\"}";
        Debug.Log("Sending prompt: " + fullPrompt);

        // Create the request and send JSON data
        using (UnityWebRequest uwr = UnityWebRequest.PostWwwForm(apiUrl, "POST"))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(requestJson);
            uwr.uploadHandler = new UploadHandlerRaw(jsonToSend);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");

            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError || uwr.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.LogError("Error: " + uwr.error);
            }
            else
            {
                var jsonResponse = JsonUtility.FromJson<Response>(uwr.downloadHandler.text);
                if (jsonResponse.response != null && jsonResponse.response.Length > 0)
                {
                    Debug.Log("Response: " + jsonResponse.response);

                    string responseWithPlaceholder = Regex.Replace(jsonResponse.response, @"\n", " ");
                    responseWithPlaceholder = Regex.Replace(responseWithPlaceholder, @"[^\w\s.,!?]", "");
                    // if there is a characterName or the character name with a space before it in the begining replace with nothing, but only in the begining of sentence 
                    //responseWithPlaceholder = Regex.Replace(responseWithPlaceholder, @"^\s?" + Regex.Escape(characterName), "", RegexOptions.IgnoreCase);

                    UpdateConversationContext(prompt, responseWithPlaceholder);
                    speaker.CallSynthesizeSpeech(responseWithPlaceholder);
                }
            }
        }
    }


    private void UpdateConversationContext(string prompt, string response)
    {
        if (!string.IsNullOrWhiteSpace(response))
        {
            conversationContext += $"NPC said: {prompt} Response: {response} ";
            LimitConversationHistory();
        }
    }

    private void LimitConversationHistory()
    {
        while (conversationContext.Length > maxContextLength)
        {
            int firstBreak = conversationContext.IndexOf('.');
            if (firstBreak >= 0)
            {
                conversationContext = conversationContext.Substring(firstBreak + 1);
            }
            else
            {
                break; // Break if no period is found
            }
        }
    }

    [Serializable]
    private class Response
    {
        public string response;
    }
}
