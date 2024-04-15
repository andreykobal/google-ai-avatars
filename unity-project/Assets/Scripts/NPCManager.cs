using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text.RegularExpressions;

public class NPCManager : MonoBehaviour
{
    [SerializeField] private TextToSpeechClient npc1;
    [SerializeField] private TextToSpeechClient npc2;

    [SerializeField] private SubtitleDisplayManager subtitleDisplayManager;


    private string conversationContext = "";
    private const int maxContextLength = 4096;
    private const string apiUrl = "https://ailandtestnetai.top/generate";
    private TextToSpeechClient currentSpeaker, nextSpeaker;

    public string NPC1 = "Ava";
    public string NPC2 = "Kai";

    public string Npc1Bio = "Ava Marshall, a 28-year-old former cyber security expert, turns her hacking skills into survival tactics in the virtual battlegrounds of Ethernity. The year is 2052, the world is in chaos because of environmental disasters and greed. To distract people and control the economy, the richest 1% create a virtual game called Ethernity. In this AI-driven open-world PvPvE shooter game, players compete on a mysterious island for valuable blockchain tokens called ETNT, which are super important in the new world economy. The island's challenges are designed by the rich elite, each adding their own twist based on their interests, like tech, oil, or crime. The game is full of different challenges like fighting zombies, surviving against robots, or finding treasures, and it changes all the time. The host of the game is a friendly AI robot, but there is a big surprise about him at the end. Players fight hard for the chance to win big and become famous in this high-stakes virtual world.";

    public string Npc2Bio = "Kai Smith is a 30-year-old professional mercenary who now uses his skills to survive in the virtual battlegrounds of Ethernity.";


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
        string npcBio = speaker == npc1 ? Npc1Bio : Npc2Bio;
        string otherNpcBio = speaker == npc1 ? Npc2Bio : Npc1Bio;
        string formattedContext = FormatConversationContext(conversationContext);
        string fullPrompt = $"You are {npcName}, your bio: {npcBio}, and you are having a casual conversation discussing the recent battles, weapons, skins, tactics, crypto earnings, completed quests and a humorous misunderstanding has just occurred with {otherNPCName}, who's bio is {otherNpcBio}. Keep the conversation light, filled with jokes and playful banter, respond very shotrly to the last message, considering conversation context {formattedContext}, {npcName}: ";

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

                // Before synthesizing speech, show subtitles
                subtitleDisplayManager.DisplaySubtitles(responseWithPlaceholder);


                //Debug.Log("Synthesizing speech for: " + responseWithPlaceholder + " Gender: " + gender);
                speaker.CallSynthesizeSpeech(responseWithPlaceholder, gender);

            }
        }
    }

    private void UpdateConversationContext(string response)
    {
        if (!string.IsNullOrWhiteSpace(response))
        {
            string npcName = currentSpeaker == npc1 ? NPC1 : NPC2;
            conversationContext += $" {npcName}: {response}";
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
