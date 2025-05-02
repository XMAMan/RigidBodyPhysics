using PhysicGlobal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace LevelEditorControl.EditorFunctions
{
    internal static class MathHelper
    {
        public static Vec2D Center(this RectangleF rec)
        {
            return new Vec2D(rec.X + rec.Width / 2, rec.Y + rec.Height / 2);
        }

        public static RectangleF GetBoundingBox(IEnumerable<RectangleF> boxes)
        {
            Vec2D min = new Vec2D(float.MaxValue, float.MaxValue);
            Vec2D max = new Vec2D(float.MinValue, float.MinValue);

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

        public static RectangleF GetBoundingBoxFromPolygon(Vec2D[] polygon)
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
            return new RectangleF(min.X, min.Y, max.X - min.X, max.Y - min.Y);
        }

        //Quelle: https://de.wikipedia.org/wiki/Gau%C3%9Fsche_Trapezformel
        public static float GetAreaFromPolygon(Vec2D[] polygon)
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

        public static bool IsPolygonCCW(Vec2D[] polygon)
        {
            float area = 0;
            for (int i = 0; i < polygon.Length; i++)
            {
                var p1 = polygon[i];
                var p2 = polygon[(i + 1) % polygon.Length];

                area += Vec2D.ZValueFromCross(p1, p2); //Area from Triangle p1-p2-[0;0] = 1/2*|Cross(p1,p2)|
            }
            return area < 0;
        }

        public static Vec2D[] OrderPointsCCW(Vec2D[] polygon)
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
    }
}
