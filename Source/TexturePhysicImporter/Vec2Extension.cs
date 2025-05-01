using PhysicGlobal;
using System.Drawing;

namespace TexturePhysicImporter
{
    internal static class Vec2Extension
    {
        internal static RectangleF ToRectangleF(this RigidBodyPhysics.MathHelper.BoundingBox box)
        {
            return new RectangleF(box.Min.X, box.Min.Y, box.Width, box.Height);
        }
    }
}
