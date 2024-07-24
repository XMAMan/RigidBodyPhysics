using SoundEngine;
using SoundEngine.SoundSnippeds;

namespace AstroidsControl.Model
{
    internal class Sounds
    {
        private ISoundGenerator soundGenerator;          //Sound-Wiedergabe
        private IAudioFileSnipped shot;
        private IAudioFileSnipped explodeAstroid;
        private IAudioFileSnipped explodeSatellit;
        private IAudioFileSnipped particleExplode;
        private IFrequenceToneSnipped thrusterSound;

        public Sounds(ISoundGenerator soundGenerator, string dataFolder)
        {
            this.soundGenerator = soundGenerator;
            this.soundGenerator.Volume = 0.1f;
            this.shot = this.soundGenerator.AddMusicFile(dataFolder + "Shot.music");
            this.explodeAstroid = this.soundGenerator.AddMusicFile(dataFolder + "AstroidExplode.music");
            this.explodeSatellit = this.soundGenerator.AddMusicFile(dataFolder + "SatellitExplode.music");
            this.particleExplode = this.soundGenerator.AddMusicFile(dataFolder + "ParticleExplode.music");
            this.thrusterSound = this.soundGenerator.AddSynthSoundCollection(dataFolder + "Thruster.music")[0];
        }

        public void PlayShot()
        {
            this.shot.Play();
        }

        public void PlayExplodeAstroid()
        {
            this.explodeAstroid.Play();
        }

        public void PlayExplodeSatellit()
        {
            this.explodeSatellit.Play();
        }

        public void PlayParticleExplode()
        {
            this.particleExplode.Play();
        }

        public void StartThruster()
        {
            this.thrusterSound.Play();
        }

        public void StopThruster()
        {
            this.thrusterSound.Stop();
        }
    }
}
