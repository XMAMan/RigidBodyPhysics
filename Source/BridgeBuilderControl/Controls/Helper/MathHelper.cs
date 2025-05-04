using BridgeBuilderControl.Controls.BridgeEditor.Model;
using PhysicGlobal;
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

        public static bool IsPointAboveBar(Vec2D pixelPoint, Bar bar, float gridSize, float cameraZoomFactor)
        {
            var pix1 = GridToPixelPoint(bar.P1, gridSize);
            var pix2 = GridToPixelPoint(bar.P2, gridSize);

            return PhysicGlobal.MathHelper.IsPointAboveLine(pix1, pix2, pixelPoint, 10 * cameraZoomFactor);
        }

        public static Vec2D GridToPixelPoint(Point point, float gridSize)
        {
            return new Vec2D(point.X * gridSize, point.Y * gridSize);
        }
    }
}
