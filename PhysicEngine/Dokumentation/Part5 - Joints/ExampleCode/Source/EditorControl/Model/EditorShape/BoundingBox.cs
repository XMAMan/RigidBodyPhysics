using PhysicEngine.MathHelper;

namespace EditorControl.Model.EditorShape
{
    internal class BoundingBox
    {
        public Vec2D Min { get; private set; }
        public Vec2D Max { get; private set; }

        public BoundingBox(Vec2D min, Vec2D max)
        {
            Min = min;
            Max = max;
        }

        public Vec2D Center
        {
            get
            {
                return new Vec2D(Min.X + (Max.X - Min.X) / 2, Min.Y + (Max.Y - Min.Y) / 2);
            }
        }

        public float Radius
        {
            get
            {
                return (Max - Min).Length() / 2;
            }
        }
    }
}
