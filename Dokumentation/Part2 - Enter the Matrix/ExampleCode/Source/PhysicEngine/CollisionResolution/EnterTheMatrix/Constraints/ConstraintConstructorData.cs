using PhysicEngine.CollisionDetection;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.EnterTheMatrix.Constraints
{
    internal class ConstraintConstructorData
    {
        public SolverSettings Settings;
        public List<IRigidBody> Bodies;
        public RigidBodyCollision[] Collisions;
        public float Dt;
    }
}
