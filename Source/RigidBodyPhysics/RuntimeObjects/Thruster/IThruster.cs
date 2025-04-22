using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace RigidBodyPhysics.RuntimeObjects.Thruster
{
    internal interface IThruster : IExportableThruster, IPublicThruster
    {
        IRigidBody B1 { get; }
        Vec2D R1 { get; } //Angabe in Weltkoordinaten
        bool IsEnabled { get; }
        void UpdateAnchorPoints(); //Muss aufgerufen werden, wenn sich die Position der Bodys geändert hat
    }
}
