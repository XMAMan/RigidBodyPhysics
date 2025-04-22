using RigidBodyPhysics.MathHelper;

namespace DynamicObjCreation.PolygonIntersection
{
    //Schnittpunkt zwischen den Kanten Poly1[Poly1Edge]..Poly1[Poly1Edge+1] und Poly2[Poly2Edge] .. Poly2[Poly2Edge+1]
    internal class BorderPoint
    {
        public int Poly1Edge { get; }   //index aus poly1 
        public int Poly2Edge { get; }   //index aus poly1
        public float T1 { get; }        //Abstand zwischen this.Position und Poly1[Poly1Edge]
        public float T2 { get; }        //Abstand zwischen this.Position und Poly2[Poly2Edge]
        public Vec2D Position { get; }
        public bool Visited = false;    //Wurde dieser BorderPoint schon genutzt, um damit ein Polygon zu bilden? 

        public BorderPoint(int poly1Edge, int poly2Edge, float t1, float t2, Vec2D position)
        {
            Poly1Edge = poly1Edge;
            Poly2Edge = poly2Edge;
            T1 = t1;
            T2 = t2;
            Position = position;
        }
    }
}
