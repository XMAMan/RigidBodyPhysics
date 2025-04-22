using GraphicMinimal;
using GraphicPanels;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BridgeBuilderControl.Controls.Helper
{
    internal static class DrawingHelper
    {
        public static void DrawBackgroundOrClipboardImage(GraphicPanel2D panel, uint xCount, uint yCount, uint waterHeight, uint groundHeight)
        {
            if (panel.IsNamedBitmapTextureAvailable("BackgroundEditorImage"))
            {
                panel.ClearScreen(Color.White);                
                panel.DrawFillRectangle("BackgroundEditorImage", 0, 0, panel.Width, panel.Height, false, Color.White);
                SetTransformationMatrix(panel, xCount);
            }
            else
            {
                ClearScreen(panel);
                SetTransformationMatrix(panel, xCount);
                DrawBackground(panel, xCount, yCount, waterHeight, groundHeight);
            }
        }

        //Skaliert das Level so, dass dessen Breite genau so breit ist wie das Fenster
        public static void SetTransformationMatrix(GraphicPanel2D panel, uint xCount)
        {
            float sizeFactor = panel.Width / (float)xCount; //So viele Pixel ist ein kleines Kästchen breit
            panel.SetTransformationMatrixToIdentity();
            panel.MultTransformationMatrix(Matrix4x4.Scale(sizeFactor, sizeFactor, 0));
        }

        public static void ClearScreen(GraphicPanel2D panel)
        {
            Color backColor = Color.FromArgb(31, 32, 39);
            panel.ClearScreen(backColor);
        }

        public static void DrawBackground(GraphicPanel2D panel, uint xCount, uint yCount, uint waterHeight, uint groundHeight)
        {
            //Ein kleines Kästchen ist 1*1 groß
            //Ein großes Kästchen ist 8*8 groß

            Color lineColorBright = Color.FromArgb(40 + 30, 42 + 30, 48 + 30);
            Color lineColorDark = Color.FromArgb(6 + 40, 8 + 40, 14 + 40);
            Color lineColorMiddle = Color.FromArgb(23, 24, 31);
            Color lineColorYellow = Color.FromArgb(62, 66, 45);            

            //Vertikallinien von den kleinen Kästchen
            for (uint x = 0; x < xCount; x++)
            {
                float xi = x;
                panel.DrawLine(new Pen(lineColorMiddle, 1), new Vector2D(xi, 0), new Vector2D(xi, yCount));
            }

            //Horizontallinien von den kleinen Kästchen
            for (uint y = 0; y < yCount; y++)
            {
                float yi = y;
                panel.DrawLine(new Pen(lineColorMiddle, 1), new Vector2D(0, yi), new Vector2D(xCount, yi));
            }

            //Vertikallinien von den großen Kästchen
            uint boxBigXCount = xCount / 8;
            for (uint x = 1; x <= boxBigXCount; x++)
            {
                float xi = 8 * x;
                Color color = (x % 4 == 0) ? lineColorDark : lineColorBright;
                panel.DrawLine(new Pen(color, 1), new Vector2D(xi, 0), new Vector2D(xi, yCount));
            }

            //Horizontallinien von den großen Kästchen
            float yHeight = 8;
            uint boxBigYCount = yCount / 8;
            for (uint y = 0; y <= boxBigYCount; y++)
            {
                float yi = yHeight * y;
                Color color = (y % 4 == 0) ? lineColorDark : lineColorBright;
                panel.DrawLine(new Pen(color, 1), new Vector2D(0, yi), new Vector2D(xCount, yi));
            }

            //Wasser
            DrawWater(panel, waterHeight, groundHeight, xCount, yCount);

            //Boden
            float groundY = groundHeight;
            panel.DrawLine(new Pen(lineColorYellow, 1), new Vector2D(0, groundY), new Vector2D(xCount, groundY));
        }

        public static void DrawWater(GraphicPanel2D panel, uint waterHeight, uint groundHeight, uint xCount, uint yCount)
        {
            Color waterColor = Color.FromArgb(150, 13, 20, 85);

            float waterY = groundHeight + waterHeight;
            panel.DrawFillRectangle(waterColor, 0, waterY, xCount, yCount);
        }

        public static void DrawGroundBorder(GraphicPanel2D panel, IEnumerable<Point> points, uint groundHeight, uint xCount)
        {
            if (points == null || points.Any() == false) return;

            foreach (var point in points)
            {
                DrawPolyPoint(panel, Color.Yellow, point, groundHeight);
            }

            var groundPoints = GetGroundPoints(points, groundHeight, xCount);

            var pen = new Pen(Color.Yellow, 2);

            for (int i = 1; i < groundPoints.Length; i++)
            {
                panel.DrawLine(pen, groundPoints[i - 1], groundPoints[i]);
            }
        }

        public static void DrawGround(GraphicPanel2D panel, IEnumerable<Point> points, uint groundHeight, uint xCount, uint yCount)
        {
            if (points == null || points.Any() == false) return;

            var groundPolygon = GetGroundPolygon(points, groundHeight, xCount, yCount).ToList();

            panel.DrawFillPolygon(Color.FromArgb(50, 50, 50), groundPolygon);
        }

        public static Vector2D[] GetGroundPolygon(IEnumerable<Point> points, uint groundHeight, uint xCount, uint yCount)
        {
            var groundPoints = GetGroundPoints(points, groundHeight, xCount).ToList();

            uint extraY = 200; //Sorge mit dieser Zahl dafür, dass das GroundPolygon sehr weit nach unten geht und damit bis zum unteren Fensterrand reicht

            groundPoints.Add(new Vector2D(xCount, yCount + extraY));
            groundPoints.Add(new Vector2D(0, yCount + extraY));

            return groundPoints.ToArray();
        }

        public static Vector2D[] GetGroundPoints(IEnumerable<Point> points, uint groundHeight, uint xCount)
        {
            List<Point> newPoints = new List<Point>();
            if (points.First().X > 0)
            {
                newPoints.Add(new Point(0, 0));
            }
            newPoints.AddRange(points);
            if (points.Last().X != xCount)
            {
                newPoints.Add(new Point((int)xCount, 0));
            }
            return newPoints.Select(x => PolyPointToPixel(x, groundHeight)).ToArray();
        }

        public static void DrawAnchorPoints(GraphicPanel2D panel, IEnumerable<Point> points)
        {
            if (points == null || points.Any() == false) return;

            Color anchorPointColor = Color.FromArgb(122, 125, 69);

            foreach (var point in points)
            {
                DrawGridPoint(panel, anchorPointColor, point);
            }
        }

        //polyPoint = DefineGround.GetPolygonPoint
        public static void DrawPolyPoint(GraphicPanel2D panel, Color color, Point polyPoint, uint groundHeight)
        {
            var pixelPosition = PolyPointToPixel(polyPoint, groundHeight);
            DrawFillCircle(panel, color, pixelPosition, 0.25f);
        }

        private static Vector2D PolyPointToPixel(Point point, uint groundHeight)
        {
            return new Vector2D(point.X, (groundHeight + point.Y));
        }

        //gridPoint = MouseGrid.SnapToInt
        public static void DrawGridPoint(GraphicPanel panel, Color color, Point gridPoint)
        {
            DrawFillCircle(panel, color, new Vector2D(gridPoint.X, gridPoint.Y), 0.25f);
        }

        public static void DrawFillCircle(GraphicPanel panel, Color color, Vector2D pos, float radius)
        {
            panel.DrawFillCircleWithTriangles(color, pos, radius, 10);

            //So ist der Punkt zu klein, da intern über alle Pixel des Kreises in Einerschritte gelaufen wird und Radius aber kleiner 1 ist
            //panel.DrawFillCircle(color, new Vector2D(gridPoint.X, gridPoint.Y), 0.25f); 

        }

        public static void DrawBar(GraphicPanel panel, Color color, Point p1, Point p2)
        {
            float lineWidth = 3;
            panel.DrawLine(new Pen(color, lineWidth), new Vector2D(p1.X, p1.Y), new Vector2D(p2.X, p2.Y));
            DrawGridPoint(panel, color, p1);
            DrawGridPoint(panel, color, p2);
        }

        public static void DrawBar(GraphicPanel panel, Color point1Color, Color point2Color, Color lineColor, Point p1, Point p2)
        {
            float lineWidth = 3;
            panel.DrawLine(new Pen(lineColor, lineWidth), new Vector2D(p1.X, p1.Y), new Vector2D(p2.X, p2.Y));
            DrawGridPoint(panel, point1Color, p1);
            DrawGridPoint(panel, point2Color, p2);
        }


        public static float GetRightEdgeOfBridge(Vector2D[] groundPoints)
        {
            float groundY = groundPoints.Last().Y;
            for (int i = groundPoints.Length - 1; i > 0; i--)
            {
                if (groundPoints[i].Y != groundY)
                    return groundPoints[i + 1].X;
            }

            return 0;
        }

        public static float GetWaterHeight(uint waterHeight, uint groundHeight)
        {
            float waterY = groundHeight + waterHeight;
            return waterY;
        }
    }
}
