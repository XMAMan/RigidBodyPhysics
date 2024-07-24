using PhysicEngine.MathHelper.PolygonDecomposition;

namespace PhysicEngine.MathHelper
{
    public static class PolygonHelper
    {
        //https://www.eecs.umich.edu/courses/eecs380/HANDOUTS/PROJ2/InsidePoly.html -> Solution 1 -> Funktioniert
        public static bool PointIsInsidePolygon1(Vec2D[] polygon, Vec2D p)
        {
            int counter = 0;
            int i;
            double xinters;
            Vec2D p1, p2;

            p1 = polygon[0];
            for (i = 1; i <= polygon.Length; i++)
            {
                p2 = polygon[i % polygon.Length];
                if (p.Y > Math.Min(p1.Y, p2.Y))     //Hier steht > und nicht >= um Ecken nicht doppelt zu zählen
                {
                    if (p.Y <= Math.Max(p1.Y, p2.Y))
                    {
                        if (p.X <= Math.Max(p1.X, p2.X)) //Wenn ich rechts neben der Linie starte, kann es kein Schnittpunkt geben
                        {
                            if (p1.Y != p2.Y)//Suche kein Schnittpunkt zwischen zwei horizontalen Linien
                            {
                                xinters = (p.Y - p1.Y) * (p2.X - p1.X) / (p2.Y - p1.Y) + p1.X; //X-Koordinante vom Schnittpunkt
                                if (p1.X == p2.X || p.X <= xinters)
                                    counter++;
                            }
                        }
                    }
                }
                p1 = p2;
            }

            if (counter % 2 == 0)
                return false;
            else
                return true;
        }

        //Variante 2 von Solution 1 von Randolph Franklin -> Funktioniert auch, ist aber kürzer.
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

        //Quelle: https://de.wikipedia.org/wiki/Gau%C3%9Fsche_Trapezformel
        public static float GetAreaFromPolygon(Vec2D[] polygon)
        {
            float area = 0;
            for (int i = 0; i < polygon.Length; i++)
            {
                var p1 = polygon[i];
                var p2 = polygon[(i + 1) % polygon.Length];

                //area += polygon[i].X * (polygon[(i + 1) % polygon.Length].Y - polygon[(i - 1 + polygon.Length) % polygon.Length].Y); //Quelle: https://en.wikipedia.org/wiki/Shoelace_formula#Other_formulas
                area += (p1.Y + p2.Y) * (p1.X - p2.X);//Quelle: https://de.wikipedia.org/wiki/Gau%C3%9Fsche_Trapezformel
            }
            return Math.Abs(area * 0.5f);
        }

        //Das ist eine Abwandlung von hier: https://github.com/erincatto/box2d/blob/main/src/collision/b2_polygon_shape.cpp#L274
        //Hier geht es darum zu zeigen, dass man auch mit dem Cross-Produkt die Fläche berechnen kann
        public static float GetSignedAreaFromPolygon(Vec2D[] polygon)
        {
            float area = 0;
            for (int i = 0; i < polygon.Length; i++)
            {
                var p1 = polygon[i];
                var p2 = polygon[(i + 1) % polygon.Length];

                area += Vec2D.ZValueFromCross(p1, p2); //Area from Triangle p1-p2-[0;0] = 1/2*|Cross(p1,p2)|
            }
            return area * 0.5f;
        }

        //Ist das Polygon im Uhrzeigersinn definiert?
        public static bool IsCCW(Vec2D[] polygon)
        {
            return GetSignedAreaFromPolygon(polygon) > 0;
        }

        public static Vec2D[] OrderPointsCCW(Vec2D[] polygon)
        {
            if (IsCCW(polygon) == false)
            {
                var list = polygon.ToList();
                list.Reverse();
                return list.ToArray();
            }

            return polygon;
        }


        //Quelle1: https://en.wikipedia.org/wiki/Centroid#Of_a_polygon
        //Quelle2: https://demonstrations.wolfram.com/CenterOfMassOfAPolygon/
        public static Vec2D GetCenterOfMassFromPolygon(Vec2D[] polygon)
        {
            Vec2D pos = new Vec2D(0, 0);
            float area = 0;
            for (int i = 0; i < polygon.Length; i++)
            {
                var p1 = polygon[i];
                var p2 = polygon[(i + 1) % polygon.Length];

                float s = p1.X * p2.Y - p2.X * p1.Y; //s=Vec2D.ZValueFromCross(p1, p2);
                pos += (p1 + p2) * s;//Center from Triangle p1-p2-[0;0] = 1/3*(p1 + p2 + new Vec2D(0,0))

                area += s;
            }

            float f = 1 / (3 * area);

            return pos * f;
        }

        public static BoundingBox GetBoundingBoxFromPolygon(Vec2D[] polygon)
        {
            Vec2D min = new Vec2D(float.MaxValue, float.MaxValue);
            Vec2D max = new Vec2D(float.MinValue, float.MinValue);
            foreach (var p in polygon)
            {
                if (p.X < min.X) min.X = p.X;
                if (p.Y < min.Y) min.Y = p.Y;
                if (p.X > max.X) max.X = p.X;
                if (p.Y > max.Y) max.Y = p.Y;
            }
            return new BoundingBox(min, max);
        }


        //Quelle: https://github.com/erincatto/box2d/blob/main/src/collision/b2_polygon_shape.cpp#L274c -> Funktioniert
        public static float GetInertiaFromPolygon(float density, Vec2D[] polygon)
        {
            Vec2D center = new Vec2D(0, 0);
            float area = 0;
            float I = 0;
            Vec2D s = polygon[0]; // Get a reference point for forming triangles. Use the first vertex to reduce round-off errors.
            float k_inv3 = 1f / 3f;

            for (int i=0;i<polygon.Length;i++)
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

        //Gibt es ein Schnittpunkt zwischen zwei Linien?
        public static bool IntersectLines(Vec2D p11, Vec2D p12, Vec2D p21, Vec2D p22)
        {
            return PolyPointHelper.Intersects(p11, p12, p21, p22);
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
