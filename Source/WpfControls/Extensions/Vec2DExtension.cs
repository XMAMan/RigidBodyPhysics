using GraphicMinimal;
using PhysicGlobal;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace WpfControls.Extensions
{
    public static class Vec2DExtension
    {
        public static GraphicMinimal.Vector2D ToGrx(this Point v)
        {
            return new GraphicMinimal.Vector2D(v.X, v.Y);
        }

        public static Vector2D ToGrx(this PointF p)
        {
            return new Vector2D(p.X, p.Y);
        }

        public static GraphicMinimal.Vector2D ToGrx(this Vec2D v)
        {
            return new GraphicMinimal.Vector2D(v.X, v.Y);
        }

        public static IEnumerable<GraphicMinimal.Vector2D> ToGrx(this IEnumerable<Vec2D> v)
        {
            return v.Select(x => new GraphicMinimal.Vector2D(x.X, x.Y)).ToList();
        }

        public static Vec2D ToPhx(this GraphicMinimal.Vector2D v)
        {
            return new Vec2D(v.X, v.Y);
        }

        public static Vec2D ToPhx(this PointF p)
        {
            return new Vec2D(p.X, p.Y);
        }

        public static Vec2D[] ToPhx(this GraphicMinimal.Vector2D[] vArray)
        {
            return vArray.Select(v => new Vec2D(v.X, v.Y)).ToArray();
        }

        public static Color ToColor(this Vector3D color)
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

        public static PointF ToPointF(this Vec2D v)
        {
            return new PointF(v.X, v.Y);
        }

        public static Point ToPoint(this GraphicMinimal.Vector2D v)
        {
            return new Point(v.Xi, v.Yi);
        }

        public static RectangleF ToRectangleF(this PhysicGlobal.BoundingBox box)
        {
            return new RectangleF(box.Min.X, box.Min.Y, box.Width, box.Height);
        }
    }
}
