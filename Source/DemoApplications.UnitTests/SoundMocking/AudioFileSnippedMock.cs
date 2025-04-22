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
        public double SampleIndex { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int SampleCount => throw new NotImplementedException();

        public float[] AudioFileSamples { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public float Pitch { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public float Speed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool IsFinish => throw new NotImplementedException();

        public Action EndTrigger { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Action<ISoundSnipped> CopyWasCreated { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Action<ISoundSnipped> DisposeWasCalled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int SampleRate => throw new NotImplementedException();

        public bool UseDelayEffect { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool UseHallEffect { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool UseGainEffect { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public float Gain { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool UsePitchEffect { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public float PitchEffect { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool UseVolumeLfo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public float VolumeLfoFrequency { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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

        public IAudioFileSnipped GetCopy()
        {
            this.log.AddMessage(this.name, "GetCopy");
            return this;
        }

        public float GetNextSample()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            this.log.AddMessage(this.name, "Dispose");
        }
    }
}
