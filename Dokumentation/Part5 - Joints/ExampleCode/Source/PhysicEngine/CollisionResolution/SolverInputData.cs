using PhysicEngine.CollisionDetection;
using PhysicEngine.Joints;
using PhysicEngine.MouseBodyClick;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution
{
    internal class SolverInputData
    {
        internal List<IRigidBody> Bodies;
        internal List<IJoint> Joints;
        internal RigidBodyCollision[] Collisions;
        internal MouseConstraintData MouseData;
        internal float Dt;
        internal SolverSettings Settings;

        public SolverInputData(List<IRigidBody> bodies, List<IJoint> joints, RigidBodyCollision[] collisions, MouseConstraintData mouseData, float dt, SolverSettings settings)
        {
            this.Bodies = bodies;
            this.Joints = joints;
            this.Collisions = collisions;
            this.MouseData = mouseData;
            this.Dt = dt;
            this.Settings = settings;
        }

        public SolverInputData(SolverInputData copy)
        {
            this.Bodies = copy.Bodies;
            this.Joints = copy.Joints;
            this.Collisions = copy.Collisions;
            this.MouseData = copy.MouseData;
            this.Dt = copy.Dt;
            this.Settings = copy.Settings;
        }
    }
}
