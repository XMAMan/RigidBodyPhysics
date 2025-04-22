using PhysicEngine.Joints;
using ReactiveUI;

namespace SimulatorControl.ViewModel.ManipulateJoint
{
    internal class ManipulateDistanceJointViewModel : ReactiveObject
    {
        private IPublicDistanceJoint distanceJoint;

        public float LengthPosition
        {
            get => this.distanceJoint.LengthPosition;
            set => this.distanceJoint.LengthPosition = value;
        }

        public float MinLengthPosition { get; }
        public float MaxLengthPosition { get; }

        public ManipulateDistanceJointViewModel(IPublicDistanceJoint distanceJoint)
        {
            this.distanceJoint = distanceJoint;

            this.MinLengthPosition = distanceJoint.MinLength;
            this.MaxLengthPosition = distanceJoint.MaxLength;
        }
    }
}
