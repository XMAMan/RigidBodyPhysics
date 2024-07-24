using ReactiveUI;
using RigidBodyPhysics.RuntimeObjects.Joints;

namespace PhysicSceneSimulatorControl.Controls.ManipulateJoint.WeldJoint
{
    internal class ManipulateWeldJointViewModel : ReactiveObject
    {
        private IPublicWeldJoint weldJoint;

        public float Stiffness
        {
            get => this.weldJoint.Stiffness;
            set => this.weldJoint.Stiffness = value;
        }

        public float Damping
        {
            get => this.weldJoint.Damping;
            set => this.weldJoint.Damping = value;
        }

        public ManipulateWeldJointViewModel(IPublicWeldJoint distanceJoint)
        {
            this.weldJoint = distanceJoint;
        }
    }
}
