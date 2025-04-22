namespace PhysicEngine.MathHelper
{
    internal static class MathHelp
    {
        public static float Clamp(float a, float low, float high)
        {
            return Math.Max(low, Math.Min(a, high));
        }

        //Rundet decimalPlaces Stellen nach dem Komma
        public static float Round(float f, int decimalPlaces)
        {
            return Convert.ToSingle(f.ToString("N" + decimalPlaces));
        }
    }
}
