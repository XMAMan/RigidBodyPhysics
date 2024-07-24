using RigidBodyPhysics.RuntimeObjects.AxialFriction;
using RigidBodyPhysics.RuntimeObjects.Joints;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using RigidBodyPhysics.RuntimeObjects.RotaryMotor;
using RigidBodyPhysics.RuntimeObjects.Thruster;

namespace RigidBodyPhysics.ExportData
{
    //All die Laufzeitobjekte, die Erzeugt werden, wenn man ein PhysicSceneExportData in die PhysicScene per AddPhysicScene einbringt
    public class PhysicScenePublicData
    {
        public IPublicRigidBody[] Bodies { get; }
        public IPublicJoint[] Joints { get; }
        public IPublicThruster[] Thrusters { get; }
        public IPublicRotaryMotor[] Motors { get; }
        public IPublicAxialFriction[] AxialFrictions { get; }

        public PhysicScenePublicData(IPublicRigidBody[] bodies, IPublicJoint[] joints, IPublicThruster[] thrusters, IPublicRotaryMotor[] motors, IPublicAxialFriction[] axialFrictions)
        {
            Bodies = bodies;
            Joints = joints;
            Thrusters = thrusters;
            Motors = motors;
            AxialFrictions = axialFrictions;
        }

        public PhysicScenePublicData(PhysicScenePublicData copy)
        {
            Bodies = copy.Bodies;
            Joints = copy.Joints;
            Thrusters = copy.Thrusters;
            Motors = copy.Motors;
            AxialFrictions = copy.AxialFrictions;
        }
    }
}
