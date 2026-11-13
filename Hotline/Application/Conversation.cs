namespace Hotline.Application;

public sealed class Conversation(SessionLanguage language, IResponder responder)
{
    private readonly List<Message> history = new() { new("system", "You are a friendly conversational hotline. Reply briefly in " + (language.Value == "en" ? "English." : "French.")) };
    public async Task<PendingResponse> RespondAsync(string text)
    {
        var messages = history.Append(new Message("user", text)).ToList();
        return new PendingResponse(this, messages, await responder.RespondAsync(messages));
    }
    public sealed class PendingResponse(Conversation owner, List<Message> messages, string answer)
    {
        private bool committed;
        public string Answer { get; } = answer;
        public void Commit()
        {
            if (committed)
            {
                throw new InvalidOperationException("Response already committed.");
            }

            committed = true;
            owner.history.Clear();
            owner.history.Add(messages[0]);
            owner.history.AddRange(messages.Skip(1).Append(new Message("assistant", Answer)).TakeLast(12));
        }
    }
}
