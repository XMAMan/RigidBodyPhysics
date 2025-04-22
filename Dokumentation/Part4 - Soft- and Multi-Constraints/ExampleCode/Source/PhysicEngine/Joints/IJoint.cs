using GraphicMinimal;
using PhysicEngine.RigidBody;

namespace PhysicEngine.Joints
{
    public interface IJoint : IExportableJoint
    {
        IRigidBody B1 { get; }
        IRigidBody B2 { get; }
        Vector2D Anchor1 { get; } //Angabe in Weltkoordinaten
        Vector2D Anchor2 { get; } //Angabe in Weltkoordinaten
        float AccumulatedImpulse { get; set; }
        void UpdateAnchorPoints(); //Muss aufgerufen werden, wenn sich die Position der Bodys geändert hat
    }
}
