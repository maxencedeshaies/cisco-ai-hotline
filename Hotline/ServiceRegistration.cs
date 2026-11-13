using Hotline.Application;
using Hotline.Audio;
using Hotline.Integrations;
using Microsoft.Extensions.DependencyInjection;

namespace Hotline;

public static class ServiceRegistration
{
    public static ServiceProvider Build(Config config)
    {
        var services = new ServiceCollection();
        services.AddSingleton(config);
        services.AddSingleton(_ => new HttpClient(new HttpClientHandler { AllowAutoRedirect = false, UseProxy = false }) { Timeout = TimeSpan.FromSeconds(180) });
        services.AddSingleton<IProcessRunner, ProcessRunner>();
        services.AddSingleton<IAudio, WindowsAudio>();
        services.AddSingleton<ConsoleApplication>();
        services.AddScoped<SessionLanguage>();
        services.AddScoped<ITranscriber>(s => new Whisper(config, s.GetRequiredService<SessionLanguage>().Value, s.GetRequiredService<IProcessRunner>()));
        services.AddScoped<ISynthesizer>(s => new Piper(config, s.GetRequiredService<SessionLanguage>().Value, s.GetRequiredService<IProcessRunner>()));
        services.AddScoped<IResponder>(s => new Llama(s.GetRequiredService<HttpClient>(), config.LlamaUrl));
        services.AddScoped<Conversation>();
        services.AddScoped<VoiceSession>();
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }
}
