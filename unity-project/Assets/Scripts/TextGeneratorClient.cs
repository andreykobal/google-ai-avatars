using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Text.RegularExpressions;

public class TextGeneratorClient : MonoBehaviour
{
    public UIDocument uiDocument;
    private TextField userInputField;
    private Button sendButton;
    private VisualElement startButton; // To handle the StartButton as a VisualElement
    private VisualElement bodyElement; // To reference the Body VisualElement

    private VisualElement suggestionsContainer;



    public TextToSpeechClient textToSpeechClient;
    public EmotionAnalyzer emotionAnalyzer;

    private string characterName = "Ava Marshall";
    private string characterBio = "Ava Marshall, a 28-year-old former cyber security expert, turns her hacking skills into survival tactics in the virtual battlegrounds of Ethernity. In the future, the world is in chaos because of environmental disasters and greed. To distract people and control the economy, the richest 1% create a virtual game called Ethernity. In this AI-driven open-world PvPvE shooter game, players compete on a mysterious island for valuable tokens called ETNT, which are super important in the new world economy. The island's challenges are designed by the rich elite, each adding their own twist based on their interests, like tech, oil, or crime. The game is full of different challenges like fighting zombies, surviving against robots, or finding treasures, and it changes all the time. The host of the game is a friendly AI robot, but there is a big surprise about him at the end. Players fight hard for the chance to win big and become famous in this high-stakes virtual world.";



    private readonly string generateUrl = "https://ailandtestnetai.top/generate"; // Update with your server URL

    private string conversationContext = "";
    private const int maxContextLength = 4096; // Adjust based on your backend model's limit

    public ChatHistoryManager chatHistoryManager;
    public SuggestionsGeneratorClient suggestionsManager;

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
        var root = uiDocument.rootVisualElement;
        userInputField = root.Q<TextField>("Input");
        sendButton = root.Q<Button>("Send");
        startButton = root.Q<VisualElement>("StartButton"); // Find the StartButton element
        bodyElement = root.Q<VisualElement>("Body"); // Find the Body element
        suggestionsContainer = root.Q<VisualElement>("Suggestions");



        sendButton.clicked += OnSendButtonClick;

        // Initially hide the Body element
        bodyElement.style.display = DisplayStyle.None;

        // Handle StartButton click: hide StartButton, show Body, and send intro message
        startButton.RegisterCallback<ClickEvent>(evt =>
        {
            startButton.style.display = DisplayStyle.None; // Hide StartButton
            bodyElement.style.display = DisplayStyle.Flex; // Show Body element

            StartCoroutine(SendPromptAndGetResponse("Hi! In one line introduce yourself and welcome the player to the game world."));
        });


        // Add the event listener for FocusInEvent
        userInputField.RegisterCallback<FocusInEvent>(OnInputFieldFocused);

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnSendButtonClick();
        }
    }

    private void OnSendButtonClick()
    {
        if (!string.IsNullOrWhiteSpace(userInputField.text))
        {
            // clear suggestions container
            suggestionsContainer.Clear();
            StartCoroutine(SendPromptAndGetResponse(userInputField.text));
            userInputField.SetValueWithoutNotify("Write your message"); // Clear the input field after sending the message

        }
    }

    private void OnInputFieldFocused(FocusInEvent evt)
    {
        if (userInputField.text == "Write your message")
        {
            userInputField.SetValueWithoutNotify(""); // Clear the input field
        }
    }

    public void SendPromptExternal(string prompt)
    {
        StartCoroutine(SendPromptAndGetResponse(prompt));
    }

    IEnumerator SendPromptAndGetResponse(string userPrompt) // Use 'userPrompt' for clarity
    {
        chatHistoryManager.AddUserMessage(userPrompt);
        // Include the conversation context with the new prompt
        string fullPrompt = $"You are {characterName}, your bio: {characterBio}. Behave like a human, respond to player's prompts considering the conversation context: {conversationContext} Player: {userPrompt} {characterName}:";

        fullPrompt = Regex.Replace(fullPrompt, @"\r\n?|\n", " ");
        fullPrompt = Regex.Replace(fullPrompt, @"[^\w\s.,!?]", "");


        Debug.Log("Sending prompt: " + fullPrompt);

        var requestJson = "{\"prompt\":\"" + fullPrompt + "\"}";
        UnityWebRequest uwr = WebRequestUtility.CreatePostRequest(generateUrl, requestJson);
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

            emotionAnalyzer.AnalyzeEmotions(jsonResponse.response);

            // Append both the user's prompt and AI's response to the conversation context
            conversationContext += $"Player: {userPrompt} {characterName}: {jsonResponse.response}";
            TrimConversationContext(); // Ensure the conversation context does not exceed the maximum length
            suggestionsManager.SendPrompt(conversationContext);

            // Replace newline characters with a placeholder
            string responseWithPlaceholder = Regex.Replace(jsonResponse.response, @"\n", " ");
            responseWithPlaceholder = Regex.Replace(responseWithPlaceholder, @"[^\w\s.,!?]", "");
            // if there is a characterName or the character name with a space before it in the begining replace with nothing, but only in the begining of sentence 
            responseWithPlaceholder = Regex.Replace(responseWithPlaceholder, @"^\s?" + Regex.Escape(characterName), "", RegexOptions.IgnoreCase);


            chatHistoryManager.AddAvatarMessage(responseWithPlaceholder);


            // Call the text to speech client with the response
            textToSpeechClient.CallSynthesizeSpeech(responseWithPlaceholder, "female");
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
        //userInputField.text = recognizedText; // Optional: automatically set the text field
        StartCoroutine(SendPromptAndGetResponse(recognizedText)); // Automatically send the recognized text
    }
}
