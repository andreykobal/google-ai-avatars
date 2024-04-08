using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine.Networking;


namespace uMicrophoneWebGL.Samples
{

    public class MicrophoneRecorder : MonoBehaviour
    {
        [Header("Components")]
        public MicrophoneWebGL microphoneWebGL;
        public AudioSource audioSource;

        [Header("UI")]
        public Text toggleButtonText;
        public Button playButton;
        public Text playButtonText;
        public Dropdown deviceDropdown;

        [Header("Record")]
        public float maxDuration = 10f;

        private float[] _buffer = null;
        private int _bufferSize = 0;
        private AudioClip _clip;
        private bool _isPlaying = false;


        public Button yourButton; // Assign this in the Inspector
        private bool isRecording = false;
        private AudioClip recordedClip;
        public AudioResampler resampler; // Assign this from the inspector

        public SpeechRecognitionManager speechRecognitionManager; // Assign this from the inspector

        [Header("Silence Detection")]
        public float silenceThreshold = 0.001f; // Volume level below which is considered silence
        public float silenceDuration = 2.0f; // Duration in seconds for which silence must persist to stop recording
        private float silenceTimer = 0f; // Timer to track silence duration


        void Update()
        {
            UpdatePlayButtonText();
        }

        private void UpdatePlayButtonText()
        {
            if (!audioSource) return;
            if (audioSource.isPlaying == _isPlaying) return;

            _isPlaying = audioSource.isPlaying;
            playButtonText.text = _isPlaying ? "Stop" : "Play";
        }

        public void ToggleRecord()
        {
            if (!microphoneWebGL || !microphoneWebGL.isValid) return;

            bool isRecording = microphoneWebGL.isRecording;

            if (!isRecording)
            {
                Begin();
            }
            else
            {
                End();
            }

            isRecording = !isRecording;

            if (toggleButtonText)
            {
                toggleButtonText.text = isRecording ? "Stop" : "Start";
            }

            if (playButton)
            {
                playButton.interactable = !isRecording;
            }
        }

        public void TogglePlay()
        {
            if (!audioSource) return;

            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            else
            {
                audioSource.clip = _clip;
                audioSource.Play();
            }
        }

        private void Begin()
        {
            // Reset silence timer on begin
            silenceTimer = 0f;


            if (deviceDropdown)
            {
                microphoneWebGL.micIndex = deviceDropdown.value;
            }

            microphoneWebGL.Begin();
        }

        private void End()
        {
            microphoneWebGL.End();
        }

        public void OnBegin()
        {
            int freq = microphoneWebGL.selectedDevice.sampleRate;
            int n = (int)(freq * maxDuration);
            if (_buffer == null || _buffer.Length != n)
            {
                _buffer = new float[n];
            }
            _bufferSize = 0;
        }

        public void OnEnd()
        {
            if (!audioSource) return;

            var freq = microphoneWebGL.selectedDevice.sampleRate;
            _clip = AudioClip.Create("uMicrophoneWebGL-Recorded", _bufferSize, 1, freq, false);
            var data = new float[_bufferSize];
            System.Array.Copy(_buffer, data, _bufferSize);
            _clip.SetData(data, 0);

            // Ensure there's a clip and speechRecognitionManager is assigned
            if (_clip != null)
            {
                // Resample the audio clip to 16 kHz
                AudioClip resampledClip = resampler.ResampleAudioClip(_clip);
                speechRecognitionManager.SendAudioForRecognition(resampledClip);
            }

        }
        public void OnData(float[] input)
        {
            if (input == null) return;

            if (IsSilent(input))
            {
                silenceTimer += (float)input.Length / microphoneWebGL.selectedDevice.sampleRate;
                Debug.Log("Silence detected. Timer: " + silenceTimer); // Debugging log

                if (silenceTimer >= silenceDuration)
                {
                    Debug.Log("Silence duration exceeded. Stopping recording."); // Debugging log
                    ToggleRecord(); // Stop recording if silence persists
                    return; // Exit to prevent adding silent data
                }
            }
            else
            {
                Debug.Log("Noise detected. Resetting silence timer."); // Debugging log
                silenceTimer = 0f; // Reset silence timer if there's noise
            }

            int n = input.Length;
            if (_bufferSize + n > _buffer.Length) return;
            Array.Copy(input, 0, _buffer, _bufferSize, n);
            _bufferSize += n;
        }

        private bool IsSilent(float[] buffer)
        {
            float sum = 0f;
            for (int i = 0; i < buffer.Length; i++)
            {
                sum += buffer[i] * buffer[i]; // Summing squares of samples
            }
            float rms = Mathf.Sqrt(sum / buffer.Length); // Root mean square volume
            Debug.Log("RMS Volume: " + rms); // Debugging log

            return rms < silenceThreshold;
        }

        public void OnDeviceListUpdated(List<Device> devices)
        {
            if (!deviceDropdown) return;

            var options = new List<Dropdown.OptionData>();
            foreach (var device in devices)
            {
                Dropdown.OptionData option = new Dropdown.OptionData()
                {
                    text = device.label,
                };
                options.Add(option);
            }
            deviceDropdown.options = options;
        }

        // Expose the recorded audio clip
        public AudioClip GetClip()
        {
            return _clip;
        }
    }

}
