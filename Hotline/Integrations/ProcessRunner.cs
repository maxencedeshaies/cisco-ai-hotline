using System.Diagnostics;
using System.Text;

namespace Hotline.Integrations;

public interface IProcessRunner
{
    Task RunAsync(string executable, IEnumerable<string> arguments, string? input = null);
}

public sealed class ProcessRunner : IProcessRunner
{
    public async Task RunAsync(string executable, IEnumerable<string> arguments, string? input = null)
    {
        var info = new ProcessStartInfo(executable) { UseShellExecute = false, RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, StandardInputEncoding = Encoding.UTF8, CreateNoWindow = true };
        foreach (var argument in arguments)
        {
            info.ArgumentList.Add(argument);
        }

        using var process = Process.Start(info) ?? throw new InvalidOperationException("Could not start " + executable);
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        if (input != null)
        {
            await process.StandardInput.WriteAsync(input);
        }

        process.StandardInput.Close();
        await process.WaitForExitAsync();
        await stdout;
        string error = await stderr;
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"{Path.GetFileName(executable)} failed ({process.ExitCode}): {error}");
        }
    }
}
