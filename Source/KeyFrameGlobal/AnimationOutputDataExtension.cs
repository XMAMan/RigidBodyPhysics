namespace KeyFrameGlobal
{
    public static class AnimationOutputDataExtension
    {
        //Newtonsoft wandelt ein float in double (beim Laden oder Speichern um)
        public static AnimationOutputData ReplaceDoublesWithFloats(this AnimationOutputData data)
        {
            foreach (var frame in data.Frames)
            {
                for (int i = 0; i < frame.Values.Length; i++)
                {
                    if (frame.Values[i] is double)
                    {
                        double d = (double)frame.Values[i];
                        frame.Values[i] = (float)d;
                    }
                }
            }

            return data;
        }
    }
}
