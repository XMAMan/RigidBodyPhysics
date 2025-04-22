using System.Drawing;
using WpfControls.Controls.CameraSetting;

namespace KeyFrameGlobal
{
    public interface IAnimationModelDrawer
    {
        RectangleF GetBoundingBoxFromScene();
        void Draw(Camera2D camera);
    }
}
