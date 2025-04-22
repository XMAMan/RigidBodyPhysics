using GraphicMinimal;
using GraphicPanels;
using LevelEditorControl.LevelItems.Polygon;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace LevelEditorControl.EditorFunctions
{
    //Zeichnet eine Rasenkante um ein Polygon
    internal class LawnEdgeDrawer
    {
        public class PolygonPoint
        {
            private ILevelItemPolygon polygon;

            public int Index;   //Index aus polygon.Points-Array
            public float FPos;  //0..1: 0 -> Punkt liegt bei Point[Index]; 1 -> Punkt liegt bei Point[Index+1]
            public Vector2D Position { get => GetPosition(); }
            public Vector2D Normal { get => GetNormal(); }

            public PolygonPoint(int index, float fPos, ILevelItemPolygon polygon)
            {
                this.polygon = polygon;

                Index = index;
                FPos = fPos;
            }

            private Vector2D GetNormal()
            {
                var p1 = polygon.Points[Index % polygon.Points.Length];
                var p2 = polygon.Points[(Index + 1) % polygon.Points.Length];
                var normal = (p2 - p1).Normalize().Spin90();
                if (polygon.IsOutside == false) normal = -normal;
                return normal;
            }

            private Vector2D GetPosition()
            {
                var p1 = polygon.Points[Index % polygon.Points.Length];
                var p2 = polygon.Points[(Index + 1) % polygon.Points.Length];
                return (1 - FPos) * p1 + FPos * p2;
            }

            public void TakeData(PolygonPoint point)
            {
                this.Index = point.Index;
                this.FPos = point.FPos;
            }
        }

        public class PolygonWith4Points
        {
            public Vector2D[] Points;

            public PolygonWith4Points(Vector2D[] points)
            {
                if (points.Length != 4) throw new ArgumentException("This polygon has only 4 Points");
                Points = points;
            }

            public bool IsPointInside(Vector2D point)
            {
                return MathHelper.PointIsInsidePolygon(this.Points, point);
            }

            public float GetArea()
            {
                return MathHelper.GetAreaFromPolygon(this.Points);
            }
        }

        private ILevelItemPolygon polygon;

        public string TextureFile = "#00FF00";
        public float LawnHeight = 40; //So viele interne Einheiten ist das Gras hoch
        public float ZValue = 0;

        public LawnEdgeDrawer(ILevelItemPolygon polygon)
        {
            this.polygon = polygon;
        }

        public void DrawLawn(PolygonPoint p1, PolygonPoint p2, GraphicPanel2D panel)
        {
            var segments = GetAllSegments(p1, p2);

            string texture = this.TextureFile;
            float height = this.LawnHeight;
            panel.ZValue2D = this.ZValue;

            foreach (var segment in segments)
            {
                var p1A = segment.Points[0];
                var p1B = segment.Points[1];
                var p2B = segment.Points[2];
                var p2A = segment.Points[3];

                var center = (p1A + p2B) / 2;
                float width = (p1A - p2A).Length();

                float angle = Vector2D.Angle360(new Vector2D(1, 0), (p1A - p2A).Normalize());
                if (this.polygon.IsOutside == false) angle += 180;

                if (string.IsNullOrEmpty(texture) == false)
                    panel.DrawFillRectangle(texture, center.Xi, center.Yi, (int)width, (int)height, true, Color.White, angle);
                else
                    panel.DrawPolygon(new Pen(Color.Green, 2), segment.Points.ToList());

            }

        }

        public PolygonWith4Points[] GetAllSegments(PolygonPoint p1, PolygonPoint p2)
        {
            if (p1.Index == p2.Index && p1.FPos < p2.FPos)
            {
                return new PolygonWith4Points[] { GetSegment(p1, p2) };
            }

            List<PolygonWith4Points> segments = new List<PolygonWith4Points>();
            int endIndex = p2.Index;
            if (endIndex < p1.Index || (endIndex == p1.Index && p2.FPos < p1.FPos)) endIndex += this.polygon.Points.Length;
            for (int i = p1.Index; i <= endIndex; i++)
            {
                if (i == p1.Index)
                {
                    //Erstes Segment geht von p1 bis p1-EdgeEnd
                    segments.Add(GetSegment(p1, GetPointOnEdge(i, false)));
                }
                else if (i == endIndex)
                {
                    //Letztes Segment geht von p2-EdgeStart bis p2
                    segments.Add(GetSegment(GetPointOnEdge(i, true), p2));
                }
                else
                {
                    //Mittelsegment geht von i-EdgeStart bis i-EdgeEnd
                    segments.Add(GetSegment(GetPointOnEdge(i, true), GetPointOnEdge(i, false)));
                }
            }

            return segments.ToArray();
        }

        private PolygonPoint GetPointOnEdge(int index, bool start)
        {
            if (start)
            {
                return new PolygonPoint(index, 0, polygon);
            }
            else
            {
                return new PolygonPoint(index, 1, polygon);
            }
        }


        //Der Index von p1 und p2 muss gleich sein
        private PolygonWith4Points GetSegment(PolygonPoint p1, PolygonPoint p2)
        {
            var p1A = p1.Position;
            var p1B = p1.Position + p1.Normal * this.LawnHeight;

            var p2A = p2.Position;
            var p2B = p2.Position + p2.Normal * this.LawnHeight;

            return new PolygonWith4Points(new Vector2D[]
            {
                p1A, p1B, p2B, p2A
            });
        }
    }
}
