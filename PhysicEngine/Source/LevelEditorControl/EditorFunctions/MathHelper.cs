using GraphicMinimal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace LevelEditorControl.EditorFunctions
{
    internal static class MathHelper
    {
        public static Vector2D Center(this RectangleF rec)
        {
            return new Vector2D(rec.X + rec.Width / 2, rec.Y + rec.Height / 2);
        }

        public static RectangleF GetBoundingBox(IEnumerable<RectangleF> boxes)
        {
            Vector2D min = new Vector2D(float.MaxValue, float.MaxValue);
            Vector2D max = new Vector2D(float.MinValue, float.MinValue);

            foreach (var box in boxes)
            {
                min.X = Math.Min(min.X, box.X);
                min.Y = Math.Min(min.Y, box.Y);

                max.X = Math.Max(max.X, box.X + box.Width);
                max.Y = Math.Max(max.Y, box.Y + box.Height);
            }

            return new RectangleF(min.X, min.Y, max.X - min.X, max.Y - min.Y);
        }

        //Gibt es ein Schnittpunkt zwischen zwei Linien?
        public static bool IntersectLines(Vector2D p11, Vector2D p12, Vector2D p21, Vector2D p22)
        {
            if ((p11.X == p21.X) && (p11.Y == p21.Y))
                return false;

            if ((p11.X == p22.X) && (p11.Y == p22.Y))
                return false;

            if ((p12.X == p21.X) && (p12.Y == p21.Y))
                return false;

            if ((p12.X == p22.X) && (p12.Y == p22.Y))
                return false;

            Vector2D min1 = new Vector2D(Math.Min(p11.X, p12.X), Math.Min(p11.Y, p12.Y));
            Vector2D max1 = new Vector2D(Math.Max(p11.X, p12.X), Math.Max(p11.Y, p12.Y));

            Vector2D min2 = new Vector2D(Math.Min(p21.X, p22.X), Math.Min(p21.Y, p22.Y));
            Vector2D max2 = new Vector2D(Math.Max(p21.X, p22.X), Math.Max(p21.Y, p22.Y));

            bool boxIntersects = max1.X > min2.X && min1.X < max2.X && max1.Y > min2.Y && min1.Y < max2.Y;
            if (boxIntersects == false) return false;

            Vector2D v1ort = new Vector2D(p12.Y - p11.Y, p11.X - p12.X);
            Vector2D v2ort = new Vector2D(p22.Y - p21.Y, p21.X - p22.X);

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

        public static Vector2D GetProjectedPointOnLine(Vector2D p1, Vector2D p2, Vector2D point, out float distance, out float distancePercent)
        {
            Vector2D dir = (p2 - p1);
            float dirLength = dir.Length();
            if (dirLength < 0.0001f)
            {
                distance = float.NaN;
                distancePercent = float.NaN;
                return null;
            }
            dir /= dirLength;
            Vector2D d = point - p1;

            float projection = dir * d;

            distance = projection;
            distancePercent = distance / dirLength;

            if (projection < 0)
            {
                return null;
            }

            return p1 + dir * projection;
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

        public static RectangleF GetBoundingBoxFromPolygon(Vector2D[] polygon)
        {
            Vector2D min = new Vector2D(float.MaxValue, float.MaxValue);
            Vector2D max = new Vector2D(float.MinValue, float.MinValue);
            foreach (var p in polygon)
            {
                if (p.X < min.X) min.X = p.X;
                if (p.Y < min.Y) min.Y = p.Y;
                if (p.X > max.X) max.X = p.X;
                if (p.Y > max.Y) max.Y = p.Y;
            }
            return new RectangleF(min.X, min.Y, max.X - min.X, max.Y - min.Y);
        }

        //Quelle: https://de.wikipedia.org/wiki/Gau%C3%9Fsche_Trapezformel
        public static float GetAreaFromPolygon(Vector2D[] polygon)
        {
            float area = 0;
            for (int i = 0; i < polygon.Length; i++)
            {
                var p1 = polygon[i];
                var p2 = polygon[(i + 1) % polygon.Length];

                area += (p1.Y + p2.Y) * (p1.X - p2.X);//Quelle: https://de.wikipedia.org/wiki/Gau%C3%9Fsche_Trapezformel
            }
            return Math.Abs(area * 0.5f);
        }

        public static bool IsPolygonCCW(Vector2D[] polygon)
        {
            float area = 0;
            for (int i = 0; i < polygon.Length; i++)
            {
                var p1 = polygon[i];
                var p2 = polygon[(i + 1) % polygon.Length];

                area += Vector2D.ZValueFromCross(p1, p2); //Area from Triangle p1-p2-[0;0] = 1/2*|Cross(p1,p2)|
            }
            return area < 0;
        }

        public static Vector2D[] OrderPointsCCW(Vector2D[] polygon)
        {
            if (IsPolygonCCW(polygon) == false)
            {
                var list = polygon.ToList();
                list.Reverse();
                return list.ToArray();
            }

            return polygon;
        }


        //Gibt alle Schnittpunkte zwischen einer Scanline(Horizontale Linie) und dem Polygon zurück
        //yScan = Y-Wert der Scanline(x geht von -unendlich bis +unendlich
        //Liefert alle X-Wert der Schnittpunkte zurück. Y-Wert von allen Punkten ist yScan. Es liefert null, wenn Scanline durch ein Eckpunkt geht.
        public static List<int> PolygonScanlineIntersectionTest(Vector2D[] polygon, float yScan)
        {
            List<int> ret = new List<int>();
            for (int i = 0; i < polygon.Length; i++)
            {
                Vector2D p1 = polygon[i];
                Vector2D p2 = polygon[(i + 1) % polygon.Length];
                if (!(yScan >= Math.Min(p1.Y, p2.Y) && yScan <= Math.Max(p1.Y, p2.Y))) continue;
                if (p1.Y == p2.Y)
                {
                    ret.Add(p1.Xi);
                    ret.Add(p2.Xi);
                    continue;
                }
                if (p1.X == p2.X) //Linie ist Senkrecht
                {
                    if (yScan >= Math.Min(p1.Y, p2.Y) && yScan <= Math.Max(p1.Y, p2.Y)) ret.Add(p1.Xi);
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
    }
}
