namespace PhysicEngine.MathHelper.PolygonDecomposition
{
    internal static class PolyPointHelper
    {
        public static bool IsReflex(Vec2D p1, Vec2D p2, Vec2D p3)
        {
            float tmp = (p3.Y - p1.Y) * (p2.X - p1.X) - (p3.X - p1.X) * (p2.Y - p1.Y);
            return tmp < 0;
        }

        public static bool IsConvex(Vec2D p1, Vec2D p2, Vec2D p3)
        {
            float tmp = (p3.Y - p1.Y) * (p2.X - p1.X) - (p3.X - p1.X) * (p2.Y - p1.Y);
            return tmp > 0;
        }

        public static bool InCone(Vec2D p1, Vec2D p2, Vec2D p3, Vec2D p)
        {
            bool convex = IsConvex(p1, p2, p3);

            if (convex)
            {
                if (!IsConvex(p1, p2, p))
                    return false;

                if (!IsConvex(p2, p3, p))
                    return false;

                return true;
            }
            else
            {
                if (IsConvex(p1, p2, p))
                    return true;

                if (IsConvex(p2, p3, p))
                    return true;

                return false;
            }
        }

        public static bool Intersects(Vec2D p11, Vec2D p12, Vec2D p21, Vec2D p22)
        {
            Vec2D min1 = new Vec2D(Math.Min(p11.X, p12.X), Math.Min(p11.Y, p12.Y));
            Vec2D max1 = new Vec2D(Math.Max(p11.X, p12.X), Math.Max(p11.Y, p12.Y));

            Vec2D min2 = new Vec2D(Math.Min(p21.X, p22.X), Math.Min(p21.Y, p22.Y));
            Vec2D max2 = new Vec2D(Math.Max(p21.X, p22.X), Math.Max(p21.Y, p22.Y));

            bool boxIntersects = max1.X > min2.X && min1.X < max2.X && max1.Y > min2.Y && min1.Y < max2.Y;
            if (boxIntersects == false) return false;

            if ((p11.X == p21.X) && (p11.Y == p21.Y))
                return false;

            if ((p11.X == p22.X) && (p11.Y == p22.Y))
                return false;

            if ((p12.X == p21.X) && (p12.Y == p21.Y))
                return false;

            if ((p12.X == p22.X) && (p12.Y == p22.Y))
                return false;

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
    }
}
