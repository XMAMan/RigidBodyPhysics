using PhysicGlobal;
using System.Drawing;

namespace KeyFrameGlobal
{
    public interface IAnimationModelDrawer
    {
        RectangleF GetBoundingBoxFromScene();
        void Draw(Camera2D camera);
    }
}
