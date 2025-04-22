namespace PhysicEngine.MathHelper.PolygonDecomposition
{
    internal class IndexPoly
    {
        public long[] Indizes;

        public IndexPoly(long[] indizes)
        {
            this.Indizes = indizes;
        }

        public IndexPoly(int size)
        {
            this.Indizes = new long[size];
            for (long i = 0; i < size; i++)
                this.Indizes[i] = i;
        }
    }
}
