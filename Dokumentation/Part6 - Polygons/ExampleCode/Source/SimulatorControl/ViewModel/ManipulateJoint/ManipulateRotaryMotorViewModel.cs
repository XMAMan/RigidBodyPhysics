using PhysicEngine.RotaryMotor;
using ReactiveUI;

namespace SimulatorControl.ViewModel.ManipulateJoint
{
    internal class ManipulateRotaryMotorViewModel : ReactiveObject
    {
        private IPublicRotaryMotor motor;

        public float RotaryForce
        {
            get => this.motor.RotaryForce;
            set => this.motor.RotaryForce = value;
        }

        public bool IsEnabled
        {
            get => this.motor.IsEnabled;
            set => this.motor.IsEnabled = value;
        }

        public ManipulateRotaryMotorViewModel(IPublicRotaryMotor motor)
        {
            this.motor = motor;
        }
    }
}
