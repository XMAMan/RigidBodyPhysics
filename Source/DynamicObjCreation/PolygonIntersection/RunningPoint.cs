namespace DynamicObjCreation.PolygonIntersection
{
    //Punkt der auf der Ecke von ein Polygon oder auf dem Schnittpunkt zwischen zwei Polygonkanten liegt
    class RunningPoint
    {
        public int PolyIndex;   //0 = Poly1; 1 = Poly2
        public int EdgeIndex;   //0..PolygonPoints.Length
        public float T;         //Abstand zum Punkt Polygon[EdgeIndex]

        public RunningPoint(int polyIndex, int edgeIndex, float t)
        {
            PolyIndex = polyIndex;
            EdgeIndex = edgeIndex;
            T = t;
        }

        public RunningPoint(RunningPoint copy)
        {
            PolyIndex = copy.PolyIndex;
            EdgeIndex = copy.EdgeIndex;
            T = copy.T;
        }

        public void JumpToNextBorderPoint(BorderPoint borderPoint)
        {
            if (PolyIndex == 0)
            {
                PolyIndex = 1;
                EdgeIndex = borderPoint.Poly2Edge;
                T = borderPoint.T2;
            }
            else
            {
                PolyIndex = 0;
                EdgeIndex = borderPoint.Poly1Edge;
                T = borderPoint.T1;
            }
        }

        public void JumpToNextCornerPoint(int maxEdgeIndex)
        {
            T = 0;
            EdgeIndex = (EdgeIndex + 1) % maxEdgeIndex;
        }
    }
}
