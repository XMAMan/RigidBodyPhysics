using GraphicMinimal;
using System;

namespace TextureEditorControl.Controls.Editor.Model
{
    static class MathHelper
    {
        //Die Linie geht von p1 nach p2
        public static bool IsPointAboveLine(Vector2D p1, Vector2D p2, Vector2D point, float lineWidth)
        {
            Vector2D dir = (p2 - p1);
            float dirLength = dir.Length();
            if (dirLength < 0.0001f) return false;
            dir /= dirLength;
            Vector2D d = point - p1;

            float projection1 = dir * d;
            if (projection1 < 0) return false;
            if (projection1 > (p2 - p1).Length()) return false;

            float projection2 = dir.Spin90() * d;
            if (Math.Abs(projection2) > lineWidth) return false;

            return true;
        }

        public static float GetNormalDistanceToLine(Vector2D p1, Vector2D p2, Vector2D point)
        {
            Vector2D dir = (p2 - p1).Normalize();
            Vector2D normal = dir.Spin90();
            return normal * (point - p1);
        }

        public static bool IsPointInRectangle(Vector2D[] points, Vector2D point)
        {
            for (int i = 0; i < points.Length; i++)
            {
                Vector2D edge = (points[(i + 1) % points.Length] - points[i]).Normalize();
                bool isInside = edge * (point - points[i]) > 0;
                if (isInside == false) return false;
            }

            return true;
        }

        public static bool PointIsInsidePolygon(Vector2D[] polygon, Vector2D p)
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
