namespace KeyboardRecordAndPlay
{
    public static class TimerTickConverter
    {
        public static string ToString(int timerTicks, float dt)
        {
            var timeDiff = TimeSpan.FromMilliseconds(timerTicks * dt);
            return string.Format("{0:d2}:{1:d2}:{2:d3}", timeDiff.Minutes, timeDiff.Seconds, timeDiff.Milliseconds);
        }
    }
}
