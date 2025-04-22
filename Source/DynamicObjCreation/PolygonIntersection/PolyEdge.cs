namespace DynamicObjCreation.PolygonIntersection
{
    //Polygonkante von Poly[StartPoint] .. Poly[StartPoint+1]
    internal class PolyEdge
    {
        private int startPointIndex;
        private List<BorderPoint> borderPoints = new List<BorderPoint>();

        public PolyEdge(int startPointIndex)
        {
            this.startPointIndex = startPointIndex;
        }

        public bool HasAnyPoints()
        {
            return borderPoints.Any();
        }

        public void AddBorderPoint(BorderPoint point)
        {
            borderPoints.Add(point);
        }

        public void OrderByT1()
        {
            borderPoints = borderPoints.OrderBy(x => x.T1).ToList();
        }

        public void OrderByT2()
        {
            borderPoints = borderPoints.OrderBy(x => x.T2).ToList();
        }

        public BorderPoint TryToGetNextT1Point(float t1)
        {
            foreach (var p in borderPoints)
            {
                if (p.T1 > t1)
                {
                    return p;
                }
            }

            return null;
        }

        public BorderPoint TryToGetNextT2Point(float t2)
        {
            foreach (var p in borderPoints)
            {
                if (p.T2 > t2)
                {
                    return p;
                }
            }

            return null;
        }

        public BorderPoint GetFirstUnvisitedPoint()
        {
            return borderPoints.Where(x => x.Visited == false).FirstOrDefault();
        }

        public string ToStringForT1()
        {
            if (borderPoints.Count == 0)
                return startPointIndex + ":";

            return startPointIndex + ": " + string.Join(",", borderPoints.Select(x => (int)x.T1));
        }

        public string ToStringForT2()
        {
            if (borderPoints.Count == 0)
                return startPointIndex + ":";

            return startPointIndex + ": " + string.Join(",", borderPoints.Select(x => (int)x.T2));
        }
    }
}
