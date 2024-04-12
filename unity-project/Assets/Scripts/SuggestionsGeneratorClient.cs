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
        string prompt = "Generate a list of 6 diverse and engaging short responses for the player, based on the provided conversation context. The responses should cover a broad range of emotions including happiness, anger, sadness, and romance. Ensure at least 2 of these responses are questions aimed at furthering the dialogue. Present the responses in a structured JSON format, adhering to the template: {\"suggestions\": [\"Response 1\", \"Response 2\", \"Response 3\", \"Response 4\", \"Response 5\", \"Response 6\"]}. Incorporate the conversation context to tailor the suggestions appropriately. Conversation context: ";
        //based on the context send the prompt
        StartCoroutine(SendPromptAndGetResponse(prompt + context));
    }

    IEnumerator SendPromptAndGetResponse(string prompt)
    {
        var requestData = new RequestData { prompt = prompt };
        string json = JsonConvert.SerializeObject(requestData);

        UnityWebRequest uwr = WebRequestUtility.CreatePostRequest(generateUrl, json);
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
                var tempResponseObject = JsonConvert.DeserializeObject<TempResponseObject>(uwr.downloadHandler.text);
                if (tempResponseObject != null && !string.IsNullOrEmpty(tempResponseObject.response))
                {
                    string cleanedResponse = tempResponseObject.response.Trim(new char[] { ' ', '\n' });
                    cleanedResponse = cleanedResponse.Replace(@"\\\", @"\");

                    if (cleanedResponse.StartsWith("```json"))
                    {
                        cleanedResponse = cleanedResponse.Substring(7);
                    }
                    if (cleanedResponse.StartsWith("```JSON"))
                    {
                        cleanedResponse = cleanedResponse.Substring(7);
                    }
                    if (cleanedResponse.EndsWith("```"))
                    {
                        cleanedResponse = cleanedResponse.Substring(0, cleanedResponse.Length - 3);
                    }

                    cleanedResponse = System.Text.RegularExpressions.Regex.Unescape(cleanedResponse);

                    // Try to parse the entire response object to see if it's correctly formatted
                    var responseData = JsonConvert.DeserializeObject<ResponseData>(cleanedResponse);
                    if (responseData != null && responseData.suggestions != null)
                    {
                        foreach (var suggestion in responseData.suggestions)
                        {
                            Debug.Log(suggestion);
                            CreateSuggestionButton(suggestion);
                        }
                    }
                    else
                    {
                        // If parsing fails, manually extract and clean suggestions
                        // This is a fallback and might need adjustments based on actual malformed JSON structure
                        var pattern = @"\{""(.*?)""\}";
                        var matches = System.Text.RegularExpressions.Regex.Matches(cleanedResponse, pattern);
                        foreach (System.Text.RegularExpressions.Match match in matches)
                        {
                            if (match.Success)
                            {
                                var suggestion = match.Groups[1].Value;
                                Debug.Log(suggestion);
                                CreateSuggestionButton(suggestion);
                            }
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