using RigidBodyPhysics.MathHelper;
using PolyHelp = RigidBodyPhysics.MathHelper.PolygonHelper;

namespace DynamicObjCreation.PolygonIntersection
{
    //Speichert alle Schnittpunkte zwischen zwei Polygonen
    internal class PolyIntersections
    {
        private PolyEdge[] edge1; //Alle Kanten von Poly1
        private PolyEdge[] edge2; //Alle Kanten von Poly2

        public bool HasAnyPoints { get; private set; }

        public PolyIntersections(Vec2D[] poly1, Vec2D[] poly2)
        {
            edge1 = poly1.Select((x, index) => new PolyEdge(index)).ToArray();
            edge2 = poly2.Select((x, index) => new PolyEdge(index)).ToArray();

            for (int x = 0; x < poly1.Length; x++)
                for (int y = 0; y < poly2.Length; y++)
                {
                    var intersectionPoint = GetIntersectionPointBetweenToLines(poly1[x], poly1[(x + 1) % poly1.Length], poly2[y], poly2[(y + 1) % poly2.Length], out float t1, out float t2);
                    if (intersectionPoint != null)
                    {
                        var point = new BorderPoint(x, y, t1, t2, intersectionPoint);
                        edge1[x].AddBorderPoint(point);
                        edge2[y].AddBorderPoint(point);
                    }
                }

            foreach (var e1 in edge1)
                e1.OrderByT1();

            foreach (var e2 in edge2)
                e2.OrderByT2();

            HasAnyPoints = edge1.Any(x => x.HasAnyPoints());
        }


        //Linie 1 = p1..p2       Linie 2=p3..p4
        private static Vec2D GetIntersectionPointBetweenToLines(Vec2D p1, Vec2D p2, Vec2D p3, Vec2D p4, out float t1, out float t2)
        {
            float epsilon = 0.0001f; //Wegen Testcase Intersect11 muss hier ein Wert größer 0 stehen

            var p1p2 = p2 - p1;
            var p3p4 = p4 - p3;
            float length1 = p1p2.Length();
            float length2 = p3p4.Length();
            var dir1 = p1p2 / length1;
            var dir2 = p3p4 / length2;
            PolyHelp.IntersectionTwoRays(p1, dir1, p3, dir2, out t1, out t2);
            if (float.IsNaN(t1) || float.IsInfinity(t1) || t1 < -epsilon || t1 > length1 + epsilon)
            {
                return null;
            }
            if (float.IsNaN(t2) || float.IsInfinity(t2) || t2 < -epsilon || t2 > length2 + epsilon)
            {
                return null;
            }

            return p1 + dir1 * t1;
        }

        //Aktuell steht der Running-Punkt auf den Polygon 'polyIndex' an dessen Kante edgeIndex mit Abstand t1 zum Startpunkt von der Kante
        //Schaue ob es auf der Kante ein weiteren BorderPoint in Laufrichtung gibt
        public BorderPoint TryToGetNextBorderPoint(RunningPoint runningPoint)
        {
            if (runningPoint.PolyIndex == 0)
                return edge1[runningPoint.EdgeIndex].TryToGetNextT1Point(runningPoint.T);

            if (runningPoint.PolyIndex == 1)
                return edge2[runningPoint.EdgeIndex].TryToGetNextT2Point(runningPoint.T);

            throw new ArgumentException("polyIndex must be 0 or 1");
        }

        public BorderPoint GetFirstUnvisitedPoly1BorderPoint()
        {
            foreach (var edge in edge1)
            {
                var point = edge.GetFirstUnvisitedPoint();
                if (point != null)
                    return point;
            }

            return null;
        }

        public override string ToString()
        {
            string s1 = "Poly1\n" + string.Join("\n", edge1.Where(x => x.HasAnyPoints()).Select(x => " " + x.ToStringForT1()));
            string s2 = "Poly2\n" + string.Join("\n", edge2.Where(x => x.HasAnyPoints()).Select(x => " " + x.ToStringForT2()));
            return s1 + "\n" + s2;
        }


    }
}
