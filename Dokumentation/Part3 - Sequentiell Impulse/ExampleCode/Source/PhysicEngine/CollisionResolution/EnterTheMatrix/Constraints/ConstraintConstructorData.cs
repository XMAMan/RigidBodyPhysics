using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.EnterTheMatrix.Constraints
{
    internal class ConstraintConstructorData
    {
        public SolverSettings Settings;
        public List<IRigidBody> Bodies;
        public CollisionPointWithLambda[] Collisions;
        public float Dt;
    }
}
