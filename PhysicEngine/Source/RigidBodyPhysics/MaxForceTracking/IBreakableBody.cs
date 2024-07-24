using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace RigidBodyPhysics.MaxForceTracking
{
    internal interface IBreakableBody : IRigidBody
    {
        float GetPushPullForce();
        bool IsBroken { get; set; } //Dieses Flag wird vom MaxForceTracker gesetzt
        bool BreakWhenMaxPushPullForceIsReached { get; }
        float MaxPushPullForce { get; }
    }
}
