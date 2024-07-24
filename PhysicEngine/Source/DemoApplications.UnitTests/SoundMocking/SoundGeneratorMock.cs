using DemoApplications.UnitTests.Logging;
using SoundEngine;
using SoundEngine.SoundSnippeds;

namespace DemoApplications.UnitTests.SoundMocking
{
    internal class SoundGeneratorMock : ISoundGenerator
    {
        private ILogger log = new Logger();

        public int SampleRate => throw new NotImplementedException();

        public float Volume {get; set; }

        public IFrequenceToneSnipped AddFrequencyTone()
        {
            throw new NotImplementedException();
        }

        public IFrequenceToneSnipped AddFrequencyTone(string syntiFile)
        {
            throw new NotImplementedException();
        }

        public IAudioFileSnipped AddMusicFile(string musicFile)
        {
            return new AudioFileSnippedMock(musicFile, this.log);
        }

        public IAudioFileSnipped AddSoundFile(string audioFile)
        {
            return new AudioFileSnippedMock(audioFile, this.log);
        }

        public IFrequenceToneSnipped[] AddSynthSoundCollection(string musicFile)
        {
            return new IFrequenceToneSnipped[] { new FrequenceToneSnippedMock(musicFile, this.log) };
        }

        public void Dispose()
        {
            this.log.AddMessage("SoundGenerator", "Dispose");
        }

        public IFrequenceToneSnipped[] GetAllFrequenceTones()
        {
            throw new NotImplementedException();
        }

        public string GetLoggingText()
        {
            return this.log.GetAllMessages();
        }
    }
}
