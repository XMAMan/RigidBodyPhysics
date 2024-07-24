using RigidBodyPhysics.ExportData.AxialFriction;
using RigidBodyPhysics.ExportData.Joints;
using RigidBodyPhysics.ExportData.RigidBody;
using RigidBodyPhysics.ExportData.RotaryMotor;
using RigidBodyPhysics.ExportData.Thruster;

namespace RigidBodyPhysics.ExportData
{
    public class PhysicSceneExportData
    {
        public IExportRigidBody[] Bodies { get; set; }
        public IExportJoint[] Joints { get; set; }
        public IExportThruster[] Thrusters { get; set; }
        public IExportRotaryMotor[] Motors { get; set; }
        public IExportAxialFriction[] AxialFrictions { get; set; }
        public bool[,] CollisionMatrix { get; set; }

        public PhysicSceneExportData() { }

        public PhysicSceneExportData(PhysicSceneExportData copy)
        {
            this.Bodies = copy.Bodies.Select(x => x.GetCopy()).ToArray();
            this.Joints = copy.Joints.Select(x => x.GetCopy()).ToArray();
            this.Thrusters = copy.Thrusters.Select(x => x.GetCopy()).ToArray();
            this.Motors = copy.Motors.Select(x => x.GetCopy()).ToArray();
            this.AxialFrictions = copy.AxialFrictions.Select(x => x.GetCopy()).ToArray();
            this.CollisionMatrix = copy.CollisionMatrix;
        }
    }
}
