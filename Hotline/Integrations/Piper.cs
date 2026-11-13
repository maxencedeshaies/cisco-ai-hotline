using Hotline.Application;

namespace Hotline.Integrations;

public sealed class Piper(Config config, string language, IProcessRunner runner) : ISynthesizer
{
    public Task SynthesizeAsync(string text, string output) => runner.RunAsync(config.PiperExe, new[] { "--model", config.Voices[language], "--output_file", output }, text);
}
