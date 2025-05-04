using PhysicGlobal;
using RigidBodyPhysics.MathHelper.PolygonDecomposition;

namespace RigidBodyPhysics.MathHelper
{
    public static class PolygonHelper
    {
        //Quelle: https://github.com/erincatto/box2d/blob/main/src/collision/b2_polygon_shape.cpp#L274c -> Funktioniert
        public static float GetInertiaFromPolygon(float density, Vec2D[] polygon)
        {
            Vec2D center = new Vec2D(0, 0);
            float area = 0;
            float I = 0;
            Vec2D s = polygon[0]; // Get a reference point for forming triangles. Use the first vertex to reduce round-off errors.
            float k_inv3 = 1f / 3f;

            for (int i = 0; i < polygon.Length; i++)
            {
                // Triangle vertices.
                Vec2D e1 = polygon[i] - s;
                Vec2D e2 = i + 1 < polygon.Length ? polygon[i + 1] - s : polygon[0] - s;

                float D = Vec2D.ZValueFromCross(e1, e2);

                float triangleArea = 0.5f * D;
                area += triangleArea;

                // Area weighted centroid
                center += triangleArea * k_inv3 * (e1 + e2);

                float ex1 = e1.X, ey1 = e1.Y;
                float ex2 = e2.X, ey2 = e2.Y;

                float intx2 = ex1 * ex1 + ex2 * ex1 + ex2 * ex2;
                float inty2 = ey1 * ey1 + ey2 * ey1 + ey2 * ey2;

                I += (0.25f * k_inv3 * D) * (intx2 + inty2);
            }

            // Total mass
            float mass = density * area;

            // Center of mass (Shows from point s to local CenterOfMass; GlobalCenterOfMass=center+s)
            center *= 1.0f / area;

            // Inertia tensor relative to the local origin (point s).
            I *= density;

            // Shift to center of mass then to original body origin by using the parallel axis theorem
            I -= mass * (center * center);

            return Math.Abs(I); //I=Positiv -> Polygon is CW; Negativ -> Polygon is CCW
        }

        public static ConvexPolygon[] ConvertConcavePolygonToConvexes(Vec2D[] polygon)
        {
            bool isCCW = new Poly(polygon).IsCCW();
            if (isCCW == false)
            {
                polygon = polygon.Reverse().ToArray();
            }

            var indexPolys = PolygonDecomposer.DecomposePolygon(new Poly(polygon));

            if (isCCW == false)
            {
                polygon = polygon.Reverse().ToArray();
                foreach (var poly in indexPolys)
                {
                    poly.Indizes = poly.Indizes.Select(x => polygon.Length - x - 1).ToArray();
                }
            }

            var polys = indexPolys.Select(x => new ConvexPolygon(polygon, x.Indizes)).ToArray();
            return polys;
        }

        //Den Schnittpunkt erhalte ich durch p1 + direcdtion1 * t1      oder p2 + direction2 * t2
        public static void IntersectionTwoRays(Vec2D p1, Vec2D direction1, Vec2D p2, Vec2D direction2, out float t1, out float t2)
        {
            Vec2D V = direction1;
            Vec2D L = direction2;
            Vec2D C = p2 - p1;

            t2 = (V.Y * C.X / V.X - C.Y) / (L.Y - L.X * V.Y / V.X);
            if (float.IsNaN(t2) || float.IsInfinity(t2))
            {
                t2 = (C.Y * V.X / V.Y - C.X) / (L.X + L.Y * V.X / V.Y);
            }

            t1 = (C.X * L.Y / L.X - C.Y) / (V.X * L.Y / L.X - V.Y);
            if (float.IsNaN(t1) || float.IsInfinity(t1))
            {
                t1 = (C.Y * L.X / L.Y - C.X) / (V.Y * L.X / L.Y - V.X);
            }
        }
    }
}
