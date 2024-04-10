using UnityEngine;

public class EmotionManager : MonoBehaviour
{
    public AudioSource audioSource;
    private bool isCurrentlyPlaying;

    public string currentEmotion;

    void Start()
    {
        if (audioSource == null)
        {
            Debug.LogError("AudioSourceStateLogger: No AudioSource component found on the GameObject.");
        }
        else
        {
            // Initialize the isCurrentlyPlaying with the AudioSource's initial state
            isCurrentlyPlaying = audioSource.isPlaying;
        }
    }

    void Update()
    {
        // Check if the AudioSource component is found
        if (audioSource != null)
        {
            // Check if the state has changed
            if (audioSource.isPlaying != isCurrentlyPlaying)
            {
                // Update the current state
                isCurrentlyPlaying = audioSource.isPlaying;

                // Log the state change
                if (isCurrentlyPlaying)
                {
                    Debug.LogWarning("AudioSource has started playing.");
                    Debug.LogWarning("Current emotion is neutral");

                }
                else
                {
                    Debug.LogWarning("AudioSource has stopped.");
                    Debug.LogWarning("Current emotion: " + currentEmotion);

                }
            }
        }
    }
}
