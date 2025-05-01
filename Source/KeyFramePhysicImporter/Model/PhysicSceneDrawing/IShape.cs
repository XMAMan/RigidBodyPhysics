using GraphicPanels;
using PhysicGlobal;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace KeyFramePhysicImporter.Model.PhysicSceneDrawing
{
    internal interface IShape
    {
        IPublicRigidBody PhysicModel { get; }
        Color FillColor { get; set; }
        PhysicGlobal.BoundingBox BoundingBox { get; }
        void Draw(GraphicPanel2D panel, Pen borderPen, Color fillColor, Camera2D camera);
    }
}
