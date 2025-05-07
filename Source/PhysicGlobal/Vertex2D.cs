namespace PhysicGlobal
{
    public class Vertex2D
    {
        public Vec2D Position { get; private set; }

        public Vec2D Textcoord { get; private set; }

        public Vertex2D(Vec2D pos, Vec2D textcoord)
        {
            Position = pos;
            Textcoord = textcoord;
        }

        public Vertex2D(float x, float y, float u, float v)
        {
            this.Position = new Vec2D(x, y);
            this.Textcoord = new Vec2D(u, v);
        }
    }
}
