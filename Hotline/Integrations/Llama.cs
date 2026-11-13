using System.Text;
using System.Text.Json;
using Hotline.Application;

namespace Hotline.Integrations;

public sealed class Llama(HttpClient client, string url) : IResponder
{
    public async Task<string> RespondAsync(IReadOnlyList<Message> messages)
    {
        using var content = new StringContent(JsonSerializer.Serialize(new { messages, max_tokens = 160 }), Encoding.UTF8, "application/json");
        using var response = await client.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        string answer = json.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString()?.Trim() ?? "";
        
        if (answer.Length == 0)
        {
            throw new InvalidOperationException("The language model returned an empty response.");
        }

        return answer;
    }
}
