namespace PhysicEngine.MathHelper
{
    public class ConvexPolygon
    {
        public Vec2D[] Points { get; }
        public bool[] IsOutsideEdge { get; } //[0] = Bezieht sich auf die Kante Points[0]-Points[1]; [Length-1]=Points[Length-1]-Points[0]
        public long[] Indizes { get; } //Bezieht sich auf den Index aus dem concavePolygon aus dem Konstruktor

        public ConvexPolygon(Vec2D[] concavePolygon, long[] indexPoly)
        {
            this.Points = indexPoly.Select(x => concavePolygon[x]).ToArray();
            this.Indizes = indexPoly;

            this.IsOutsideEdge = new bool[this.Points.Length];
            for (int i=0; i< IsOutsideEdge.Length; i++)
            {
                long i1 = indexPoly[i];
                long i2 = indexPoly[(i + 1) % indexPoly.Length];
                this.IsOutsideEdge[i] = (i1 + 1) == i2 || (i1 == concavePolygon.Length - 1 && i2 == 0);
            }
        }
    }
}
