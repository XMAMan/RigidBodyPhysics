using RigidBodyPhysics.RuntimeObjects.Joints;

namespace RigidBodyPhysics.MaxForceTracking
{
    internal interface IBreakableJoint : IJoint
    {
        bool IsBroken { get; set; } //Dieses Flag wird vom MaxForceTracker gesetzt
        bool BreakWhenMaxForceIsReached { get; }
        float MaxForceToBreak { get; }
        float CurrentForce { get; } //Diese Kraft wurde im letzen TimeStep auf das Gelenk angwendet (Entspricht dem PointToPoint-AccumuletedImpulse oder dem DistanceImpluse)
    }

    //Beim Distancejoint können neben Druck-Kräfte auch Zugkräfte wirken
    internal interface IBreakablePushPullJoint : IBreakableJoint
    {
        float MinForceToBreak { get; }
    }
}
