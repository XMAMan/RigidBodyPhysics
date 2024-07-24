namespace GameHelper
{
    //Zählt die TimerTicks und führt eine Aktion nach N Sekunden aus
    public class TickCounterStopwatch
    {
        private float timerDtCounter = 0;
        private bool timerIsRunning = false;
        private float durationInMs;
        private Action action;

        public float Timer
        {
            get
            {
                return timerDtCounter / 1000f;
            }
        }

        public bool IsRunning { get => this.timerIsRunning; }

        //action = Diese Action wird durationInSeconds Sekunden nach Aufruf von StartTimer ausgeführt
        public void StartTimer(Action action, int durationInSeconds)
        {
            this.timerDtCounter = 0;
            this.action = action;
            this.timerIsRunning = true;
            this.durationInMs = durationInSeconds * 1000;
        }

        public void TimerTickHandler(float dt)
        {
            if (this.timerIsRunning == false) return;

            this.timerDtCounter += dt;

            if (this.timerDtCounter > this.durationInMs)
            {
                this.timerIsRunning = false;
                this.action();
            }
        }
    }
}
