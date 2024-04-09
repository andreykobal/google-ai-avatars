using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class ChatHistoryManager : MonoBehaviour
{
    public UIDocument uiDocument; // Reference to your UIDocument
    public VisualElement chatHistory; // Reference to the ChatHistory ScrollView

    public Font robotoFont; // Reference to the Roboto-Regular font asset

    public void Start()
    {
        // Load and clone the UI document
        var root = uiDocument.rootVisualElement;

        // Find the ChatHistory ScrollView by name (assuming it has a specific name)
        chatHistory = root.Q<VisualElement>("ChatHistory");

        // Load the Roboto-Regular font asset
        robotoFont = Resources.Load<Font>("Resources/Roboto/Roboto-Regular");

        //test 
        //AddUserMessage("Hello");
        //AddAvatarMessage("Hi");
    }

    public void AddUserMessage(string messageText)
    {
        // Create a new label for the user message
        var userMessage = new Label();
        userMessage.style.whiteSpace = WhiteSpace.Normal; // Allow text to wrap
        userMessage.style.color = Color.yellow; // Set text color to white

        // Set the font and size
        userMessage.style.unityFont = robotoFont;
        userMessage.style.fontSize = 14;

        userMessage.text = "User: " + messageText;

        // Add the user message label to the ChatHistory
        chatHistory.Add(userMessage);

        // Scroll to the bottom of the ChatHistory
        ScrollToBottom();
    }

    public void AddAvatarMessage(string messageText)
    {
        // Create a new label for the avatar message
        var avatarMessage = new Label();
        avatarMessage.style.whiteSpace = WhiteSpace.Normal; // Allow text to wrap
        avatarMessage.style.color = Color.white; // Set text color to white

        // Set the font and size
        avatarMessage.style.unityFont = robotoFont;
        avatarMessage.style.fontSize = 14;

        avatarMessage.text = "Avatar: " + messageText;

        // Add the avatar message label to the ChatHistory
        chatHistory.Add(avatarMessage);

        // Scroll to the bottom of the ChatHistory
        ScrollToBottom();
    }

    private void ScrollToBottom()
    {
        // Find the Scroller element by type (assuming it's a direct child)
        var scroller = chatHistory.Q<Scroller>();

        if (scroller != null)
        {
            // Set the value of the Scroller's vertical axis to its maximum value
            scroller.value = scroller.highValue;

            // Wait for one frame to allow the UI to update
            StartCoroutine(ScrollToBottomCoroutine());
        }
    }

    private IEnumerator ScrollToBottomCoroutine()
    {
        // Wait for the next frame
        yield return null;

        // Find the Scroller element again
        var scroller = chatHistory.Q<Scroller>();

        if (scroller != null)
        {
            // Set the value of the Scroller's vertical axis to its maximum value
            scroller.value = scroller.highValue;
        }
    }
}
