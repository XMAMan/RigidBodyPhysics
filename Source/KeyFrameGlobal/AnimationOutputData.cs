namespace KeyFrameGlobal
{
    public class AnimationOutputData
    {
        public enum AnimationType
        {
            OneTime,    // Animation wird per Timer einmal abgespielt. Beispiel: Person verschießt mit Bogen ein Pfeil
            AutoLoop,   // Animation wird per Timmer unendlich oft wiederholt. Beispiel: Person die läuft
            Manually    // Animation muss per KeyDown vor/zurück gespult werden. Beispiel: Skispringen
        }

        public FrameData[] Frames { get; set; }
        public float DurrationInSeconds { get; set; } //So lange ist die Animation in Sekunden
        public AnimationType Type { get; set; }
        public bool[] PropertyIsAnimated { get; set; }
        public float StartTime { get; set; } //0..1 Mit diesen Timewert beginnt die Animation

        public AnimationOutputData() { } //Für den Serialisierer

        public AnimationOutputData(FrameData[] frames, float durrationInSeconds, AnimationType type, bool[] propertyIsAnimated, float startTime)
        {
            Frames = frames;
            DurrationInSeconds = durrationInSeconds;
            Type = type;
            PropertyIsAnimated = propertyIsAnimated;
            StartTime = startTime;
        }

        public AnimationOutputData(AnimationOutputData copy)
        {
            this.Frames = new FrameData[copy.Frames.Length];
            for (int i = 0; i < this.Frames.Length; i++)
            {
                this.Frames[i] = new FrameData(copy.Frames[i]);
            }
            this.DurrationInSeconds = copy.DurrationInSeconds;
            this.Type = copy.Type;
            this.PropertyIsAnimated = new bool[copy.PropertyIsAnimated.Length];
            for (int i = 0; i < this.PropertyIsAnimated.Length; i++)
            {
                this.PropertyIsAnimated[i] = copy.PropertyIsAnimated[i];
            }
            this.StartTime = copy.StartTime;
        }
    }

    public class FrameData
    {
        public float Time { get; set; } //Zahl zwischen 0 und 1
        public object[] Values { get; set; } //Float oder Boolean

        public FrameData() { } //Für den Serialisierer

        public FrameData(float time, object[] values)
        {
            Time = Math.Min(1, Math.Max(0, time));
            Values = values;
        }

        public FrameData(FrameData copy)
        {
            this.Time = copy.Time;
            this.Values = copy.Values;
        }
    }
}
