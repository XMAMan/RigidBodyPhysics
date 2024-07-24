using DemoApplications.UnitTests.Logging;
using SoundEngine.SoundSnippeds;

namespace DemoApplications.UnitTests.SoundMocking
{
    internal class AudioFileSnippedMock : IAudioFileSnipped
    {
        private string name;
        private ILogger log;
        public AudioFileSnippedMock(string name, ILogger log)
        {
            this.name = name;
            this.log = log;
        }

        public bool AutoLoop { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool IsRunning => throw new NotImplementedException();

        public Action<bool> IsRunningChanged { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public float Volume { get; set; }

        public void Play()
        {
            this.log.AddMessage(this.name, "Play");
        }

        public void Stop()
        {
            this.log.AddMessage(this.name, "Stop");
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
