using GraphicPanelWpf;

namespace KeyboardRecordAndPlay
{
    //Zeichnet all die Keydown- und Keyupevents auf, die zwischen zwei Timer-Tick-Signalen passiert sind
    public class KeyboardRecorder : ITimerHandler
    {
        private List<KeyEvent> keyEvents = new List<KeyEvent>();
        private List<KeyEvents> timerEvents = new List<KeyEvents>();

        public int TimerTickCounter { get; private set; } = 0;

        public string GetElapsedTime(float dt)
        {
            return TimerTickConverter.ToString(TimerTickCounter, dt);
        }

        public void Reset()
        {
            this.TimerTickCounter = 0;
            this.keyEvents.Clear();
            this.timerEvents.Clear();
        }
        public void HandleTimerTick(float dt)
        {
            if (this.keyEvents.Any())
            {
                var timerEvent = new KeyEvents(this.TimerTickCounter, this.keyEvents.ToArray());
                this.timerEvents.Add(timerEvent);
                this.keyEvents.Clear();
            }
            this.TimerTickCounter++;
        }

        public void AddKeyDownEvent(System.Windows.Input.Key key)
        {
            this.keyEvents.Add(new KeyEvent(key, true));
        }

        public void AddKeyUpEvent(System.Windows.Input.Key key)
        {
            this.keyEvents.Add(new KeyEvent(key, false));
        }

        public KeyBoardRecordData GetRecordedData()
        {
            return new KeyBoardRecordData()
            {
                KeyEvents = this.timerEvents.ToArray(),
                TimerTicks = this.TimerTickCounter
            };
        }
    }
}
