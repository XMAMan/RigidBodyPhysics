using PhysicEngine.Joints;
using ReactiveUI;

namespace SimulatorControl.ViewModel.ManipulateJoint
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
