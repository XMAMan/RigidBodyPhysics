using ReactiveUI;
using RigidBodyPhysics.RuntimeObjects.RotaryMotor;

namespace PhysicSceneSimulatorControl.Controls.ManipulateJoint.RotaryMotor
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

        public bool BrakeIsEnabled
        {
            get => this.motor.BrakeIsEnabled;
            set => this.motor.BrakeIsEnabled = value;
        }

        public ManipulateRotaryMotorViewModel(IPublicRotaryMotor motor)
        {
            this.motor = motor;
        }
    }
}
