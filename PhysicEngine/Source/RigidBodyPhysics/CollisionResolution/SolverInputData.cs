using RigidBodyPhysics.CollisionDetection;
using RigidBodyPhysics.MouseBodyClick;
using RigidBodyPhysics.RuntimeObjects.AxialFriction;
using RigidBodyPhysics.RuntimeObjects.Joints;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using RigidBodyPhysics.RuntimeObjects.RotaryMotor;
using RigidBodyPhysics.RuntimeObjects.Thruster;

namespace RigidBodyPhysics.CollisionResolution
{
    internal class SolverInputData
    {
        internal List<IRigidBody> Bodies;
        internal List<IJoint> Joints;
        internal List<IThruster> Thrusters;
        internal List<IRotaryMotor> Motors;
        internal List<IAxialFriction> AxialFrictions;
        internal RigidBodyCollision[] Collisions;
        internal MouseConstraintData MouseData;
        internal float Dt;
        internal SolverSettings Settings;

        public SolverInputData(List<IRigidBody> bodies, List<IJoint> joints, List<IThruster> thrusters, List<IRotaryMotor> motors, List<IAxialFriction> axialFrictions, RigidBodyCollision[] collisions, MouseConstraintData mouseData, float dt, SolverSettings settings)
        {
            this.Bodies = bodies;
            this.Joints = joints;
            this.Thrusters = thrusters;
            this.Motors = motors;
            this.AxialFrictions = axialFrictions;
            this.Collisions = collisions;
            this.MouseData = mouseData;
            this.Dt = dt;
            this.Settings = settings;
        }

        public SolverInputData(SolverInputData copy)
        {
            this.Bodies = copy.Bodies;
            this.Joints = copy.Joints;
            this.Thrusters = copy.Thrusters;
            this.Motors = copy.Motors;
            this.AxialFrictions = copy.AxialFrictions;
            this.Collisions = copy.Collisions;
            this.MouseData = copy.MouseData;
            this.Dt = copy.Dt;
            this.Settings = copy.Settings;
        }
    }
}
