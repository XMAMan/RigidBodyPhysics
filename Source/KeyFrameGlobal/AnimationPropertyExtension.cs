namespace KeyFrameGlobal
{
    public static class AnimationPropertyExtension
    {
        public static FrameData GetFrameFromAnimatedObject(this IAnimationProperty[] properties, float time)
        {
            return new FrameData(time, properties.Select(x => x.ObjValue).ToArray());
        }

        public static void WriteFrameToAnimatedObject(this IAnimationProperty[] properties, FrameData frame, bool[] propertyIsAnimated)
        {
            for (int i = 0; i < properties.Length; i++)
            {
                if (propertyIsAnimated[i])
                    properties[i].ObjValue = frame.Values[i];
            }
        }
    }
}
