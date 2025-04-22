using GraphicPanelWpf;

namespace KeyboardRecordAndPlay
{
    //Spielt die aufgezeichneten Tastendrücke ab
    public class KeyBoardPlayer : ITimerHandler
    {
        private KeyBoardPlayerConstructorData data;
        private KeyEvents nextEvent = null;
        private int index = -1;

        public bool IsFinish { get; private set; } = false;
        public int TimerTickCounter { get; private set; } = 0;

        public string GetElapsedTime(float dt)
        {
            return TimerTickConverter.ToString(TimerTickCounter, dt);
        }

        public KeyBoardPlayer(KeyBoardPlayerConstructorData data)
        {
            this.data = data;
            Reset();
        }

        public void Reset()
        {
            this.IsFinish = false;
            this.TimerTickCounter = 0;
            if (data.RecordData.KeyEvents.Length > 0)
            {
                this.nextEvent = data.RecordData.KeyEvents[0];
                this.index = 0;                
            }
            else
            {
                this.nextEvent = null;
                this.index = -1;
            }
        }

        public void HandleTimerTick(float dt)
        {
            if (this.IsFinish) return;

            if (this.nextEvent != null && this.nextEvent.TimerTick == this.TimerTickCounter)
            {
                ReplayKeys(this.nextEvent);
                this.index++;
                if (this.index < data.RecordData.KeyEvents.Length)
                {
                    this.nextEvent = data.RecordData.KeyEvents[index];
                }
                else
                {
                    this.nextEvent = null;
                }
            }

            this.TimerTickCounter++;

            if (this.TimerTickCounter >= this.data.RecordData.TimerTicks)
            {
                this.IsFinish = true;
                this.data.IsFinish();
            }
        }

        private void ReplayKeys(KeyEvents keyEvents)
        {
            foreach (var keyEvent in keyEvents.Events)
            {
                if (keyEvent.IsKeyDown)
                    this.data.KeyDownAction(keyEvent.Key);
                else
                    this.data.KeyUpAction(keyEvent.Key);
            }
        }
    }

    public class KeyBoardPlayerConstructorData
    {
        public KeyBoardRecordData RecordData { get; set; }
        public Action<System.Windows.Input.Key> KeyDownAction { get; set; }
        public Action<System.Windows.Input.Key> KeyUpAction { get; set; }
        public Action IsFinish { get; set; }
    }
}
