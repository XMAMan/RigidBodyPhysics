using PhysicEngine.RigidBody;

namespace PhysicEngine.RotaryMotor
{
    internal interface IRotaryMotor : IExportableRotaryMotor, IPublicRotaryMotor
    {
        IRigidBody B1 { get; }
        bool IsEnabled { get; }
    }
}
