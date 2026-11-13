using Hotline.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Hotline;

public sealed class ConsoleApplication(IServiceScopeFactory scopes, IAudio audio)
{
    public async Task<int> RunAsync()
    {
        Console.Write("Language / Langue (en/fr): ");

        string language = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();

        if (language is not ("en" or "fr"))
        {
            throw new ArgumentException("Choose en or fr.");
        }

        using var scope = scopes.CreateScope();
        scope.ServiceProvider.GetRequiredService<SessionLanguage>().Value = language;
        var session = scope.ServiceProvider.GetRequiredService<VoiceSession>();

        while (true)
        {
            Console.Write("Press Enter to speak, or q to quit: ");
            string? command = Console.ReadLine();

            if (command is null || command.Trim().Equals("q", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            string directory = Path.Combine(Path.GetTempPath(), "hotline-" + Guid.NewGuid().ToString("N"));

            try
            {
                Directory.CreateDirectory(directory);
                string input = Path.Combine(directory, "input.wav");

                using (var recording = audio.StartRecording(input))
                {
                    Console.Write("Recording. Press Enter to stop: ");
                    Console.ReadLine();
                    recording.Stop();
                }

                var result = await session.RunTurnAsync(input);

                if (result is null)
                {
                    Console.WriteLine("No speech recognized; try again.");
                    continue;
                }

                Console.WriteLine($"You: {result.Transcript}\nAssistant: {result.Answer}");
                Console.WriteLine(
                    $"Seconds: transcription {result.Transcription.TotalSeconds:F2}, generation {result.Generation.TotalSeconds:F2}, synthesis {result.Synthesis.TotalSeconds:F2}");
                audio.Play(result.Audio);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Turn failed: {ex.Message}");
            }
            finally
            {
                try
                {
                    if (Directory.Exists(directory))
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

        return 0;
    }
}
