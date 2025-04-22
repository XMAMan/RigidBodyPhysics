using DemoApplications.UnitTests.Logging;
using SoundEngine.SoundSnippeds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveMaker.KeyboardComponents;

namespace DemoApplications.UnitTests.SoundMocking
{
    internal class MusicFileSnippedMock : IMusicFileSnipped
    {
        private string name;
        private ILogger log;
        public MusicFileSnippedMock(string name, ILogger log)
        {
            this.name = name;
            this.log = log;
        }


        public float KeyStrokeSpeed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int KeyShift { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool AutoLoop { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Action EndTrigger { get; set; }

        public bool IsRunning => throw new NotImplementedException();

        public Action<bool> IsRunningChanged { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public float Volume { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Action<ISoundSnipped> CopyWasCreated { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Action<ISoundSnipped> DisposeWasCalled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int SampleRate => throw new NotImplementedException();

        public void Dispose()
        {
            this.log.AddMessage(this.name, "Dispose");
        }

        public IMusicFileSnipped GetCopy()
        {
            this.log.AddMessage(this.name, "GetCopy");
            return this;
        }

        public float GetNextSample()
        {
            throw new NotImplementedException();
        }

        public Synthesizer GetSynthesizer(int index)
        {
            throw new NotImplementedException();
        }

        public void Play()
        {
            this.log.AddMessage(this.name, "Play");
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            this.log.AddMessage(this.name, "Stop");
        }
    }
}
