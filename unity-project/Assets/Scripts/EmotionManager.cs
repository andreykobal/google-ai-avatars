using UnityEngine;
using System.Collections;

public class EmotionManager : MonoBehaviour
{
    public AudioSource audioSource;
    public SkinnedMeshRenderer meshRenderer;
    private bool isCurrentlyPlaying;
    public string currentEmotion;
    private Coroutine currentTransitionCoroutine;

    void Start()
    {
        if (audioSource == null)
        {
            Debug.LogError("EmotionManager: No AudioSource component found on the GameObject.");
        }
        else
        {
            isCurrentlyPlaying = audioSource.isPlaying;
        }

        if (meshRenderer == null)
        {
            Debug.LogError("EmotionManager: No SkinnedMeshRenderer component found on the GameObject.");
        }
    }

    void Update()
    {
        if (audioSource != null && meshRenderer != null)
        {
            if (audioSource.isPlaying != isCurrentlyPlaying)
            {
                isCurrentlyPlaying = audioSource.isPlaying;

                if (isCurrentlyPlaying)
                {
                    //Debug.LogWarning("AudioSource has started playing.");
                    SetEmotionBlendShapes("neutral"); // Smooth transition to neutral
                }
                else
                {
                    //Debug.LogWarning("AudioSource has stopped.");
                    SetEmotionBlendShapes(currentEmotion);
                }
            }
        }
    }

    private void SetEmotionBlendShapes(string emotion)
    {
        //Debug.LogWarning("Setting emotion to: " + emotion);
        // If there's an existing transition, stop it
        if (currentTransitionCoroutine != null)
        {
            StopCoroutine(currentTransitionCoroutine);
        }
        currentTransitionCoroutine = StartCoroutine(TransitionToEmotion(emotion));
    }

    private IEnumerator TransitionToEmotion(string emotion)
    {
        // Define target weights for each emotion here
        float[] targetWeights = new float[meshRenderer.sharedMesh.blendShapeCount];
        // Assuming default is 0 for all
        for (int i = 0; i < targetWeights.Length; i++)
        {
            targetWeights[i] = 0f; // Default neutral state
        }
        // Adjust target weights based on the desired emotion
        switch (emotion.ToLower())
        {
            case "angry":
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.browDownLeft")] = 100f;
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.browDownRight")] = 100f;
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.eyeLookDownLeft")] = 20f;
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.eyeLookDownRight")] = 20f;
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.noseSneerLeft")] = 40f;
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.noseSneerRight")] = 40f;
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.mouthPressLeft")] = 60f;
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.mouthPressRight")] = 60f;
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.mouthPucker")] = 40f;
                break;
            case "fear":
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.browInnerUp")] = 100f;
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.eyeLookUpLeft")] = 100f;
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.eyeLookUpRight")] = 100f;
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.jawOpen")] = 40f;
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.mouthStretchLeft")] = 60f;
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.mouthStretchRight")] = 60f;
                break;
            case "happy":
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.mouthSmileLeft")] = 100f;
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.mouthSmileRight")] = 100f;
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.cheekPuff")] = 40f;
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.eyeSquintLeft")] = 60f;
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.eyeSquintRight")] = 60f;
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.browOuterUpLeft")] = 40f;
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.browOuterUpRight")] = 40f;
                break;
            case "sad":
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.browInnerUp")] = 60f;
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.mouthFrownLeft")] = 100f;
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.mouthFrownRight")] = 100f;
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.eyeLookDownLeft")] = 20f;
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.eyeLookDownRight")] = 20f;
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.jawOpen")] = 20f;
                break;
            case "surprise":
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.browOuterUpLeft")] = 100f;
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.browOuterUpRight")] = 100f;
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.eyeLookUpLeft")] = 100f;
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.eyeLookUpRight")] = 100f;
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.jawOpen")] = 80f;
                targetWeights[meshRenderer.sharedMesh.GetBlendShapeIndex("HEAD.mouthClose")] = 20f;
                break;
            case "neutral":
                for (int i = 0; i < targetWeights.Length; i++)
                {
                    targetWeights[i] = 0f; // Default neutral state
                }
                break;

        }

        // Transition time
        float transitionDuration = 1.0f; // Duration of the transition in seconds
        float time = 0;

        // Store initial weights
        float[] initialWeights = new float[targetWeights.Length];
        for (int i = 0; i < initialWeights.Length; i++)
        {
            initialWeights[i] = meshRenderer.GetBlendShapeWeight(i);
        }

        // Interpolate from initial weights to target weights over time
        while (time < transitionDuration)
        {
            time += Time.deltaTime;
            float progress = time / transitionDuration;

            for (int i = 0; i < targetWeights.Length; i++)
            {
                float newWeight = Mathf.Lerp(initialWeights[i], targetWeights[i], progress);
                meshRenderer.SetBlendShapeWeight(i, newWeight);
            }

            yield return null;
        }

        // Ensure final weights are set
        for (int i = 0; i < targetWeights.Length; i++)
        {
            meshRenderer.SetBlendShapeWeight(i, targetWeights[i]);
        }
    }

    private void SetBlendShapeWeight(string blendShapeName, float weight)
    {
        int index = meshRenderer.sharedMesh.GetBlendShapeIndex(blendShapeName);
        if (index >= 0)
        {
            meshRenderer.SetBlendShapeWeight(index, weight);
        }
        else
        {
            Debug.LogError("EmotionManager: Blend shape not found - " + blendShapeName);
        }
    }
}
