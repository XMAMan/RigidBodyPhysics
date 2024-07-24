using ReactiveUI;
using RigidBodyPhysics.RuntimeObjects.Joints;
using static RigidBodyPhysics.RuntimeObjects.Joints.IPublicJoint;

namespace PhysicSceneSimulatorControl.Controls.ManipulateJoint.PrismaticJoint
{
    internal class ManipulatePrismaticJointViewModel : ReactiveObject
    {
        private IPublicPrismaticJoint prismaticJoint;

        public ManipulatePrismaticJointViewModel(IPublicPrismaticJoint prismaticJoint)
        {
            this.prismaticJoint = prismaticJoint;

            if (this.prismaticJoint.LimitIsEnabled)
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
            get => this.prismaticJoint.Motor;
            set => this.prismaticJoint.Motor = value;
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
            get => this.prismaticJoint.MotorSpeed;
            set => this.prismaticJoint.MotorSpeed = value;
        }

        public float MotorPosition
        {
            get => this.prismaticJoint.MotorPosition;
            set => this.prismaticJoint.MotorPosition = value;
        }

        public float MaxMotorForce
        {
            get => this.prismaticJoint.MaxMotorForce;
            set => this.prismaticJoint.MaxMotorForce = value;
        }
    }
}
