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

    public string NPC1 = "Ava";
    public string NPC2 = "Kai";

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
        string npcName = speaker == npc1 ? NPC1 : NPC2;
        string otherNPCName = speaker == npc1 ? NPC2 : NPC1;
        string formattedContext = FormatConversationContext(conversationContext);
        string fullPrompt = $"You are {npcName}, and you are having a conversation with {otherNPCName}, respond to the last message, considering conversation context {formattedContext}, {npcName}: ";
        string gender = npcName == NPC1 ? "female" : "male";



        Debug.Log("Sending prompt: " + fullPrompt);

        fullPrompt = Regex.Replace(fullPrompt, @"\r\n?|\n", " ");
        fullPrompt = Regex.Replace(fullPrompt, @"[^\w\s.,!?]", "");


        using (UnityWebRequest uwr = UnityWebRequest.PostWwwForm(apiUrl, "POST"))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes("{\"prompt\":\"" + fullPrompt + "\"}");
            uwr.uploadHandler = new UploadHandlerRaw(jsonToSend);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");

            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + uwr.error);
            }
            else
            {
                string jsonResponse = JsonUtility.FromJson<Response>(uwr.downloadHandler.text).response;
                Debug.Log("Response: " + jsonResponse);

                string responseWithPlaceholder = Regex.Replace(jsonResponse, @"\n", " ");
                responseWithPlaceholder = Regex.Replace(responseWithPlaceholder, @"[^\w\s.,!?]", "");
                // if there is a characterName or the character name with a space before it in the begining replace with nothing, but only in the begining of sentence 
                responseWithPlaceholder = Regex.Replace(responseWithPlaceholder, @"^\s?" + Regex.Escape(npcName), "", RegexOptions.IgnoreCase);


                UpdateConversationContext(responseWithPlaceholder);

                
                Debug.Log("Synthesizing speech for: " + responseWithPlaceholder + " Gender: " + gender);
                speaker.CallSynthesizeSpeech(responseWithPlaceholder, gender);

            }
        }
    }

    private void UpdateConversationContext(string response)
    {
        if (!string.IsNullOrWhiteSpace(response))
        {
            string npcName = currentSpeaker == npc1 ? NPC1 : NPC2;
            conversationContext += $"{npcName}: {response}";
            LimitConversationHistory();
        }
    }

    private void LimitConversationHistory()
    {
        while (conversationContext.Length > maxContextLength)
        {
            int firstBreak = conversationContext.IndexOf('\n');
            if (firstBreak >= 0)
            {
                conversationContext = conversationContext.Substring(firstBreak + 1);
            }
            else
            {
                break; // No further break found, break out of the loop
            }
        }
    }

    private string FormatConversationContext(string context)
    {
        return Regex.Replace(context, @"\r\n?|\n", " ");
    }

    [Serializable]
    private class Response
    {
        public string response;
    }
}
