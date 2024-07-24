using SoundEngine.SoundSnippeds;
using SoundEngine;

namespace MoonlanderControl.Model
{
    internal class Sounds
    {
        private ISoundGenerator soundGenerator;          //Sound-Wiedergabe
        private IFrequenceToneSnipped mainThrusterSound;
        private IAudioFileSnipped explodeSound;
        private IAudioFileSnipped introSound;

        public Sounds(ISoundGenerator soundGenerator, string dataFolder)
        {
            this.soundGenerator = soundGenerator;
            this.soundGenerator.Volume = 0.1f;
            this.mainThrusterSound = this.soundGenerator.AddSynthSoundCollection(dataFolder + "Moonlander_MainThruster.music")[0];
            this.explodeSound = this.soundGenerator.AddMusicFile(dataFolder + "Moonlander_Explosion.music");
            this.introSound = this.soundGenerator.AddMusicFile(dataFolder + "Moonlander_Intro.music");
        }

        public void StartMainThrusterSound()
        {
            this.mainThrusterSound.Play();
        }

        public void StopMainThrusterSound()
        {
            this.mainThrusterSound.Stop();
        }

        public void PlayExplosionSound()
        {
            this.explodeSound.Play();
        }

        public void PlayIntroSound()
        {
            this.introSound.Play();
        }

        public void StopIntroSound()
        {
            this.introSound.Stop();
        }
    }
}
