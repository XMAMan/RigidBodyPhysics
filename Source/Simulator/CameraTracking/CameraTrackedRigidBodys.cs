using GraphicMinimal;
using LevelEditorGlobal;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace Simulator.CameraTracking
{
    internal class CameraTrackedRigidBodys : ICameraTrackedItem
    {
        private IPublicRigidBody[] bodys;
        public RectangleF BoundingBox => GetBoundingBox();

        private float[] radi;
        public CameraTrackedRigidBodys(IPublicRigidBody[] bodys)
        {
            this.bodys = bodys;
            this.radi = bodys.Select(x => GetRadius(x)).ToArray();
        }

        private RectangleF GetBoundingBox()
        {
            Vector2D min = new Vector2D(float.MaxValue, float.MaxValue);
            Vector2D max = new Vector2D(float.MinValue, float.MinValue);
            for (int i = 0; i < bodys.Length; i++)
            {
                var body = bodys[i];
                min.X = Math.Min(min.X, body.Center.X - radi[i]);
                min.Y = Math.Min(min.Y, body.Center.Y - radi[i]);
                max.X = Math.Max(max.X, body.Center.X + radi[i]);
                max.Y = Math.Max(max.Y, body.Center.Y + radi[i]);
            }
            var range = max - min;
            return new RectangleF(min.X, min.Y, range.X, range.Y);
        }

        private static float GetRadius(IPublicRigidBody body)
        {
            if (body is IPublicRigidRectangle)
            {
                var r = (IPublicRigidRectangle)body;
                return new Vector2D(r.Size.X, r.Size.Y).Length() / 2;
            }

            if (body is IPublicRigidCircle)
            {
                var r = (IPublicRigidCircle)body;
                return r.Radius;
            }

            if (body is IPublicRigidPolygon)
            {
                var r = (IPublicRigidPolygon)body;
                return PolygonHelper.GetBoundingBoxFromPolygon(r.Vertex).Radius;
            }

            throw new ArgumentException("Unknown type " + body.GetType());
        }
    }
}
