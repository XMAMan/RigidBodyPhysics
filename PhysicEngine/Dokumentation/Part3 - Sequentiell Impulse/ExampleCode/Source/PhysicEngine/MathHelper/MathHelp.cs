namespace PhysicEngine.MathHelper
{
    internal static class MathHelp
    {
        public static float Clamp(float a, float low, float high)
        {
            return Math.Max(low, Math.Min(a, high));
        }        
    }
}
