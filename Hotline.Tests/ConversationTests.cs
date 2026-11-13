using Hotline;
using Hotline.Integrations;
using Hotline.Application;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class ConversationTests
{
    [Fact]
    public async Task HistoryIsOnlyKeptAfterCommit()
    {
        var responder = new FakeResponder();
        var conversation = new Conversation(new SessionLanguage { Value = "fr" }, responder);
        await conversation.RespondAsync("discarded");
        var pending = await conversation.RespondAsync("bonjour");
        Assert.DoesNotContain(responder.Last, m => m.content == "discarded");
        pending.Commit();
        await conversation.RespondAsync("encore");
        Assert.Contains(responder.Last, m => m.content == "bonjour");
        Assert.Contains(responder.Last, m => m.content == "salut");
    }
    [Theory]
    [InlineData("http://example.com/")]
    [InlineData("http://localhost@example.com/")]
    [InlineData("https://localhost/")]
    public void RejectsNonlocalEndpoints(string url) => Assert.Throws<ArgumentException>(() => new Config { LlamaUrl = url }.Validate());

    [Fact]
    public void ScopesHaveIndependentSessions()
    {
        using var provider = ServiceRegistration.Build(new Config { LlamaUrl = "http://localhost:8080/" });
        using var first = provider.CreateScope();
        using var second = provider.CreateScope();
        first.ServiceProvider.GetRequiredService<SessionLanguage>().Value = "en";
        second.ServiceProvider.GetRequiredService<SessionLanguage>().Value = "fr";
        Assert.NotSame(first.ServiceProvider.GetRequiredService<Conversation>(), second.ServiceProvider.GetRequiredService<Conversation>());
        Assert.Equal("en", first.ServiceProvider.GetRequiredService<SessionLanguage>().Value);
        Assert.Equal("fr", second.ServiceProvider.GetRequiredService<SessionLanguage>().Value);
    }

    private sealed class FakeResponder : IResponder
    {
        public IReadOnlyList<Message> Last = Array.Empty<Message>();
        public Task<string> RespondAsync(IReadOnlyList<Message> messages)
        {
            Last = messages;
            return Task.FromResult("salut");
        }
    }
}
