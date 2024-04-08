using UnityEngine;

public class AudioResampler : MonoBehaviour
{
    // Modified to return an AudioClip
    public AudioClip ResampleAudioClip(AudioClip originalClip)
    {
        float[] originalData = new float[originalClip.samples * originalClip.channels];
        originalClip.GetData(originalData, 0);

        int newSampleRate = 16000;
        float resampleRatio = newSampleRate / (float)originalClip.frequency;
        int newSampleCount = Mathf.CeilToInt(originalData.Length * resampleRatio);
        float[] newData = new float[newSampleCount];

        for (int i = 0; i < newSampleCount; i++)
        {
            float originalPosition = i / resampleRatio;
            int originalIndex = Mathf.FloorToInt(originalPosition);
            float nextSampleRatio = originalPosition - originalIndex;
            int nextIndex = originalIndex + 1;

            if (nextIndex >= originalData.Length) nextIndex = originalData.Length - 1; // Boundary check

            // Linear interpolation for resampling
            newData[i] = Mathf.Lerp(originalData[originalIndex], originalData[nextIndex], nextSampleRatio);
        }

        AudioClip newClip = AudioClip.Create(originalClip.name + "_resampled", newSampleCount / originalClip.channels, originalClip.channels, newSampleRate, false);
        newClip.SetData(newData, 0);

        // Return the newly created AudioClip
        return newClip;
    }
}
