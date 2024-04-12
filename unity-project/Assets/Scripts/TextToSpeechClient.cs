using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

public class TextToSpeechClient : MonoBehaviour
{
    public AudioSource audioSource;

    void Start()
    {
        // Example call to synthesize speech from text and play it
        //StartCoroutine(SynthesizeSpeech("Hello, this is a test of the speech synthesis."));
    }

    IEnumerator SynthesizeSpeech(string textToSynthesize)
    {
        string url = "https://ailandtestnetai.top/synthesize_speech_base64";
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

    public void CallSynthesizeSpeech(string textToSynthesize)
    {
        Debug.Log("Synthesizing speech for: " + textToSynthesize);

        StartCoroutine(SynthesizeSpeech(textToSynthesize));
    }

    [Serializable]
    private class Response
    {
        public string audioContentBase64;
    }
}
