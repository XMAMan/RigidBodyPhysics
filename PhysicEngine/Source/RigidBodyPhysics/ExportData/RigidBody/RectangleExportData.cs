using RigidBodyPhysics.MathHelper;

namespace RigidBodyPhysics.ExportData.RigidBody
{
    public class RectangleExportData : PropertysExportData, IExportRigidBody
    {
        public Vec2D Size { get; set; }
        public bool BreakWhenMaxPushPullForceIsReached { get; set; }
        public float MaxPushPullForce { get; set; }

        public RectangleExportData() { }

        public RectangleExportData(RectangleExportData copy)
            :base(copy)
        {
            this.Size = new Vec2D(copy.Size);
            this.BreakWhenMaxPushPullForceIsReached = copy.BreakWhenMaxPushPullForceIsReached;
            this.MaxPushPullForce = copy.MaxPushPullForce;
        }

        public IExportRigidBody GetCopy()
        {
            return new RectangleExportData(this);
        }
    }
}
