using PhysicEngine.Joints;
using PhysicEngine.MouseBodyClick;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints
{
    internal class ConstraintConstructorData
    {
        internal SolverSettings Settings;
        internal List<IRigidBody> Bodies;
        internal List<IJoint> Joints;
        internal MouseConstraintData MouseData;
        internal float Dt;
        internal float InvDt;
    }
}
