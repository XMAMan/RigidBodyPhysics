using BridgeBuilderControl.Controls.BridgeEditor.Model;
using PhysicGlobal;
using RigidBodyPhysics.MathHelper;
using System;
using System.Drawing;

namespace BridgeBuilderControl.Controls.Helper
{
    internal static class MathHelper
    {
        public static float GetDistance(Point point1, Point point2)
        {
            var pix1 = new Vec2D(point1.X, point1.Y);
            var pix2 = new Vec2D(point2.X, point2.Y);

            return (pix1 -  pix2).Length();
        }

        public static bool LineIntersectsPolygon(Vec2D p1, Vec2D p2, Vec2D[] polygon)
        {
            for (int i=0; i<polygon.Length; i++)
            {
                var p3 = polygon[i];
                var p4 = polygon[(i+1) % polygon.Length];
                if (PolygonHelper.IntersectLines(p1, p2, p3, p4))
                    return true;
            }

            return false;   
        }

        public static bool IsPointAboveBar(Vec2D pixelPoint, Bar bar, float gridSize, float cameraZoomFactor)
        {
            var pix1 = GridToPixelPoint(bar.P1, gridSize);
            var pix2 = GridToPixelPoint(bar.P2, gridSize);

            return IsPointAboveLine(pix1, pix2, pixelPoint, 10 * cameraZoomFactor);
        }

        public static Vec2D GridToPixelPoint(Point point, float gridSize)
        {
            return new Vec2D(point.X * gridSize, point.Y * gridSize);
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
    }
}
