using PhysicEngine.CollisionDetection;
using PhysicEngine.Joints;
using PhysicEngine.MouseBodyClick;
using PhysicEngine.RigidBody;
using static PhysicEngine.PhysicScene;

namespace PhysicEngine.CollisionResolution
{
    internal class SolverInputData
    {
        public List<IRigidBody> Bodies;
        public List<IJoint> Joints;
        public RigidBodyCollision[] Collisions;
        public MouseConstraintData MouseData;
        public float Dt;
        public SolverSettings Settings;
        public Helper ResolverHelper;

        public SolverInputData(List<IRigidBody> bodies, List<IJoint> joints, RigidBodyCollision[] collisions, MouseConstraintData mouseData, float dt, SolverSettings settings, Helper resolverHelper)
        {
            this.Bodies = bodies;
            this.Joints = joints;
            this.Collisions = collisions;
            this.MouseData = mouseData;
            this.Dt = dt;
            this.Settings = settings;
            this.ResolverHelper = resolverHelper;
        }

        public SolverInputData(SolverInputData copy)
        {
            this.Bodies = copy.Bodies;
            this.Joints = copy.Joints;
            this.Collisions = copy.Collisions;
            this.MouseData = copy.MouseData;
            this.Dt = copy.Dt;
            this.Settings = copy.Settings;
            this.ResolverHelper = copy.ResolverHelper;
        }
    }
}
