using System.Diagnostics;

namespace Hotline.Application;

public sealed record TurnResult(string Transcript, string Answer, byte[] Audio, TimeSpan Transcription, TimeSpan Generation, TimeSpan Synthesis);
public sealed class VoiceSession(ITranscriber transcriber, ISynthesizer synthesizer, Conversation conversation)
{
    public async Task<TurnResult?> RunTurnAsync(string input)
    {
        string directory = Path.Combine(Path.GetTempPath(), "hotline-" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(directory);
            string output = Path.Combine(directory, "reply.wav");
            var clock = Stopwatch.StartNew();
            string text = await transcriber.TranscribeAsync(input, directory);
            var transcribed = clock.Elapsed;
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var pending = await conversation.RespondAsync(text);
            var generated = clock.Elapsed;
            await synthesizer.SynthesizeAsync(pending.Answer, output);
            byte[] audio = await File.ReadAllBytesAsync(output);
            var synthesized = clock.Elapsed;
            pending.Commit();
            return new(text, pending.Answer, audio, transcribed, generated - transcribed, synthesized - generated);
        }
        finally
        {
            try { if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Could not remove temporary files at {directory}: {ex.Message}");
            }
        }
    }
}
public sealed class SessionLanguage
{
    public string Value { get; set; } = "";
}
