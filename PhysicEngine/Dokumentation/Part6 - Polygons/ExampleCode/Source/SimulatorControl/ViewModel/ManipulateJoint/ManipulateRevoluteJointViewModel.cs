using PhysicEngine.Joints;
using ReactiveUI;
using static PhysicEngine.Joints.IPublicJoint;

namespace SimulatorControl.ViewModel.ManipulateJoint
{
    internal class ManipulateRevoluteJointViewModel : ReactiveObject
    {
        private IPublicRevoluteJoint revoluteJoint;

        public ManipulateRevoluteJointViewModel(IPublicRevoluteJoint revoluteJoint)
        {
            this.revoluteJoint = revoluteJoint;
        }

        public AngularMotor SelectedMotorType
        {
            get => this.revoluteJoint.Motor;
            set => this.revoluteJoint.Motor = value;
        }

        public IEnumerable<AngularMotor> MotorTypeValues
        {
            get
            {
                return Enum.GetValues(typeof(AngularMotor))
                    .Cast<AngularMotor>();
            }
        }

        public float MotorSpeed
        {
            get => this.revoluteJoint.MotorSpeed;
            set => this.revoluteJoint.MotorSpeed = value;
        }

        public float MotorPosition
        {
            get => this.revoluteJoint.MotorPosition;
            set => this.revoluteJoint.MotorPosition = value;
        }

        public float MaxMotorTorque
        {
            get => this.revoluteJoint.MaxMotorTorque;
            set => this.revoluteJoint.MaxMotorTorque = value;
        }
    }
}
