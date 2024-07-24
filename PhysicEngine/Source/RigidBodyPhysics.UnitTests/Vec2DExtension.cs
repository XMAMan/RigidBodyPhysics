using RigidBodyPhysics.MathHelper;

namespace RigidBodyPhysics.UnitTests
{
    internal static class Vec2DExtension
    {
        public static string ToIntString(this Vec2D v)
        {
            return (int)v.X + "_" + (int)v.Y;
        }
    }
}
