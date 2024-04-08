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

    void Start()
    {
        yourButton.onClick.AddListener(ToggleRecording);
    }

    void ToggleRecording()
    {
        if (!isRecording)
        {
            // Start recording
            isRecording = true;
            recordedClip = Microphone.Start(null, false, 10, SAMPLE_RATE);
            Debug.Log("Recording started...");
        }
        else
        {
            // Stop recording and send for recognition
            isRecording = false;
            Microphone.End(null);
            Debug.Log("Recording stopped. Sending for recognition...");
            SendAudioForRecognition();
        }
    }

    void SendAudioForRecognition()
    {
        // Assuming you have already captured the audio samples...
        float[] samples = new float[recordedClip.samples * recordedClip.channels];
        recordedClip.GetData(samples, 0);

        // Convert to WAV format and then to Base64
        byte[] wavData = WavUtility.FromSamples(samples, recordedClip.channels, SAMPLE_RATE);
        string audioContentBase64 = Convert.ToBase64String(wavData);

        StartCoroutine(SendRequestToServer(audioContentBase64));
    }

    IEnumerator SendRequestToServer(string audioContentBase64)
    {
        string recognitionUrl = "http://localhost:5002/recognize_speech"; // Update with your Flask server URL
        var requestJson = "{\"audioContentBase64\":\"" + audioContentBase64 + "\"}";
        var uwr = new UnityWebRequest(recognitionUrl, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(requestJson);
        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");

        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(uwr.error);
        }
        else
        {
            Debug.Log("Recognized Text: " + uwr.downloadHandler.text);
            // Parse JSON and use the recognized text as needed
        }
    }
}
