using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic; // For List<>
using Newtonsoft.Json; // Make sure to include this for Newtonsoft JSON

public class SuggestionsGeneratorClient : MonoBehaviour
{
    private readonly string generateUrl = "https://ailandtestnetai.top/generate"; // Update with your server URL
    public UIDocument uiDocument;
    private VisualElement suggestionsContainer;

    public TextGeneratorClient textGeneratorClient;

    void Start()
    {
        var root = uiDocument.rootVisualElement;
        suggestionsContainer = root.Q<VisualElement>("Suggestions");
        // Call the SendPrompt function with your prompt
        //SendPrompt("Player: Hello how are you doing? Avatar: I'm doing well thank you for asking. How can I help you today? Player: I'm looking for a new game to play. Avatar: I can help you with that. What type of game are you looking for?" + "Player: ");
    }

    public void SendPrompt(string context)
    {
        string prompt = "Based on conversation context, come up with 6 suggestions of short responses of the player that can include happy, angry, sad, romantic answers or statements or expressions of feelings and at least 2 questions, and format them in json format using the template: {\"suggestions\":[\"Option 1\",\"Option 2\",\"Option 3\"]}. Conversation context: ";
        //based on the context send the prompt
        StartCoroutine(SendPromptAndGetResponse(prompt + context));
    }

    IEnumerator SendPromptAndGetResponse(string prompt)
    {
        var requestData = new RequestData { prompt = prompt };
        string json = JsonConvert.SerializeObject(requestData);

        var uwr = new UnityWebRequest(generateUrl, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
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
            try
            {
                // First deserialization step to get the 'response' string
                var tempResponseObject = JsonConvert.DeserializeObject<TempResponseObject>(uwr.downloadHandler.text);
                if (tempResponseObject != null && !string.IsNullOrEmpty(tempResponseObject.response))
                {
                    // Clean the response string
                    string cleanedResponse = tempResponseObject.response.Trim(new char[] { ' ', '\n' });

                    // Replace triple backslashes with a single backslash
                    cleanedResponse = cleanedResponse.Replace(@"\\\", @"\");


                    // Check and remove Markdown code block markers if present
                    if (cleanedResponse.StartsWith("```json"))
                    {
                        cleanedResponse = cleanedResponse.Substring(7); // Remove starting '```json\n'
                    }
                    if (cleanedResponse.EndsWith("```"))
                    {
                        cleanedResponse = cleanedResponse.Substring(0, cleanedResponse.Length - 3); // Remove ending '\n```'
                    }

                    // Unescape the JSON string if it was escaped
                    cleanedResponse = System.Text.RegularExpressions.Regex.Unescape(cleanedResponse);

                    // Second deserialization step for the actual suggestions JSON
                    var responseData = JsonConvert.DeserializeObject<ResponseData>(cleanedResponse);
                    if (responseData != null && responseData.suggestions != null)
                    {
                        foreach (var suggestion in responseData.suggestions)
                        {
                            Debug.Log(suggestion);
                            CreateSuggestionButton(suggestion);
                        }
                    }
                }
            }
            catch (JsonException e)
            {
                Debug.LogError($"JSON Parsing Error: {e.Message}");
            }
        }
    }

    void CreateSuggestionButton(string suggestion)
    {
        // Create a button with the text of each suggestion as a button label  
        Button suggestionButton = new Button(() =>
        {
            textGeneratorClient.SendPromptExternal(suggestion);
            suggestionsContainer.Clear();
        })
        {
            text = suggestion // Set the button text
        };

        suggestionButton.AddToClassList("suggestion-button"); // Add a CSS class to the button
                                                              // Add it to the suggestions container
        suggestionsContainer.Add(suggestionButton);
    }



    // TempResponseObject is used for the initial deserialization to get the 'response' string.
    [System.Serializable]
    private class TempResponseObject
    {
        public string response;
    }


    [System.Serializable]
    private class RequestData
    {
        public string prompt;
    }

    [System.Serializable]
    private class ResponseData
    {
        public List<string> suggestions; // Adjusted to match the expected response
    }
}