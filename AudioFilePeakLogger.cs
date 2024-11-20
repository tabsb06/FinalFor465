
using NAudio.Wave;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using NAudio.Wave;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

public class AudioFilePeakLogger
{
    readonly double[] AudioValues;
    readonly double[] FftValues;
    readonly int sampleRate;
    readonly int bytesPerSample;
    readonly WaveFormat waveFormat;
    

    readonly string Filepath = "";

    public AudioFilePeakLogger(string mp3FilePath)
    {
        // Read the audio file
        Filepath = mp3FilePath;
        using var reader = new AudioFileReader(Filepath);
        waveFormat = reader.WaveFormat;
        sampleRate = waveFormat.SampleRate;
        bytesPerSample = waveFormat.BitsPerSample / 8;

        //Prep work for processing using FFT
        AudioValues = new double[sampleRate / 1];  // 10ms chunks
        double[] paddedAudio = FftSharp.Pad.ZeroPad(AudioValues);
        double[] fftMag = FftSharp.Transform.FFTpower(paddedAudio);
        FftValues = new double[fftMag.Length];
    }

    public void ProcessFile(string outputLogFile, IProgress<int> progress)
    {
        // Clear the log file before starting
        File.WriteAllText(outputLogFile, string.Empty);

        using var reader = new AudioFileReader(Filepath);
        long totalBytes = reader.Length;
        long bytesProcessed = 0;

        byte[] buffer = new byte[AudioValues.Length * bytesPerSample];
        int bytesRead;
        int sampleIndex = 0;

        while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
        {
            int bufferSampleCount = bytesRead / bytesPerSample;

            // Call ProcessChunk with the outputLogFile path
            ProcessChunk(buffer, bufferSampleCount, sampleIndex, outputLogFile);

            sampleIndex += bufferSampleCount;
            bytesProcessed += bytesRead;

            int percentComplete = (int)((double)bytesProcessed / totalBytes * 100);
            progress?.Report(percentComplete);
        }
    }



    private void ProcessChunk(byte[] buffer, int bufferSampleCount, int sampleIndex, string logFilePath)
    {


        if (waveFormat.Encoding == WaveFormatEncoding.Pcm)
        {
            if (bytesPerSample == 2) // 16-bit PCM
            {
                for (int i = 0; i < bufferSampleCount; i++)
                    AudioValues[i] = BitConverter.ToInt16(buffer, i * bytesPerSample);
            }
            else if (bytesPerSample == 4) // 32-bit PCM
            {
                for (int i = 0; i < bufferSampleCount; i++)
                    AudioValues[i] = BitConverter.ToInt32(buffer, i * bytesPerSample);
            }
        }
        else if (waveFormat.Encoding == WaveFormatEncoding.IeeeFloat && bytesPerSample == 4) // 32-bit float
        {
            for (int i = 0; i < bufferSampleCount; i++)
                AudioValues[i] = BitConverter.ToSingle(buffer, i * bytesPerSample);
        }
        else
        {
            throw new NotSupportedException(waveFormat.ToString());
        }

        double[] paddedAudio = FftSharp.Pad.ZeroPad(AudioValues);
        double[] fftMag = FftSharp.Transform.FFTmagnitude(paddedAudio);

        int peakIndex = 0;
        for (int i = 0; i < fftMag.Length; i++)
        {
            if (fftMag[i] > fftMag[peakIndex])
                peakIndex = i;
        }

        double fftPeriod = FftSharp.Transform.FFTfreqPeriod(sampleRate, fftMag.Length);
        double peakFrequency = fftPeriod * peakIndex;
        double currentTime = (double)sampleIndex / sampleRate;

        const double minPianoFrequency = 27.5;  // A0
        const double maxPianoFrequency = 4186.0; // C8

        if (peakFrequency >= minPianoFrequency && peakFrequency <= maxPianoFrequency)
        {

            string logMessage = $"Timestamp: {currentTime:N2} sec, Peak Frequency: {peakFrequency:N2} Hz{Environment.NewLine}";
            File.AppendAllText(logFilePath, logMessage);
        }

    }

}


public class AudioFileLoudness
{
    readonly double[] AudioValues;
    readonly int sampleRate;
    readonly int bytesPerSample;
    readonly WaveFormat waveFormat;
    readonly string Filepath;

    public AudioFileLoudness(string mp3FilePath)
    {
        Filepath = mp3FilePath;

        using var reader = new AudioFileReader(Filepath);
        waveFormat = reader.WaveFormat;
        sampleRate = waveFormat.SampleRate;
        bytesPerSample = waveFormat.BitsPerSample / 8;

        // Initialize a buffer for FFT processing (1024 samples per chunk)
        AudioValues = new double[1024];
    }

    public void ProcessFile(string outputLogFile, IProgress<int> progress)
    {
        // Clear the log file before starting
        File.WriteAllText(outputLogFile, string.Empty);

        using var reader = new AudioFileReader(Filepath);
        long totalBytes = reader.Length;
        long bytesProcessed = 0;

        byte[] buffer = new byte[AudioValues.Length * bytesPerSample];
        int bytesRead;
        int sampleIndex = 0;

        while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
        {
            int bufferSampleCount = bytesRead / bytesPerSample;
            ProcessChunk(buffer, bufferSampleCount, sampleIndex, outputLogFile);

            sampleIndex += bufferSampleCount;
            bytesProcessed += bytesRead;

            int percentComplete = (int)((double)bytesProcessed / totalBytes * 100);
            progress?.Report(percentComplete);
        }
    }

    private void ProcessChunk(byte[] buffer, int bufferSampleCount, int sampleIndex, string logFilePath)
    {
        if (waveFormat.Encoding == WaveFormatEncoding.Pcm)
        {
            if (bytesPerSample == 2) // 16-bit PCM
            {
                for (int i = 0; i < bufferSampleCount; i++)
                    AudioValues[i] = BitConverter.ToInt16(buffer, i * bytesPerSample);
            }
            else if (bytesPerSample == 4) // 32-bit PCM
            {
                for (int i = 0; i < bufferSampleCount; i++)
                    AudioValues[i] = BitConverter.ToInt32(buffer, i * bytesPerSample);
            }
        }
        else if (waveFormat.Encoding == WaveFormatEncoding.IeeeFloat && bytesPerSample == 4) // 32-bit float
        {
            for (int i = 0; i < bufferSampleCount; i++)
                AudioValues[i] = BitConverter.ToSingle(buffer, i * bytesPerSample);
        }
        else
        {
            throw new NotSupportedException(waveFormat.ToString());
        }

        // Perform FFT
        double[] paddedAudio = FftSharp.Pad.ZeroPad(AudioValues);
        double[] fftMag = FftSharp.Transform.FFTmagnitude(paddedAudio);

        int peakIndex = fftMag.ToList().IndexOf(fftMag.Max());
        double fftPeriod = FftSharp.Transform.FFTfreqPeriod(sampleRate, fftMag.Length);
        double peakFrequency = fftPeriod * peakIndex;
        double currentTime = (double)sampleIndex / sampleRate;

        // Calculate RMS (loudness)
        double rms = Math.Sqrt(AudioValues.Average(x => x * x));
        double loudnessDb = 20 * Math.Log10(rms);

        // Define piano note range
        const double minPianoFrequency = 27.5;  // A0
        const double maxPianoFrequency = 4186.0; // C8

        if (peakFrequency >= minPianoFrequency && peakFrequency <= maxPianoFrequency)
        {
            // Prepare log message with timestamp, peak frequency, and loudness
            string logMessage = $"Timestamp: {currentTime:N2} sec, " +
                                $"Peak Frequency: {peakFrequency:N2} Hz, " +
                                $"Loudness: {loudnessDb:N2} dB{Environment.NewLine}";
            File.AppendAllText(logFilePath, logMessage);
        }
    }
}