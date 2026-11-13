using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hotline;

public sealed class Config
{
    [JsonPropertyName("whisper_exe")]
    public string WhisperExe { get; set; } = "";
    
    [JsonPropertyName("whisper_model")]
    public string WhisperModel { get; set; } = "";
    
    [JsonPropertyName("whisper_gpu")]
    public bool WhisperGpu { get; set; } = true;
    
    [JsonPropertyName("piper_exe")]
    public string PiperExe { get; set; } = "";
    
    [JsonPropertyName("voices")]
    public Dictionary<string, string> Voices { get; set; } = new();
    
    [JsonPropertyName("llama_url")]
    public string LlamaUrl { get; set; } = "";

    public static Config Load(string path)
    {
        var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(path)) ?? throw new ArgumentException("Configuration is empty.");
        config.Validate();
        return config;
    }

    public void Validate()
    {
        if (!Uri.TryCreate(LlamaUrl, UriKind.Absolute, out var url) || url.Scheme != Uri.UriSchemeHttp ||
            !(url.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || url.Host is "127.0.0.1" or "[::1]" or "::1") || !string.IsNullOrEmpty(url.UserInfo))
        {
            throw new ArgumentException("llama_url must be a local HTTP address.");
        }

        foreach (var path in new[] { WhisperExe, WhisperModel, PiperExe, Voices?.GetValueOrDefault("en"), Voices?.GetValueOrDefault("fr") })
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                throw new ArgumentException($"Configured file does not exist: {path}");
            }
        }
    }
}
