using RigidBodyPhysics.MathHelper;

namespace RigidBodyPhysics.UnitTests.TestHelper
{
    internal class MathHelper
    {
        //Rundet decimalPlaces Stellen nach dem Komma
        public static float Round(float f, int decimalPlaces)
        {
            return Convert.ToSingle(f.ToString("N" + decimalPlaces));
        }

        public static Vec2D Round(Vec2D v, int decimalPlaces)
        {
            return new Vec2D(Round(v.X, decimalPlaces), Round(v.Y, decimalPlaces));
        }
    }
}
