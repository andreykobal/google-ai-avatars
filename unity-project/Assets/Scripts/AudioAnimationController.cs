using UnityEngine;

public class AudioAnimationController : MonoBehaviour
{
    // Reference to the AudioSource component.
    public AudioSource audioSource;

    // Reference to the Animator component.
    public Animator animator;

    void Update()
    {
        // Check if the audioSource is playing.
        if (audioSource.isPlaying)
        {
            // If audio is playing, set the 'isTalking' bool to true.
            animator.SetBool("isTalking", true);
        }
        else
        {
            // If audio is not playing, set the 'isTalking' bool to false.
            animator.SetBool("isTalking", false);
        }
    }
}
