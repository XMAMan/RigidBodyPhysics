using RigidBodyPhysics.CollisionResolution.SequentiellImpulse.Constraints;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace RigidBodyPhysics.RuntimeObjects.AxialFriction
{
    internal interface IAxialFriction : IExportableAxialFriction, IPublicAxialFriction, ISISolvedRuntimeObject
    {
        IRigidBody B1 { get; }
        Vec2D R1 { get; } //Angabe in Weltkoordinaten
        Vec2D ForceDirection { get; } //Angabe in Weltkoordinaten
        float Friction { get; }
        float AccumulatedFrictionImpulse { get; set; }
        void UpdateAnchorPoints(); //Muss aufgerufen werden, wenn sich die Position der Bodys geändert hat
    }
}
