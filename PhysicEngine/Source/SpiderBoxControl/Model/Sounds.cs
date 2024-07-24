using SoundEngine.SoundSnippeds;
using SoundEngine;

namespace SpiderBoxControl.Model
{
    internal class Sounds
    {
        private ISoundGenerator soundGenerator;          //Sound-Wiedergabe
        private IFrequenceToneSnipped ropeLength; //Wird gespielt, wenn die Seillänge (bei MouseDown) sich ändert
        private IAudioFileSnipped throwRope;

        public Sounds(ISoundGenerator soundGenerator, string dataFolder)
        {
            this.soundGenerator = soundGenerator;
            this.soundGenerator.Volume = 0.1f;

            this.ropeLength = this.soundGenerator.AddSynthSoundCollection(dataFolder + "Sounds.music")[0];
            this.throwRope = this.soundGenerator.AddSoundFile(dataFolder + "bow.mp3");
            this.throwRope.Volume = 0.1f;
        }

        public float RopeFrequency { get => this.ropeLength.Frequency; set => this.ropeLength.Frequency = value; }

        public void StartRope()
        {
            this.ropeLength.Play();
        }

        public void StopRope()
        {
            this.ropeLength.Stop();
        }

        public void PlayThrowRope()
        {
            this.throwRope.Play();
        }
    }
}
