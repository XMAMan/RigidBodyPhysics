using PhysicEngine.Joints;
using PhysicEngine.MouseBodyClick;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints
{
    internal class ConstraintConstructorData
    {
        public SolverSettings Settings;
        public List<IRigidBody> Bodies;
        public List<IJoint> Joints;
        public MouseConstraintData MouseData;
        public float Dt;
        public float InvDt;
    }
}
