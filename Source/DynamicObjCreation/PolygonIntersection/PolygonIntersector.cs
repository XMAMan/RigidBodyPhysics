using RigidBodyPhysics.MathHelper;
using PolyHelp = RigidBodyPhysics.MathHelper.PolygonHelper;

namespace DynamicObjCreation.PolygonIntersection
{
    //Bildet die Schnittmenge zwischen zwei Polygonen und gibt die Menge in Form einer Polygon-Liste zurück
    public static class PolygonIntersector
    {
        public static List<Vec2D[]> GetIntersection(Vec2D[] poly1, Vec2D[] poly2)
        {
            bool[] isInside1 = poly1.Select(x => PolyHelp.PointIsInsidePolygon(poly2, x)).ToArray(); //Welche Punkte von Poly1 liegen in Poly2?
            bool[] isInside2 = poly2.Select(x => PolyHelp.PointIsInsidePolygon(poly1, x)).ToArray(); //Welche Punkte von Poly2 liegen in Poly1?

            PolyIntersections polyIntersections = new PolyIntersections(poly1, poly2);

            bool noIntersections = polyIntersections.HasAnyPoints == false;
            bool isInside1AllFalse = isInside1.All(x => x == false);
            bool isInside2AllFalse = isInside2.All(x => x == false);

            //Fall 1: Polygone haben keine gemeinsamen Punkte -> Gib null zurück
            if (noIntersections && isInside1AllFalse && isInside2AllFalse)
                return null;

            //Fall 2: Poly1 liegt komplett in Poly2 -> Gib Poly1 zurück
            if (noIntersections && isInside1.All(x => x == true))
                return new List<Vec2D[]>() { poly1 };

            //Fall 3: Poly2 liegt komplett in Poly1 -> Gib Poly2 zurück
            if (noIntersections && isInside2.All(x => x == true))
                return new List<Vec2D[]>() { poly2 };

            //return new List<Vec2D[]>() { poly2 };

            //Fall 4: Es gibt ein oder mehrere BorderPoint-Ringe -> Gebe für jeden Ring ein Polygon zurück
            List<Vec2D[]> polys = new List<Vec2D[]>();
            while (true)
            {
                var firstBorderPoint = polyIntersections.GetFirstUnvisitedPoly1BorderPoint();
                if (firstBorderPoint == null) break;
                var poly = RunAboveBorderPointRint(poly1, poly2, isInside1, isInside2, firstBorderPoint, polyIntersections);
                if (poly == null) return null;
                polys.Add(poly);
            }
            if (polys.Count == 0) return null;

            return polys;
        }



        //Beginne auf den Schnittpunkt zwischen zwei Polygonkanten und laufe entlang der Kante bis zum nächsten Schnittpunkt/Polygonecke, bis du wieder am Startpunkt raus kommst
        private static Vec2D[] RunAboveBorderPointRint(Vec2D[] poly1, Vec2D[] poly2, bool[] isInside1, bool[] isInside2, BorderPoint borderStart, PolyIntersections polyIntersections)
        {
            List<bool[]> isInside = new List<bool[]>() { isInside1, isInside2 };
            List<Vec2D[]> polys = new List<Vec2D[]>() { poly1, poly2 };
            List<Vec2D> points = new List<Vec2D>();

            int polyStart = 0;
            int edgeStartIndex = borderStart.Poly1Edge;
            float tStart = borderStart.T1;

            borderStart.Visited = true;
            points.Add(borderStart.Position); //Starte auf BorderPoint von Poly1


            RunningPoint runningPoint = new RunningPoint(polyStart, edgeStartIndex, tStart);
            var startPoint = new RunningPoint(runningPoint); //Beim StartPoint ist noch nicht klar, ob er auf ein BorderPoint beginnt, welche in den Intersection-Bereich reingeht oder rausgeht

            while (true)
            {
                var borderPoint = polyIntersections.TryToGetNextBorderPoint(runningPoint);

                //Fall 1: Nächster Punkt ist ein BorderPoint -> Springe auf das andere Polygon
                if (borderPoint != null)
                {
                    if (borderPoint.Visited)
                        break;

                    borderPoint.Visited = true;
                    points.Add(borderPoint.Position);
                    runningPoint.JumpToNextBorderPoint(borderPoint);
                }
                else
                {
                    //Fall 2: Nächster Punkt ist ein Eckpunkt 
                    runningPoint.JumpToNextCornerPoint(polys[runningPoint.PolyIndex].Length);
                    if (isInside[runningPoint.PolyIndex][runningPoint.EdgeIndex]) //-> Nimm diesen Punkt wenn er im anderen Polygon liegt
                    {
                        points.Add(polys[runningPoint.PolyIndex][runningPoint.EdgeIndex]); //Eckpunkt
                    }
                    else
                    {
                        //Man sollte nur ein einziges Mal hier in diesen Block landen.
                        //Wenn hier null rauskommt, dann ist die Schnittmenge zwischen zwei Polygonen vermutlich so klein, dass es nicht geht
                        if (startPoint == null) return null;

                        //StartPunkt wurde auf BorderPoint gestartet, welcher das Polygon verläßt
                        // -> Wechsle die Richtung auf dem StartPoint und versuche es erneut
                        startPoint.JumpToNextBorderPoint(borderStart);
                        runningPoint = startPoint;
                        startPoint = null; 
                    }
                }
            }

            return points.ToArray();
        }

        public static int IndexOfWhereCondition<T>(this IEnumerable<T> list, Func<T, bool> condition)
        {
            int index = 0;
            foreach (T item in list)
            {
                if (condition(item))
                {
                    return index;
                }

                index++;
            }

            return -1;
        }


    }

    

    

    

    
}
