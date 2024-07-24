using BridgeBuilderControl.Controls.BridgeEditor.Model;
using GraphicMinimal;
using RigidBodyPhysics.MathHelper;
using System;
using System.Drawing;

namespace BridgeBuilderControl.Controls.Helper
{
    internal static class MathHelper
    {
        public static float GetDistance(Point point1, Point point2)
        {
            var pix1 = new Vector2D(point1.X, point1.Y);
            var pix2 = new Vector2D(point2.X, point2.Y);

            return (pix1 -  pix2).Length();
        }

        public static bool LineIntersectsPolygon(Vector2D p1, Vector2D p2, Vector2D[] polygon)
        {
            for (int i=0; i<polygon.Length; i++)
            {
                var p3 = polygon[i];
                var p4 = polygon[(i+1) % polygon.Length];
                if (PolygonHelper.IntersectLines(p1.ToPhx(), p2.ToPhx(), p3.ToPhx(), p4.ToPhx()))
                    return true;
            }

            return false;   
        }

        public static bool IsPointAboveBar(Vector2D pixelPoint, Bar bar, float gridSize, float cameraZoomFactor)
        {
            var pix1 = GridToPixelPoint(bar.P1, gridSize);
            var pix2 = GridToPixelPoint(bar.P2, gridSize);

            return IsPointAboveLine(pix1, pix2, pixelPoint, 10 * cameraZoomFactor);
        }

        public static Vector2D GridToPixelPoint(Point point, float gridSize)
        {
            return new Vector2D(point.X * gridSize, point.Y * gridSize);
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
    }
}
