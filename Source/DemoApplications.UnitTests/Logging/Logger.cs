namespace DemoApplications.UnitTests.Logging
{
    internal class Logger : ILogger
    {
        class Message
        {
            public string Sender;
            public string Text;

            public Message(string sender, string text) 
            { 
                Sender = sender;
                Text = text;
            }

            public override string ToString()
            {
                return Sender + "\t" + Text;
            }
        }
        private List<Message> messages = new List<Message>();
        public void AddMessage(string sender, string text)
        {
            this.messages.Add(new Message(sender, text));
        }

        public string GetAllMessages()
        {
            return string.Join("\n", this.messages);
        }
    }
}
