using RigidBodyPhysics.MathHelper;
using System.Drawing;

namespace DynamicObjCreation.RigidBodyDestroying
{
    //Berechnet die Bounding-Box von einer Menge von Punkten. Man gibt dazu an, um wie viel Grad (0..360) die Box gedreht sein soll
    public class RotatedBoundingBox
    {
        public Vec2D Center { get; }
        public float Width { get; }
        public float Height { get; }
        public Vec2D Right { get; }
        public Vec2D Up { get; }
        public Vec2D[] CornerPoints { get; }

        public RotatedBoundingBox(IEnumerable<Vec2D> points, float angleInDegree)
        {
            var right = Vec2D.GetV2FromAngle360(new Vec2D(1, 0), angleInDegree);
            var up = right.Spin90();

            GetRotatedBoundingBoxFromPoints(right, up, points, out Vec2D rotBoxCenter, out SizeF rotBoxSize);

            Right = right;
            Up = up;
            Center = rotBoxCenter;

            Width = rotBoxSize.Width;
            Height = rotBoxSize.Height;

            CornerPoints = new Vec2D[]
            {
                Center - right * Width / 2 - up * Height / 2,
                Center + right * Width / 2 - up * Height / 2,
                Center + right * Width / 2 + up * Height / 2,
                Center - right * Width / 2 + up * Height / 2,
            };
        }

        private static void GetRotatedBoundingBoxFromPoints(Vec2D right, Vec2D up, IEnumerable<Vec2D> points, out Vec2D center, out SizeF size)
        {
            var rightPoints = ProjectPointsOnDirection(right, points);
            var upPoints = ProjectPointsOnDirection(up, points);
            GetMinMaxCenter(rightPoints, out float minX, out float maxX, out float centerX);
            GetMinMaxCenter(upPoints, out float minY, out float maxY, out float centerY);

            center = right * centerX + up * centerY;
            size = new SizeF(maxX - minX, maxY - minY);
        }

        private static float[] ProjectPointsOnDirection(Vec2D rayDir, IEnumerable<Vec2D> points)
        {
            return points.Select(x => x * rayDir).ToArray();
        }

        private static void GetMinMaxCenter(float[] values, out float min, out float max, out float center)
        {
            min = values.Min();
            max = values.Max();
            center = (max + min) / 2;
        }


    }
}
