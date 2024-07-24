namespace DemoApplications.UnitTests.Logging
{
    internal interface ILogger
    {
        void AddMessage(string sender, string text);
        string GetAllMessages();
    }
}