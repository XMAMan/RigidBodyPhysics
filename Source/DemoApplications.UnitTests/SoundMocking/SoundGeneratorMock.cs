using DemoApplications.UnitTests.Logging;
using SoundEngine;
using SoundEngine.SoundSnippeds;
using WaveMaker;

namespace DemoApplications.UnitTests.SoundMocking
{
    internal class SoundGeneratorMock : ISoundGenerator
    {
        private ILogger log = new Logger();

        public int SampleRate => throw new NotImplementedException();

        public float Volume {get; set; }
        public string SelectedOutputDevice { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IAudioRecorderSnipped AudioRecorder => throw new NotImplementedException();

        public IAudioFileWriter AudioFileWriter => throw new NotImplementedException();

        public event EventHandler<float[]> AudioOutputCallback;

        public IFrequenceToneSnipped AddFrequencyTone(string syntiFile)
        {
            throw new NotImplementedException();
        }

        public IMusicFileSnipped AddMusicFile(string musicFile)
        {
            return new MusicFileSnippedMock(musicFile, this.log);
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

        public string[] GetAvailableOutputDevices()
        {
            throw new NotImplementedException();
        }

        public string GetLoggingText()
        {
            return this.log.GetAllMessages();
        }
    }
}
