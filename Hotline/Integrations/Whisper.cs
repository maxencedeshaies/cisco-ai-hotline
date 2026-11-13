using Hotline.Application;

namespace Hotline.Integrations;

public sealed class Whisper(Config config, string language, IProcessRunner runner) : ITranscriber
{
    public async Task<string> TranscribeAsync(string audio, string directory)
    {
        string output = Path.Combine(directory, "transcript");
        var args = new List<string> { "-m", config.WhisperModel, "-f", audio, "-l", language, "-otxt", "-of", output };
        if (!config.WhisperGpu)
        {
            args.Add("-ng");
        }

        await runner.RunAsync(config.WhisperExe, args);
        return (await File.ReadAllTextAsync(output + ".txt")).Trim();
    }
}
