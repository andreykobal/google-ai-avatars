using System;
using UnityEngine;

public static class WavUtility
{
    // Added an optional sampleRate parameter with a default value
    public static AudioClip FromBytes(byte[] wavFile, int sampleRate = 22050)
    {
        int dataOffset = 0;
        for (int i = 0; i < wavFile.Length - 4; i++)
        {
            if (wavFile[i] == 'd' && wavFile[i + 1] == 'a' && wavFile[i + 2] == 't' && wavFile[i + 3] == 'a')
            {
                dataOffset = i + 8;
                break;
            }
        }

        byte[] audioData = new byte[wavFile.Length - dataOffset];
        Buffer.BlockCopy(wavFile, dataOffset, audioData, 0, wavFile.Length - dataOffset);

        int sampleCount = audioData.Length / 2;
        AudioClip audioClip = AudioClip.Create("TTS", sampleCount, 1, sampleRate, false);
        float[] audioDataFloat = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            audioDataFloat[i] = BitConverter.ToInt16(audioData, i * 2) / 32768.0f;
        }

        audioClip.SetData(audioDataFloat, 0);
        return audioClip;
    }
}
