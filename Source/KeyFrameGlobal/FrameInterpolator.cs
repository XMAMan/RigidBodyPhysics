namespace KeyFrameGlobal
{
    //Bekommt eine Menge von Key-Frames und ermittelt für ein gegebenen Timewert (0..1) das interpolierte Frame
    public class FrameInterpolator
    {
        private IAnimationProperty[] properties; //Hat die Interpolate-Funktion für jede Property des zu animierenden Objektes
        private AnimationOutputData data; //Zustand des zu animierenden Objekts zum Zeitpunkt (t=0)
        public FrameInterpolator(IAnimationProperty[] properties, AnimationOutputData data)
        {
            this.properties = properties;
            this.data = data;
        }

        public FrameData GetFrame(float time)
        {
            if (data.Frames.Length == 1) return data.Frames[0];

            for (int i = 0; i < data.Frames.Length; i++)
            {
                if (data.Frames[i].Time > time)
                    return Interpolate(data.Frames[i - 1], data.Frames[i], time);
            }

            var copy = new FrameData(data.Frames.Last()); //Wiederhole das letzte Frame am Ende, wenn es nicht bei Time=1 endet
            copy.Time = time;
            return copy;
        }

        //time = Zeit die zwischen fram1 und frame2 liegt
        private FrameData Interpolate(FrameData frame1, FrameData frame2, float time)
        {
            float t = Math.Min(1, Math.Max(0, (time - frame1.Time) / (frame2.Time - frame1.Time)));

            object[] values = new object[this.properties.Length];
            for (int i = 0; i < values.Length; i++)
            {
                if (this.data.PropertyIsAnimated[i])
                    values[i] = this.properties[i].Interpolate(frame1.Values[i], frame2.Values[i], t);
            }

            return new FrameData(time, values);
        }
    }
}
