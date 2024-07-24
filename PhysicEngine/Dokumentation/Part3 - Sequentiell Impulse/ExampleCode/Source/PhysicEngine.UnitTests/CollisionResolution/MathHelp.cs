using GraphicMinimal;

namespace PhysicEngine.UnitTests.CollisionResolution
{
    internal class MathHelp
    {
        //Rundet decimalPlaces Stellen nach dem Komma
        public static float Round(float f, int decimalPlaces)
        {
            return Convert.ToSingle(f.ToString("N" + decimalPlaces));
        }

        public static Vector2D Round(Vector2D v, int decimalPlaces)
        {
            return new Vector2D(Round(v.X, decimalPlaces), Round(v.Y, decimalPlaces));
        }

        public static float[] Round(float[] v, int decimalPlaces)
        {
            return v.ToList().Select(x => Round(x, decimalPlaces)).ToArray();
        }
    }
}
