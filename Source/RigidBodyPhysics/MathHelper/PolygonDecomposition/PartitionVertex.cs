namespace RigidBodyPhysics.MathHelper.PolygonDecomposition
{
    internal class PartitionVertex
    {
        public bool IsConvex;

        public Vec2D P;
        public PartitionVertex Previous = null;
        public PartitionVertex Next = null;

        public void UpdateVertexReflexity()
        {
            this.IsConvex = !PolyPointHelper.IsReflex(this.Previous.P, this.P, this.Next.P);
        }

        public bool InCone(Vec2D p)
        {
            return PolyPointHelper.InCone(this.Previous.P, this.P, this.Next.P, p);
        }
    }
}
