using PhysicGlobal;
using System.Drawing;

namespace WpfControls.Extensions
{
    public static class Vec2DExtension
    {
        public static Vec2D ToPhx(this Point p)
        {
            return new Vec2D(p.X, p.Y);
        }

        public static Color ToColor(this Vec3D color)
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

        public static Point ToPoint(this Vec2D v)
        {
            return new Point((int)v.X, (int)v.Y);
        }
    }
}
