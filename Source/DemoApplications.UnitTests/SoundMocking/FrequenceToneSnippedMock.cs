using DemoApplications.UnitTests.Logging;
using SoundEngine.SoundSnippeds;

namespace DemoApplications.UnitTests.SoundMocking
{
    internal class FrequenceToneSnippedMock : IFrequenceToneSnipped
    {
        private string name;
        private ILogger log;
        public FrequenceToneSnippedMock(string name, ILogger log)
        {
            this.name = name;
            this.log = log;
        }

        public float Frequency { get; set; }
        public float Pitch { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public WaveMaker.KeyboardComponents.Synthesizer Synthesizer => throw new NotImplementedException();

        public bool IsRunning => throw new NotImplementedException();

        public Action<bool> IsRunningChanged { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public float Volume {get; set; }
        public Action<ISoundSnipped> CopyWasCreated { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Action<ISoundSnipped> DisposeWasCalled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int SampleRate => throw new NotImplementedException();

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IFrequenceToneSnipped GetCopy()
        {
            throw new NotImplementedException();
        }

        public float GetNextSample()
        {
            throw new NotImplementedException();
        }

        public void Play()
        {
            this.log.AddMessage(this.name, "Play");
        }

        public void Stop()
        {
            this.log.AddMessage(this.name, "Stop");
        }
    }
}
