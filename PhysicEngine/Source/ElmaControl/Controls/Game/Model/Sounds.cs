using SoundEngine;
using SoundEngine.SoundSnippeds;

namespace ElmaControl.Controls.Game.Model
{
    internal class Sounds
    {
        private ISoundGenerator soundGenerator;          //Sound-Wiedergabe
        private IFrequenceToneSnipped motor;
        private IAudioFileSnipped collectAppel;
        private IAudioFileSnipped collectFlower;
        private IAudioFileSnipped rotateArms;
        private IAudioFileSnipped spinDirection;
        private IAudioFileSnipped wallCrash;

        public Sounds(ISoundGenerator soundGenerator, string dataFolder)
        {
            this.soundGenerator = soundGenerator;
            this.soundGenerator.Volume = 0.01f;
         
            this.motor = this.soundGenerator.AddSynthSoundCollection(dataFolder + "Motor.music")[0];
            this.collectAppel = this.soundGenerator.AddSoundFile(dataFolder + "CollectAppel.mp3");
            this.collectFlower = this.soundGenerator.AddSoundFile(dataFolder + "CollectFlower.mp3");
            this.rotateArms = this.soundGenerator.AddSoundFile(dataFolder + "RotateArms.mp3");
            this.spinDirection = this.soundGenerator.AddSoundFile(dataFolder + "SpinDirection.mp3");
            this.wallCrash = this.soundGenerator.AddSoundFile(dataFolder + "WallCrash.mp3");

            this.spinDirection.Volume = 0.5f;
        }

        public float MotorFrequency { get => this.motor.Frequency; set => this.motor.Frequency = value; }

        public void StartMotor()
        {
            this.motor.Play();
        }

        public void StopMotor()
        {
            this.motor.Stop();
        }

        public void PlayCollectAppel()
        {
            this.collectAppel.Play();
        }

        public void PlayCollectFlower()
        {
            this.collectFlower.Play();
        }

        public void PlayRotateArms()
        {
            this.rotateArms.Play();
        }

        public void PlaySpinDirection()
        {
            this.spinDirection.Play();
        }

        public void PlayWallCrash()
        {
            this.wallCrash.Play();
        }
    }
}
