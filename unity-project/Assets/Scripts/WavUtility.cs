using System;
using System.IO;
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

    // Method to convert audio samples into a WAV file byte array
    public static byte[] FromSamples(float[] samples, int channels, int sampleRate)
    {
        using (var memoryStream = new MemoryStream())
        {
            using (var writer = new BinaryWriter(memoryStream))
            {
                // Write the header
                writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
                writer.Write(36 + samples.Length * 2); // File size
                writer.Write(new char[4] { 'W', 'A', 'V', 'E' });
                writer.Write(new char[4] { 'f', 'm', 't', ' ' });
                writer.Write(16); // PCM chunk size
                writer.Write((short)1); // Format tag
                writer.Write((short)channels);
                writer.Write(sampleRate);
                writer.Write(sampleRate * channels * 2); // Average bytes per second
                writer.Write((short)(channels * 2)); // Block align
                writer.Write((short)16); // Bits per sample
                writer.Write(new char[4] { 'd', 'a', 't', 'a' });
                writer.Write(samples.Length * 2); // Data chunk size

                // Write the sample data
                foreach (var sample in samples)
                {
                    writer.Write((short)(sample * 32767)); // Convert to 16-bit
                }
            }

            return memoryStream.ToArray();
        }
    }
}
