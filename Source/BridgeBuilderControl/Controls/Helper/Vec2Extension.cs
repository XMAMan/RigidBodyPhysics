using GraphicMinimal;
using System.Drawing;
using System.Linq;

namespace BridgeBuilderControl.Controls.Helper
{
    internal static class Vec2Extension
    {
        internal static Vector2D ToGrx(this PointF p)
        {
            return new Vector2D(p.X, p.Y);
        }

        internal static GraphicMinimal.Vector2D ToGrx(this RigidBodyPhysics.MathHelper.Vec2D v)
        {
            return new GraphicMinimal.Vector2D(v.X, v.Y);
        }

        internal static RigidBodyPhysics.MathHelper.Vec2D ToPhx(this GraphicMinimal.Vector2D v)
        {
            return new RigidBodyPhysics.MathHelper.Vec2D(v.X, v.Y);
        }

        internal static RigidBodyPhysics.MathHelper.Vec2D[] ToPhx(this GraphicMinimal.Vector2D[] vArray)
        {
            return vArray.Select(v => new RigidBodyPhysics.MathHelper.Vec2D(v.X, v.Y)).ToArray();
        }

        internal static Color ToColor(this Vector3D color)
        {
            color.X = Clamp(color.X, 0, 1);
            color.Y = Clamp(color.Y, 0, 1);
            color.Z = Clamp(color.Z, 0, 1);
            return Color.FromArgb((byte)(color.X * 255), (byte)(color.Y * 255), (byte)(color.Z * 255));
        }

        private static float Clamp(float f, float min, float max)
        {
            if (f < min) f = min;
            if (f > max) f = max;
            return f;
        }
    }
}
