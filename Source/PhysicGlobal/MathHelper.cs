namespace PhysicGlobal
{
    public static class MathHelper
    {
        //Gibt es ein Schnittpunkt zwischen zwei Linien?
        public static bool IntersectLines(Vec2D p11, Vec2D p12, Vec2D p21, Vec2D p22)
        {
            if ((p11.X == p21.X) && (p11.Y == p21.Y))
                return false;

            if ((p11.X == p22.X) && (p11.Y == p22.Y))
                return false;

            if ((p12.X == p21.X) && (p12.Y == p21.Y))
                return false;

            if ((p12.X == p22.X) && (p12.Y == p22.Y))
                return false;

            Vec2D min1 = new Vec2D(Math.Min(p11.X, p12.X), Math.Min(p11.Y, p12.Y));
            Vec2D max1 = new Vec2D(Math.Max(p11.X, p12.X), Math.Max(p11.Y, p12.Y));

            Vec2D min2 = new Vec2D(Math.Min(p21.X, p22.X), Math.Min(p21.Y, p22.Y));
            Vec2D max2 = new Vec2D(Math.Max(p21.X, p22.X), Math.Max(p21.Y, p22.Y));

            bool boxIntersects = max1.X > min2.X && min1.X < max2.X && max1.Y > min2.Y && min1.Y < max2.Y;
            if (boxIntersects == false) return false;

            Vec2D v1ort = new Vec2D(p12.Y - p11.Y, p11.X - p12.X);
            Vec2D v2ort = new Vec2D(p22.Y - p21.Y, p21.X - p22.X);

            float dot21 = (p21 - p11) * v1ort;
            float dot22 = (p22 - p11) * v1ort;
            float dot11 = (p11 - p21) * v2ort;
            float dot12 = (p12 - p21) * v2ort;

            if (dot11 * dot12 > 0)
                return false;

            if (dot21 * dot22 > 0)
                return false;

            return true;
        }

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

        public static Vec2D GetProjectedPointOnLine(Vec2D p1, Vec2D p2, Vec2D point, out float distance, out float distancePercent)
        {
            Vec2D dir = (p2 - p1);
            float dirLength = dir.Length();
            if (dirLength < 0.0001f)
            {
                distance = float.NaN;
                distancePercent = float.NaN;
                return null;
            }
            dir /= dirLength;
            Vec2D d = point - p1;

            float projection = dir * d;

            distance = projection;
            distancePercent = distance / dirLength;

            if (projection < 0)
            {
                return null;
            }

            return p1 + dir * projection;
        }

        public static float GetNormalDistanceToLine(Vec2D p1, Vec2D p2, Vec2D point)
        {
            Vec2D dir = (p2 - p1).Normalize();
            Vec2D normal = dir.Spin90();
            return normal * (point - p1);
        }

        //points = Eckpunkte eines Rechtecks, was gedreht sein kann
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
    }
}
