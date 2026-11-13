using Hotline;
using Microsoft.Extensions.DependencyInjection;

try
{
    string path = args.Length switch { 0 => "config.json", 2 when args[0] == "--config" => args[1], _ => throw new ArgumentException("Usage: Hotline [--config path]") };
    using var services = ServiceRegistration.Build(Config.Load(path));
    return await services.GetRequiredService<ConsoleApplication>().RunAsync();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Startup failed: {ex.Message}");
    return 1;
}
