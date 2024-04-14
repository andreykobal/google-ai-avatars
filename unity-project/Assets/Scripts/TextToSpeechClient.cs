using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using Unity.VisualScripting;

public class TextToSpeechClient : MonoBehaviour
{
    public AudioSource audioSource;
    public event Action OnSpeechEnded; // Event to be triggered when speech ends

    void Start()
    {
        // Initialization if needed
    }

    void Update()
    {
        // Check if the AudioSource is playing and if not, trigger the OnSpeechEnded event
        if (!audioSource.isPlaying && audioSource.clip != null)
        {
            audioSource.clip = null; // Optionally clear the clip
            OnSpeechEnded?.Invoke(); // Invoke the event
        }
    }

    IEnumerator SynthesizeSpeech(string textToSynthesize, string gender)
    {
        string url = (gender.ToLower() == "male")
            ? "https://ailandtestnetai.top/synthesize_speech_base64_male"
            : "https://ailandtestnetai.top/synthesize_speech_base64";
        var requestJson = "{\"text\":\"" + textToSynthesize + "\"}";

        UnityWebRequest uwr = WebRequestUtility.CreatePostRequest(url, requestJson);
        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(uwr.error);
        }
        else
        {
            var jsonResponse = JsonUtility.FromJson<Response>(uwr.downloadHandler.text);
            byte[] audioBytes = Convert.FromBase64String(jsonResponse.audioContentBase64);

            // Create an AudioClip from the WAV binary data
            AudioClip audioClip = WavUtility.FromBytes(audioBytes);
            audioSource.clip = audioClip;
            audioSource.Play();
        }
    }

    public void CallSynthesizeSpeech(string textToSynthesize, string gender)
    {
        //Debug.Log("Synthesizing speech for: " + textToSynthesize);
        StartCoroutine(SynthesizeSpeech(textToSynthesize, gender));
    }

    [Serializable]
    private class Response
    {
        public string audioContentBase64;
    }
}
