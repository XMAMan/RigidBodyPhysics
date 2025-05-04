using PhysicGlobal;

namespace KeyFrameGlobal
{
    public interface IAnimationModelDrawer
    {
        BoundingBox GetBoundingBoxFromScene();
        void Draw(Camera2D camera);
    }
}
