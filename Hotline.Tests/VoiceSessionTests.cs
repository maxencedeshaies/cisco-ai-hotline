using Hotline.Application;
using Xunit;
public class VoiceSessionTests
{
    [Fact]
    public async Task FailedSynthesisCleansUpAndDoesNotCommitTurn()
    {
        var transcriber = new FakeTranscriber();
        var responder = new FakeResponder();
        var synthesizer = new FakeSynthesizer { Fail = true };
        var session = new VoiceSession(transcriber, synthesizer, new Conversation(new SessionLanguage { Value = "en" }, responder));
        await Assert.ThrowsAsync<InvalidOperationException>(() => session.RunTurnAsync("input"));
        Assert.False(Directory.Exists(transcriber.Directory));
        synthesizer.Fail = false;
        await session.RunTurnAsync("input");
        Assert.Equal(2, responder.Last.Count);
        Assert.False(Directory.Exists(transcriber.Directory));
    }
    private sealed class FakeTranscriber : ITranscriber
    {
        public string Directory = "";
        public Task<string> TranscribeAsync(string audio, string directory)
        {
            Directory = directory;
            return Task.FromResult("hello");
        }
    }
    private sealed class FakeResponder : IResponder
    {
        public IReadOnlyList<Message> Last = Array.Empty<Message>();
        public Task<string> RespondAsync(IReadOnlyList<Message> messages)
        {
            Last = messages;
            return Task.FromResult("hi");
        }
    }
    private sealed class FakeSynthesizer : ISynthesizer
    {
        public bool Fail;
        public Task SynthesizeAsync(string text, string output)
        {
            if (Fail)
            {
                throw new InvalidOperationException();
            }

            return File.WriteAllBytesAsync(output, new byte[] { 1 });
        }
    }
}
