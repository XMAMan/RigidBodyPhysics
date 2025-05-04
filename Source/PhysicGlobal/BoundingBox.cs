using System.Drawing;

namespace PhysicGlobal
{
    public class BoundingBox
    {
        public Vec2D Min { get; set; }
        public Vec2D Max { get; set; }

        //Wird für den Json-Serializer benötigt
        public BoundingBox() { }

        public BoundingBox(Vec2D min, Vec2D max)
        {
            Min = min;
            Max = max;
        }

        public BoundingBox(float minX, float minY, float width, float height)
        {
            Min = new Vec2D(minX, minY);
            Max = new Vec2D(minX + width, minY + height);
        }

        public Vec2D GetCenter()
        {
            return new Vec2D(Min.X + (Max.X - Min.X) / 2, Min.Y + (Max.Y - Min.Y) / 2);
        }

        public float X
        {
            get
            {
                return Min.X;
            }
        }

        public float Y
        {
            get
            {
                return Min.Y;
            }
        }

        public float GetRadius()
        {
            return (Max - Min).Length() / 2;
        }

        public float GetWidth()
        {
            return Max.X - Min.X;
        }

        public float GetHeight()
        {
            return Max.Y - Min.Y;
        }

        public SizeF GetSize()
        {
            return new SizeF(GetWidth(), GetHeight());
        }

        public static BoundingBox GetBoxFromBoxes(IEnumerable<BoundingBox> boundingBoxes)
        {
            //if (boundingBoxes.Any() == false) return new BoundingBox(0, 0, 0, 0);

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

        public static BoundingBox GetBoxFromTwoPoints(Vec2D p1, Vec2D p2)
        {
            Vec2D min = new Vec2D(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y));
            Vec2D max = new Vec2D(Math.Max(p1.X, p2.X), Math.Max(p1.Y, p2.Y));

            return new BoundingBox(min, max);
        }

        public bool IsPointInBox( Vec2D point)
        {
            return point.X >= this.Min.X && point.X <= this.Max.X && point.Y >= this.Min.Y && point.Y <= this.Max.Y;
        }
    }
}
