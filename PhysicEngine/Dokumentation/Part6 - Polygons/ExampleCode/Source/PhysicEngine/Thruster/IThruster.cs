using PhysicEngine.MathHelper;
using PhysicEngine.RigidBody;

namespace PhysicEngine.Thruster
{
    internal interface IThruster : IExportableThruster, IPublicThruster
    {
        IRigidBody B1 { get; }
        Vec2D R1 { get; } //Angabe in Weltkoordinaten
        Vec2D Force { get; } //Angabe in Weltkoordinaten
        bool IsEnabled { get; }
        void UpdateAnchorPoints(); //Muss aufgerufen werden, wenn sich die Position der Bodys geändert hat
    }
}
