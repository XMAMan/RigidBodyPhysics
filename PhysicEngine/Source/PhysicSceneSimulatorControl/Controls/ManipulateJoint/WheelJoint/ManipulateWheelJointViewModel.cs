using ReactiveUI;
using RigidBodyPhysics.RuntimeObjects.Joints;
using static RigidBodyPhysics.RuntimeObjects.Joints.IPublicJoint;

namespace PhysicSceneSimulatorControl.Controls.ManipulateJoint.WheelJoint
{
    internal class ManipulateWheelJointViewModel : ReactiveObject
    {
        private IPublicWheelJoint wheelJoint;

        public ManipulateWheelJointViewModel(IPublicWheelJoint wheelJoint)
        {
            this.wheelJoint = wheelJoint;

            if (wheelJoint.LimitIsEnabled)
            {
                this.MinMotorPosition = 0;
                this.MaxMotorPosition = 1;
            }
            else
            {
                this.MinMotorPosition = 0.0001f;
                this.MaxMotorPosition = 2.0f;
            }
        }

        public float MinMotorPosition { get; }
        public float MaxMotorPosition { get; }

        public TranslationMotor SelectedMotorType
        {
            get => this.wheelJoint.Motor;
            set => this.wheelJoint.Motor = value;
        }

        public IEnumerable<TranslationMotor> MotorTypeValues
        {
            get
            {
                return Enum.GetValues(typeof(TranslationMotor))
                    .Cast<TranslationMotor>();
            }
        }

        public float MotorSpeed
        {
            get => this.wheelJoint.MotorSpeed;
            set => this.wheelJoint.MotorSpeed = value;
        }

        public float MotorPosition
        {
            get => this.wheelJoint.MotorPosition;
            set => this.wheelJoint.MotorPosition = value;
        }

        public float MaxMotorForce
        {
            get => this.wheelJoint.MaxMotorForce;
            set => this.wheelJoint.MaxMotorForce = value;
        }
    }
}
