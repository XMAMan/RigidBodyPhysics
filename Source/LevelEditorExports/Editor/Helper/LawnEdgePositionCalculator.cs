using PhysicGlobal;

namespace LevelEditorExports.Editor.Helper
{
    //Berechnet die PolygonWith4Points für ein gegebenes IPolygon-Objekt mit zwei PolygonPoints
    public class LawnEdgePositionCalculator
    {
        public interface IPolygon
        {
            Vec2D[] Points { get; }
            bool IsOutside { get; } //Zeigen die Normalen nach Außen?
        }

        protected IPolygon polygon;

        public float LawnHeight = 40; //So viele interne Einheiten ist das Gras hoch

        public LawnEdgePositionCalculator(IPolygon polygon)
        {
            this.polygon = polygon;
        }


        public class PolygonPoint
        {
            private IPolygon polygon;

            public int Index;   //Index aus polygon.Points-Array
            public float FPos;  //0..1: 0 -> Punkt liegt bei Point[Index]; 1 -> Punkt liegt bei Point[Index+1]
            public Vec2D Position { get => GetPosition(); }
            public Vec2D Normal { get => GetNormal(); }

            public PolygonPoint(int index, float fPos, IPolygon polygon)
            {
                this.polygon = polygon;

                Index = index;
                FPos = fPos;
            }

            private Vec2D GetNormal()
            {
                var p1 = polygon.Points[Index % polygon.Points.Length];
                var p2 = polygon.Points[(Index + 1) % polygon.Points.Length];
                var normal = (p2 - p1).Normalize().Spin90();
                if (polygon.IsOutside == false) normal = -normal;
                return normal;
            }

            private Vec2D GetPosition()
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
            private IPolygon parentPolygon; //An diesen Polygon hängt dieses Objekt dran
            public Vec2D[] Points; //[p1A, p1B, p2B, p2A]

            public PolygonWith4Points(Vec2D[] points, IPolygon parentPolygon)
            {
                if (points.Length != 4) throw new ArgumentException("This polygon has only 4 Points");
                Points = points;
                this.parentPolygon = parentPolygon;
            }

            public bool IsPointInside(Vec2D point)
            {
                return PolygonHelper.PointIsInsidePolygon(this.Points, point);
            }

            public float GetArea()
            {
                return PolygonHelper.GetAreaFromPolygon(this.Points);
            }

            public Vec2D GetCenter()
            {
                return (Points[0] + Points[2]) / 2;
            }

            public float GetWidth()
            {
                return (Points[0] - Points[3]).Length();
            }

            public float GetAngle()
            {
                float angle = Vec2D.Angle360(new Vec2D(1, 0), (Points[0] - Points[3]).Normalize());
                if (this.parentPolygon.IsOutside == false) angle += 180;

                return angle;
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

            return new PolygonWith4Points(new Vec2D[]
            {
                p1A, p1B, p2B, p2A
            }, this.polygon);
        }
    }
}
