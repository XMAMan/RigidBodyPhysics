using SoundEngine;
using SoundEngine.SoundSnippeds;
using System;

namespace SkiJumperControl.Model
{
    internal class Sounds
    {
        private ISoundGenerator soundGenerator;          //Sound-Wiedergabe

        private IAudioFileSnipped intro;
        private IAudioFileSnipped ouch;
        private IAudioFileSnipped broke1;
        private IAudioFileSnipped broke2;
        private IAudioFileSnipped yes;
        private IAudioFileSnipped ohNo;

        private Random rand = new Random();

        public Sounds(ISoundGenerator soundGenerator, string dataFolder)
        {
            this.soundGenerator = soundGenerator;
            this.soundGenerator.Volume = 0.1f;

            this.intro = this.soundGenerator.AddMusicFile(dataFolder + "Intro.music");
            this.ouch = this.soundGenerator.AddSoundFile(dataFolder + "uh.mp3");
            this.broke1 = this.soundGenerator.AddSoundFile(dataFolder + "wood_crack1.mp3");
            this.broke2 = this.soundGenerator.AddSoundFile(dataFolder + "wood_crack2.mp3");
            this.yes = this.soundGenerator.AddSoundFile(dataFolder + "yes_2.wav.mp3");
            this.ohNo = this.soundGenerator.AddSoundFile(dataFolder + "Disgusted.mp3");

            this.broke1.Volume = 0.2f;
            this.broke2.Volume = 0.2f;
        }

        public void PlayIntro()
        {
            this.intro.Play();
        }

        public void StopIntro() 
        { 
            this.intro.Stop();
        }

        public void PlayBrokePlank()
        {
            if (rand.Next(2) == 0)
            {
                this.broke1.Play();
            }else
            {
                this.broke2.Play();
            }            
        }

        public void PlayOuch()
        {
            this.ouch.Play();
        }

        public void PlayYes()
        {
            this.yes.Play();
        }

        public void PlayOhNo()
        {
            this.ohNo.Play();
        }
    }
}
