using PhysicEngine.Joints;

namespace PhysicEngine.MaxForceTracking
{
    internal interface IBreakableJoint : IJoint
    {
        bool BreakWhenMaxForceIsReached { get; }
        float MaxForceToBreak { get; }
        float CurrentForce { get; } //Diese Kraft wurde im letzen TimeStep auf das Gelenk angwendet (Entspricht dem PointToPoint-AccumuletedImpulse oder dem DistanceImpluse)
    }
}
