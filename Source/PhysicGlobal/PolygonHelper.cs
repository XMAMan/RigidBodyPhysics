namespace PhysicGlobal
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

        //Ist das Polygon gegen die Uhr definiert?
        public static bool IsPolygonCounterClockWise(Vec2D[] polygon)
        {
            return GetSignedAreaFromPolygon(polygon) > 0;
        }

        public static Vec2D[] OrderPointsCounterClockWise(Vec2D[] polygon)
        {
            if (IsPolygonCounterClockWise(polygon) == false)
            {
                var list = polygon.ToList();
                list.Reverse();
                return list.ToArray();
            }

            return polygon;
        }

        public static Vec2D[] OrderPointsClockWise(Vec2D[] polygon)
        {
            if (IsPolygonCounterClockWise(polygon) == true)
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

        //Ein Polygon, was weitere Polygone enthalten kann, wo es aber zwischen den Kanten keine Schnittpunkte gibt
        public class NestedPolygon<T>
        {
            public Vec2D[] Points { get; set; }
            public int ZOrder { get; set; } = 0; //Z-Order für die Sortierung
            public bool IsOutside { get; set; } = true; //Zeigen die Normalen nach Außen?
            public T Polygon { get; set; }

            public NestedPolygon(Vec2D[] points, T polygon)
            {
                Points = points;
                Polygon = polygon;
            }
        }

        //Wenn man mehrere verschachtelte Polygone hat, wo das innerste Polygon ganz vorne ist und das äußerste ganz hinten, 
        //dann ermittelt diese Funktion für alle Polygone die ZOrder und ob die Normale nach außen oder innen zeigt.
        public static void UpdateIsOutsideAndUVFromAllPolygons<T>(NestedPolygon<T>[] polygons, Action<T, int, bool> setZOrderAndIsOutside)
        {
            if (polygons.Length == 0) return;

            //Schritt 1: Finde den Y-Bereich herraus und setze alle ZOrder auf 0
            foreach (var polygon in polygons)
            {
                polygon.ZOrder = 0;
            }

            //Schritt 2: Setze die ZOrder-Werte
            float minY = polygons.Min(x => x.Points.Min(p => p.Y));
            float maxY = polygons.Max(x => x.Points.Max(p => p.Y));
            
            for (int y = (int)minY; y <= (int)maxY; y++)
            {
                NestedPolygon<T>[] polys = GetOrderedScanlineList<T>(polygons, y);
                if (polys == null) continue;
                int currentZ = 0;
                foreach (var poly in polys)
                {
                    if (poly.ZOrder == 0)
                    {
                        currentZ++;
                        poly.ZOrder = currentZ;
                    }
                    else
                    {
                        if (currentZ != poly.ZOrder)
                            currentZ = poly.ZOrder;
                        else
                            currentZ--;
                    }
                }
            }

            //Schritt 3: Sortiere nach ZOrder
            polygons = polygons.OrderBy(x => x.ZOrder).ToArray();

            //Schritt 4: Lege IsOutside für die Polygone fest
            foreach (var poly in polygons)
            {
                if (poly.ZOrder == 0) throw new Exception("Error: ZOrder can not be detected");
                if (poly.ZOrder % 2 == 0)
                    poly.IsOutside = false;
                else
                {
                    poly.IsOutside = true;
                }

                poly.ZOrder -= 100; //Damit ein Hintergrund-Polygon kein anders Objekt verdeckt (Ihre Z-Order-Werte sind alle größer 0)
            }

            //Schritt 5: Übertrage die ZOrder und IsOutside-Werte in die Objekte
            foreach (var poly in polygons)
            {
                setZOrderAndIsOutside(poly.Polygon, poly.ZOrder, poly.IsOutside);
            }
        }

        //Rückgabe alle Schnittpunkte von allen Polygonen für eine Scanline. Sie werden dabei nach X sortiert.
        private static NestedPolygon<T>[] GetOrderedScanlineList<T>(NestedPolygon<T>[] polygons, int yScan)
        {
            List<NestedPolygon<T>> ret = new List<NestedPolygon<T>>();
            List<int> xValues = new List<int>();

            //Schritt 1: Schnittpunkte von allen Polygonen einsammeln
            for (int i = 0; i < polygons.Length; i++)
            {
                NestedPolygon<T> poly = polygons[i];
                List<int> points = PolygonHelper.PolygonScanlineIntersectionTest(poly.Points, yScan);
                if (points == null) return null;
                foreach (int p in points)
                {
                    ret.Add(poly);
                    xValues.Add(p);
                }
            }

            //Schritt 2: Schnittpunkte sortieren
            for (int i = 0; i < ret.Count; i++)
                for (int j = i; j < ret.Count; j++)
                {
                    if (xValues[i] > xValues[j])
                    {
                        NestedPolygon<T> tmp1 = ret[i];
                        ret[i] = ret[j];
                        ret[j] = tmp1;
                        int tmp2 = xValues[i];
                        xValues[i] = xValues[j];
                        xValues[j] = tmp2;
                    }
                }

            return ret.ToArray();
        }

        //Gibt alle Schnittpunkte zwischen einer Scanline(Horizontale Linie) und dem Polygon zurück
        //yScan = Y-Wert der Scanline(x geht von -unendlich bis +unendlich
        //Liefert alle X-Wert der Schnittpunkte zurück. Y-Wert von allen Punkten ist yScan. Es liefert null, wenn Scanline durch ein Eckpunkt geht.
        public static List<int> PolygonScanlineIntersectionTest(Vec2D[] polygon, float yScan)
        {
            List<int> ret = new List<int>();
            for (int i = 0; i < polygon.Length; i++)
            {
                Vec2D p1 = polygon[i];
                Vec2D p2 = polygon[(i + 1) % polygon.Length];
                if (!(yScan >= Math.Min(p1.Y, p2.Y) && yScan <= Math.Max(p1.Y, p2.Y))) continue;
                if (p1.Y == p2.Y)
                {
                    ret.Add((int)p1.X);
                    ret.Add((int)p2.X);
                    continue;
                }
                if (p1.X == p2.X) //Linie ist Senkrecht
                {
                    if (yScan >= Math.Min(p1.Y, p2.Y) && yScan <= Math.Max(p1.Y, p2.Y)) ret.Add((int)p1.X);
                    continue;
                }

                float a = (p2.Y - p1.Y) / (float)(p2.X - p1.X); //y = a*x + b
                float b = p1.Y - p1.X * a;                      // a*x + b == yScan     -> x == (yScan - b) / a
                int x = (int)((yScan - b) / a + 0.5f);          //X-Koordinate des Schnittpunktes
                if (x >= Math.Min(p1.X, p2.X) && x <= Math.Max(p1.X, p2.X)) ret.Add(x);
            }

            ret = ret.Distinct().ToList(); //Entferne doppelte Einträge

            if (ret.Count % 2 == 1) return null;
            return ret;
        }

        public static bool LineIntersectsPolygon(Vec2D p1, Vec2D p2, Vec2D[] polygon)
        {
            for (int i = 0; i < polygon.Length; i++)
            {
                var p3 = polygon[i];
                var p4 = polygon[(i + 1) % polygon.Length];
                if (MathHelper.IntersectLines(p1, p2, p3, p4))
                    return true;
            }

            return false;
        }
    }
}
