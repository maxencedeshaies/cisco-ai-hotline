using Hotline.Application;
using NAudio.Wave;

namespace Hotline.Audio;

public sealed class WindowsAudio : IAudio
{
    public IRecording StartRecording(string path) => new Recording(path);
    public void Play(byte[] wav)
    {
        using var stream = new MemoryStream(wav);
        using var reader = new WaveFileReader(stream);
        using var output = new WaveOutEvent();
        using var done = new ManualResetEventSlim();
        Exception? error = null;
        
        output.PlaybackStopped += (_, e) =>
        {
            error = e.Exception;
            done.Set();
        };
        
        output.Init(reader);
        output.Play();
        done.Wait();
        
        if (error != null)
        {
            throw new InvalidOperationException("Playback failed.", error);
        }
    }
    private sealed class Recording : IRecording
    {
        private readonly WaveInEvent input = new() { WaveFormat = new WaveFormat(16000, 16, 1) };
        private readonly WaveFileWriter writer;
        private readonly ManualResetEventSlim done = new();
        private Exception? error;
        private bool stopped;
        
        public Recording(string path)
        {
            try
            {
                writer = new WaveFileWriter(path, input.WaveFormat);
            }
            catch
            {
                input.Dispose();
                done.Dispose();
                throw;
            }
            input.DataAvailable += (_, e) => writer.Write(e.Buffer, 0, e.BytesRecorded);
            input.RecordingStopped += (_, e) =>
            {
                error = e.Exception;
                done.Set();
            };
            try
            {
                input.StartRecording();
            }
            catch
            {
                input.Dispose();
                writer.Dispose();
                done.Dispose();
                throw;
            }
        }
        public void Stop()
        {
            if (stopped)
            {
                return;
            }

            input.StopRecording();
            done.Wait();
            stopped = true;
            
            if (error != null)
            {
                throw new InvalidOperationException("Recording failed.", error);
            }

            if (writer.Length == 0)
            {
                throw new InvalidOperationException("No microphone audio was captured.");
            }
        }
        public void Dispose()
        {
            try
            {
                Stop();
            }
            finally
            {
                input.Dispose();
                writer.Dispose();
                done.Dispose();
            }
        }
    }
}
