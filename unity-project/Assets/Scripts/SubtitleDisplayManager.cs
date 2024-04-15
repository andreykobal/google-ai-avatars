using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class SubtitleDisplayManager : MonoBehaviour
{
    public UIDocument uiDocument; // The UIDocument component that contains your UI Toolkit elements.
    private Label subtitlesLabel; // The Label element to display subtitles.
    private Coroutine subtitleCoroutine; // Reference to the currently running subtitle coroutine.

    void Start()
    {
        if (uiDocument != null)
        {
            // Retrieve the Label element by its name.
            subtitlesLabel = uiDocument.rootVisualElement.Q<Label>("Subtitles");
        }
    }

    // Method to start the subtitles display coroutine.
    public void DisplaySubtitles(string text)
    {
        if (subtitlesLabel != null)
        {
            // First clear the text field
            subtitlesLabel.text = "";
        }

        // Stop the existing coroutine if it is running
        if (subtitleCoroutine != null)
        {
            StopCoroutine(subtitleCoroutine);
        }

        // Start a new coroutine and store its reference
        subtitleCoroutine = StartCoroutine(ShowSubtitles(text));
    }

    // Coroutine to display each sentence in the subtitles label with a delay.
    IEnumerator ShowSubtitles(string fullText)
    {
        // Split the text by '.', '?', '!'
        char[] delimiters = new char[] { '.', '?', '!' };
        string[] sentences = fullText.Split(delimiters, System.StringSplitOptions.RemoveEmptyEntries);

        foreach (string sentence in sentences)
        {
            if (subtitlesLabel != null)
            {
                // Add the punctuation back to the end of each sentence
                string trimmedSentence = sentence.Trim();
                string lastChar = fullText[fullText.IndexOf(trimmedSentence) + trimmedSentence.Length].ToString();
                // Convert the whole sentence to uppercase before displaying it
                subtitlesLabel.text = (trimmedSentence + lastChar).ToUpper();

                yield return new WaitForSeconds(4.0f); // Wait for 4 seconds before showing the next sentence.
            }
        }

        // Clear the subtitles after the last sentence.
        if (subtitlesLabel != null)
        {
            subtitlesLabel.text = "";
        }
    }
}
