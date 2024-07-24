using GraphicMinimal;
using System.Drawing;

namespace LevelToSimulatorConverter
{
    internal static class Vec2Extension
    {
        internal static RigidBodyPhysics.MathHelper.Vec2D ToPhx(this GraphicMinimal.Vector2D v)
        {
            return new RigidBodyPhysics.MathHelper.Vec2D(v.X, v.Y);
        }

        internal static PointF ToPointF(this Vector2D v)
        {
            return new PointF(v.X, v.Y);
        }

        internal static Vector2D ToGrx(this PointF p)
        {
            return new Vector2D(p.X, p.Y);
        }
    }
}
