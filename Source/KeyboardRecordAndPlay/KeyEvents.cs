namespace KeyboardRecordAndPlay
{
    //All die Keyevents zwischen zwei dem Timer-Ticksignal Nummer TimerTick und TimerTick+1
    public class KeyEvents
    {
        public int TimerTick { get; set; } //Start-Timertick-Event
        public KeyEvent[] Events { get; set; }

        public KeyEvents() { }

        public KeyEvents(int timerTick, KeyEvent[] events)
        {
            this.TimerTick = timerTick;
            this.Events = events;
        }
    }
}
