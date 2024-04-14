using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public UIDocument uiDocument;  // Public reference to the UIDocument component

    void Start()
    {
        // Obtain the root visual element from the UIDocument
        var root = uiDocument.rootVisualElement;

        // Find the "NextScene" button and register a click event
        var nextButton = root.Q<Button>("NextScene");
        if (nextButton != null)
        {
            nextButton.clicked += () => ChangeScene(1);  // Move to the next scene
        }

        // Find the "PreviousScene" button and register a click event
        var prevButton = root.Q<Button>("PreviousScene");
        if (prevButton != null)
        {
            prevButton.clicked += () => ChangeScene(-1);  // Move to the previous scene
        }
    }

    private void ChangeScene(int direction)
    {
        // Get the current scene index
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Calculate the new scene index using modulo to wrap around
        int newSceneIndex = (currentSceneIndex + direction + SceneManager.sceneCountInBuildSettings) % SceneManager.sceneCountInBuildSettings;

        // Load the new scene
        SceneManager.LoadScene(newSceneIndex);
    }
}
