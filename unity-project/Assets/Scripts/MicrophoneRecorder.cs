using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;

namespace uMicrophoneWebGL.Samples
{
    public class MicrophoneRecorder : MonoBehaviour
    {
        [Header("Components")]
        public MicrophoneWebGL microphoneWebGL;
        public AudioSource audioSource;

        [Header("UI Toolkit")]
        public UIDocument uiDocument;

        private DropdownField deviceDropdown;
        private Button yourButton;
        private VisualElement micIcon;

        [Header("Record")]
        public float maxDuration = 10f;

        private float[] _buffer = null;
        private int _bufferSize = 0;
        private AudioClip _clip;

        private bool isRecording = false;
        public AudioResampler resampler;

        public SpeechRecognitionManager speechRecognitionManager;

        [Header("Silence Detection")]
        public float silenceThreshold = 0.001f;
        public float silenceDuration = 2.0f;
        private float silenceTimer = 0f;

        public Texture2D defaultMicIcon;
        public Texture2D recordingMicIcon;

        private Dictionary<string, int> deviceLabelToIndexMap = new Dictionary<string, int>();


        private void Awake()
        {
            var root = uiDocument.rootVisualElement;
            deviceDropdown = root.Q<DropdownField>("MicrophoneDropdown");
            yourButton = root.Q<Button>("Voice");
            micIcon = root.Q<VisualElement>("MicIcon");

            yourButton.clicked += ToggleRecord;
            deviceDropdown.RegisterCallback<ChangeEvent<string>>(OnMicrophoneSelected);

            SetMicIcon(defaultMicIcon);
        }

        private void OnMicrophoneSelected(ChangeEvent<string> evt)
        {
            // Log the selected device name
            Debug.Log($"Selected Device Name: {evt.newValue}");

            // Update to use the mapping
            if (deviceLabelToIndexMap.TryGetValue(evt.newValue, out int deviceIndex))
            {
                microphoneWebGL.micIndex = deviceIndex;
                Debug.Log($"Device Index Set To: {deviceIndex}"); // Optionally log the index as well

            }
            else
            {
                Debug.LogError($"No device index found for selected label: {evt.newValue}");
            }
        }

        public void ToggleRecord()
        {
            if (!microphoneWebGL || !microphoneWebGL.isValid) return;

            isRecording = !microphoneWebGL.isRecording;

            if (isRecording)
            {
                Begin();
            }
            else
            {
                End();
            }

            SetMicIcon(isRecording ? recordingMicIcon : defaultMicIcon);
        }

        private void Begin()
        {
            silenceTimer = 0f;
            microphoneWebGL.Begin();
        }

        private void End()
        {
            microphoneWebGL.End();
        }

        public void OnBegin()
        {
            int freq = microphoneWebGL.selectedDevice.sampleRate;
            _buffer = new float[(int)(freq * maxDuration)];
            _bufferSize = 0;
        }

        public void OnEnd()
        {
            if (!audioSource) return;

            _clip = AudioClip.Create("uMicrophoneWebGL-Recorded", _bufferSize, 1, microphoneWebGL.selectedDevice.sampleRate, false);
            _clip.SetData(_buffer, 0);

            if (_clip != null)
            {
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

                if (silenceTimer >= silenceDuration)
                {
                    ToggleRecord();
                    return;
                }
            }
            else
            {
                silenceTimer = 0f;
            }

            if (_bufferSize + input.Length > _buffer.Length) return;

            Array.Copy(input, 0, _buffer, _bufferSize, input.Length);
            _bufferSize += input.Length;
        }

        private bool IsSilent(float[] buffer)
        {
            float sum = 0f;
            for (int i = 0; i < buffer.Length; i++)
            {
                sum += buffer[i] * buffer[i];
            }
            return Mathf.Sqrt(sum / buffer.Length) < silenceThreshold;
        }

        public void OnDeviceListUpdated(List<Device> devices)
        {
            if (deviceDropdown == null) return;

            List<string> options = new List<string>();
            deviceLabelToIndexMap.Clear(); // Clear the existing map

            for (int i = 0; i < devices.Count; i++)
            {
                var device = devices[i];
                options.Add(device.label);
                deviceLabelToIndexMap[device.label] = i; // Map label to index
            }

            deviceDropdown.choices = options;
            deviceDropdown.value = deviceDropdown.choices.Count > 0 ? deviceDropdown.choices[0] : string.Empty;
        }

        private void SetMicIcon(Texture2D iconTexture)
        {
            micIcon.style.backgroundImage = new StyleBackground(iconTexture);
        }

        public AudioClip GetClip()
        {
            return _clip;
        }
    }
}
