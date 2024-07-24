using SoundEngine;
using SoundEngine.SoundSnippeds;

namespace CarDrifterControl.Model
{
    internal class Sounds
    {
        private ISoundGenerator soundGenerator;          //Sound-Wiedergabe
        private IFrequenceToneSnipped brake;
        public Sounds(ISoundGenerator soundGenerator, string dataFolder)
        {
            this.soundGenerator = soundGenerator;
            this.soundGenerator.Volume = 0.05f;

            this.brake = this.soundGenerator.AddSynthSoundCollection(dataFolder + "Brake.music")[0];
        }

        public void StartBrake()
        {
            this.brake.Play();
        }

        public void StopBrake()
        {
            this.brake.Stop();
        }
    }
}
