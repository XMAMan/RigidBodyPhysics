using RigidBodyPhysics.MouseBodyClick;
using RigidBodyPhysics.RuntimeObjects.AxialFriction;
using RigidBodyPhysics.RuntimeObjects.Joints;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using RigidBodyPhysics.RuntimeObjects.RotaryMotor;

namespace RigidBodyPhysics.CollisionResolution.SequentiellImpulse.Constraints
{
    internal class ConstraintConstructorData
    {
        internal SolverSettings Settings;
        internal List<IRigidBody> Bodies;
        internal List<IJoint> Joints;
        internal List<IRotaryMotor> Motors;
        internal List<IAxialFriction> AxialFrictions;
        internal MouseConstraintData MouseData;
        internal float Dt;
        internal float InvDt;
    }
}
