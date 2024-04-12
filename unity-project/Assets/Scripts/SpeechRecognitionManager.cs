using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Collections;

public class SpeechRecognitionManager : MonoBehaviour
{
    public Button yourButton; // Assign this in the Inspector
    private bool isRecording = false;
    private AudioClip recordedClip;
    private const int SAMPLE_RATE = 16000; // Ensure this matches your server's expected sample rate

    // Define a delegate and an event
    public delegate void RecognizedSpeechAction(string recognizedText);
    public static event RecognizedSpeechAction OnRecognizedSpeech;


    // void Start()
    // {
    //     yourButton.onClick.AddListener(ToggleRecording);
    // }

    // void ToggleRecording()
    // {
    //     if (!isRecording)
    //     {
    //         // Start recording
    //         isRecording = true;
    //         recordedClip = Microphone.Start(null, false, 10, SAMPLE_RATE);
    //         Debug.Log("Recording started...");
    //     }
    //     else
    //     {
    //         // Stop recording and send for recognition
    //         isRecording = false;
    //         Microphone.End(null);
    //         Debug.Log("Recording stopped. Sending for recognition...");
    //         SendAudioForRecognition(recordedClip);
    //     }
    // }

    public void SendAudioForRecognition(AudioClip clip)
    {
        // Assuming you have already captured the audio samples...
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        // Convert to WAV format and then to Base64
        byte[] wavData = WavUtility.FromSamples(samples, clip.channels, SAMPLE_RATE);
        string audioContentBase64 = Convert.ToBase64String(wavData);

        StartCoroutine(SendRequestToServer(audioContentBase64));
    }

    IEnumerator SendRequestToServer(string audioContentBase64)
    {
        string recognitionUrl = "https://ailandtestnetai.top/recognize_speech";
        var requestJson = "{\"audioContentBase64\":\"" + audioContentBase64 + "\"}";

        UnityWebRequest uwr = WebRequestUtility.CreatePostRequest(recognitionUrl, requestJson);
        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(uwr.error);
        }
        else
        {
            Debug.Log("Recognized Text: " + uwr.downloadHandler.text);
            // Extract the recognized text and invoke the event
            string json = uwr.downloadHandler.text;
            RecognizedText recognizedText = JsonUtility.FromJson<RecognizedText>(json);
            OnRecognizedSpeech?.Invoke(recognizedText.recognizedText);
        }
    }

    [Serializable]
    private class RecognizedText
    {
        public string recognizedText;
    }
}
