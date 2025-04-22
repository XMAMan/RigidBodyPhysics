using PhysicEngine.RigidBody;

namespace PhysicEngine.MaxForceTracking
{
    internal interface IBreakableBody : IRigidBody
    {
        float GetPushPullForce();
        bool BreakWhenMaxPushPullForceIsReached { get; }
        float MaxPushPullForce { get; }
    }
}
