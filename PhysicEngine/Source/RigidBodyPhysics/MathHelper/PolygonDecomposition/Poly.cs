namespace RigidBodyPhysics.MathHelper.PolygonDecomposition
{
    internal class Poly
    {
        public Vec2D[] Points { get; private set; }
        public Poly(Vec2D[] points)
        {
            this.Points = points;
        }

        public bool Valid()
        {
            return this.Points.Length >= 3;
        }

        public bool IsCCW()
        {
            float area = 0;
            for (int i = 0; i < this.Points.Length; i++)
            {
                var p1 = this.Points[i];
                var p2 = this.Points[(i + 1) % this.Points.Length];

                area += Vec2D.ZValueFromCross(p1, p2); //Area from Triangle p1-p2-[0;0] = 1/2*|Cross(p1,p2)|
            }
            return area > 0;
        }
    }
}
