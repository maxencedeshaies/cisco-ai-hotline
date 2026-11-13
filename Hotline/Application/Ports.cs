namespace Hotline.Application;

public record Message(string role, string content);
public interface ITranscriber
{
    Task<string> TranscribeAsync(string audio, string directory);
}

public interface ISynthesizer
{
    Task SynthesizeAsync(string text, string output);
}

public interface IResponder
{
    Task<string> RespondAsync(IReadOnlyList<Message> messages);
}

public interface IRecording : IDisposable
{
    void Stop();
}

public interface IAudio
{
    IRecording StartRecording(string path);
    void Play(byte[] wav);
}
