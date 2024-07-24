using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints
{
    internal class ConstraintConstructorData
    {
        public SolverSettings Settings;
        public List<IRigidBody> Bodies;
        public float Dt;
        public float InvDt;
    }
}
