using RigidBodyPhysics.MathHelper;
using System;

namespace TextureEditorControl.Controls.Editor.Model
{
    static class MathHelper
    {
        //Die Linie geht von p1 nach p2
        public static bool IsPointAboveLine(Vec2D p1, Vec2D p2, Vec2D point, float lineWidth)
        {
            Vec2D dir = (p2 - p1);
            float dirLength = dir.Length();
            if (dirLength < 0.0001f) return false;
            dir /= dirLength;
            Vec2D d = point - p1;

            float projection1 = dir * d;
            if (projection1 < 0) return false;
            if (projection1 > (p2 - p1).Length()) return false;

            float projection2 = dir.Spin90() * d;
            if (Math.Abs(projection2) > lineWidth) return false;

            return true;
        }

        public static float GetNormalDistanceToLine(Vec2D p1, Vec2D p2, Vec2D point)
        {
            Vec2D dir = (p2 - p1).Normalize();
            Vec2D normal = dir.Spin90();
            return normal * (point - p1);
        }

        public static bool IsPointInRectangle(Vec2D[] points, Vec2D point)
        {
            for (int i = 0; i < points.Length; i++)
            {
                Vec2D edge = (points[(i + 1) % points.Length] - points[i]).Normalize();
                bool isInside = edge * (point - points[i]) > 0;
                if (isInside == false) return false;
            }

            return true;
        }

        public static bool PointIsInsidePolygon(Vec2D[] polygon, Vec2D p)
        {
            int i, j;
            bool c = false;
            for (i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                if ((polygon[i].Y <= p.Y && p.Y < polygon[j].Y ||
                     polygon[j].Y <= p.Y && p.Y < polygon[i].Y) &&
                    p.X < (polygon[j].X - polygon[i].X) * (p.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X)
                    c = !c;
            }
            return c;
        }
    }
}
