using PhysicGlobal;

namespace RigidBodyPhysics.MathHelper.PolygonDecomposition
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
    }
}
