namespace RigidBodyPhysics.MathHelper
{
    public class BoundingBox
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

        public float Width
        {
            get
            {
                return Max.X - Min.X;
            }
        }

        public float Height
        {
            get
            {
                return Max.Y - Min.Y;
            }
        }

        public static BoundingBox GetBoxFromBoxes(IEnumerable<BoundingBox> boundingBoxes)
        {
            Vec2D min = new Vec2D(float.MaxValue, float.MaxValue);
            Vec2D max = new Vec2D(float.MinValue, float.MinValue);
            foreach (BoundingBox box in boundingBoxes)
            {
                if (box.Min.X < min.X)
                    min.X = box.Min.X;

                if (box.Min.Y < min.Y)
                    min.Y = box.Min.Y;

                if (box.Max.X > max.X)
                    max.X = box.Max.X;

                if (box.Max.Y > max.Y)
                    max.Y = box.Max.Y;
            }

            return new BoundingBox(min, max);
        }

        public static BoundingBox GetBoxFromPoints(IEnumerable<Vec2D> points)
        {
            Vec2D min = new Vec2D(float.MaxValue, float.MaxValue);
            Vec2D max = new Vec2D(float.MinValue, float.MinValue);
            foreach (var point in points)
            {
                if (point.X < min.X)
                    min.X = point.X;

                if (point.Y < min.Y)
                    min.Y = point.Y;

                if (point.X > max.X)
                    max.X = point.X;

                if (point.Y > max.Y)
                    max.Y = point.Y;
            }

            return new BoundingBox(min, max);
        }
    }
}
