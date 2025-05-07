using PhysicGlobal;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace KeyFramePhysicImporter.Model.PhysicSceneDrawing
{
    internal interface IShape
    {
        IPublicRigidBody PhysicModel { get; }
        Color FillColor { get; set; }
        PhysicGlobal.BoundingBox BoundingBox { get; }
        void Draw(IDrawingPanel panel, Pen borderPen, Color fillColor, Camera2D camera);
    }
}
