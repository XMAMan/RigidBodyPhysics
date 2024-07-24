using GraphicPanelWpf;
using static KeyFrameGlobal.AnimationOutputData;

namespace KeyFrameGlobal
{
    //Hat eine TimeStep-Methode. Merkt sich den IsRunning-Zustand und wie viele Frames die Animation lang ist. Gibt die TimePostion (0..1) aus.
    public class FrameToTimeConverter : ITimerHandler
    {
        public long FrameAnimationCount { get; set; } // So viele Frames ist die Animation lang (Vorgabewert von außen)
        public AnimationType AnimationType { get; set; } //Vorgabewert von außen

        public float Time { get; private set; } //Aktuelle Position von 0..1 (Dieser Wert von von dieser Klasse berechent)

        private int currentFrame = 0;

        #region If AnimationType is Manually
        private float manualPositionStep = 0; //Um diese Schrittweite wird die Time-Variable pro TimerStep erhöht/verringert (Wenn AnimationType==Manually)
        //Wenn der AnimationType manuell ist und jemand die Animation rückwärts laufen lassen will indem er den PlayBackwards-Button gedrückt hält
        public bool PlayBackwards
        {
            set
            {
                if (value)
                    this.manualPositionStep = -1f / this.FrameAnimationCount;
                else
                    this.manualPositionStep = 0;
            }
        }

        public bool PlayForward
        {
            set
            {
                if (value)
                    this.manualPositionStep = 1f / this.FrameAnimationCount;
                else
                    this.manualPositionStep = 0;
            }
        }

        #endregion

        public FrameToTimeConverter(long frameAnimationCount, AnimationType animationType, float initialTime)
        {
            this.FrameAnimationCount = frameAnimationCount;
            this.AnimationType = animationType;
            this.Time = initialTime;
            this.currentFrame = (int)(frameAnimationCount * initialTime);
        }

        public void HandleTimerTick(float dt)
        {
            if (this.AnimationType == AnimationType.Manually)
            {
                this.Time += this.manualPositionStep;
                this.Time = Math.Min(1, Math.Max(0, this.Time));
            }
            else
            {
                this.currentFrame++;
                if (this.currentFrame > this.FrameAnimationCount)
                {
                    if (this.AnimationType == AnimationType.AutoLoop)
                        this.currentFrame = 0;
                    else
                        this.currentFrame = (int)this.FrameAnimationCount;
                }

                this.Time = this.currentFrame / (float)Math.Max(1, this.FrameAnimationCount);
            }
        }

        public void Reset()
        {
            this.currentFrame = 0;
            this.Time = 0;
        }
    }
}
