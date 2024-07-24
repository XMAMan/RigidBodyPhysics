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

            if (distanceJoint.LimitIsEnabled)
            {
                this.MinLengthPosition = 0;
                this.MaxLengthPosition = 1;
            }
            else
            {
                this.MinLengthPosition = 0.0001f;
                this.MaxLengthPosition= 2.0f;
            }
        }
    }
}
