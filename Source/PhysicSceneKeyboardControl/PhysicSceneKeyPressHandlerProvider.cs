using KeyFrameGlobal;
using RigidBodyPhysics.ExportData;
using RigidBodyPhysics.RuntimeObjects.Joints;
using RigidBodyPhysics.RuntimeObjects.RotaryMotor;
using RigidBodyPhysics.RuntimeObjects.Thruster;

namespace PhysicSceneKeyboardControl
{
    public static class PhysicSceneKeyPressHandlerProvider
    {
        public static IKeyPressHandler[] GetHandler(PhysicScenePublicData physicObjects, Animator[] animators)
        {
            List<IKeyPressHandler> handler = new List<IKeyPressHandler>();

            //Möglichkeit 1: Steuere Gelenke
            var joints = physicObjects.Joints;
            for (int i = 0; i < joints.Length; i++)
            {
                var joint = joints[i];

                if (joint is IPublicRevoluteJoint)
                {
                    var revolute = (IPublicRevoluteJoint)joint;
                    if (revolute.LimitIsEnabled == false)
                    {
                        handler.Add(new RevoluteJointKeyPressHandler(revolute, "Revolute " + i, handler.Count + 1));
                    }
                }
            }
            //Möglichkeit 2: Steuere Schubdüsen
            var thrusters = physicObjects.Thrusters;
            for (int i = 0; i < thrusters.Length; i++)
            {
                var thruster = thrusters[i];
                handler.Add(new ThrusterKeyPressHandler(thruster, "Thruster " + i, handler.Count + 1));
            }

            //Möglichkeit 3: Steuere Rotations-Motoren
            var motors = physicObjects.Motors;
            for (int i = 0; i < motors.Length; i++)
            {
                var motor = motors[i];
                handler.Add(new RotaryMotorKeyPressHandler(motor, "Motor " + i, handler.Count + 1));
            }

            //Möglichkeit 4: Steuere manuelle Animation
            if (animators != null)
            {
                for (int i = 0; i < animators.Length; i++)
                {
                    var animator = animators[i];
                    if (animator.Type == AnimationOutputData.AnimationType.Manually)
                    {
                        handler.Add(new AnimatorKeyPressHandler(animator, "Animation Backward " + i, handler.Count + 1, false));
                        handler.Add(new AnimatorKeyPressHandler(animator, "Animation Forward " + i, handler.Count + 1, true));
                    }
                }
            }


            return handler.ToArray();
        }
    }

    internal class RevoluteJointKeyPressHandler : IKeyPressHandler
    {
        public IPublicRevoluteJoint RevoluteJoint { get; }
        public RevoluteJointKeyPressHandler(IPublicRevoluteJoint joint, string description, int id)
        {
            this.RevoluteJoint = joint;
            this.KeyPressDescription = description;
            Id = id;
        }

        public int Id { get; }
        public string KeyPressDescription { get; private set; }
        public void HandleKeyDown()
        {
            this.RevoluteJoint.Motor = IPublicJoint.AngularMotor.SpinAround;
        }
        public void HandleKeyUp()
        {
            this.RevoluteJoint.Motor = IPublicJoint.AngularMotor.Disabled;
        }
    }

    internal class ThrusterKeyPressHandler : IKeyPressHandler
    {
        public IPublicThruster Thruster { get; }
        public ThrusterKeyPressHandler(IPublicThruster thruster, string description, int id)
        {
            this.Thruster = thruster;
            this.KeyPressDescription = description;
            Id = id;
        }

        public int Id { get; }
        public string KeyPressDescription { get; private set; }
        public void HandleKeyDown()
        {
            this.Thruster.IsEnabled = true;
        }
        public void HandleKeyUp()
        {
            this.Thruster.IsEnabled = false;
        }
    }

    internal class RotaryMotorKeyPressHandler : IKeyPressHandler
    {
        public IPublicRotaryMotor Motor { get; }
        public RotaryMotorKeyPressHandler(IPublicRotaryMotor motor, string description, int id)
        {
            this.Motor = motor;
            this.KeyPressDescription = description;
            Id = id;
        }

        public int Id { get; }
        public string KeyPressDescription { get; private set; }
        public void HandleKeyDown()
        {
            this.Motor.IsEnabled = true;
        }
        public void HandleKeyUp()
        {
            this.Motor.IsEnabled = false;
        }
    }

    internal class AnimatorKeyPressHandler : IKeyPressHandler
    {
        private Animator animator;
        private bool forward;
        public AnimatorKeyPressHandler(Animator animator, string description, int id, bool forward)
        {
            this.animator = animator;
            this.KeyPressDescription = description;
            Id = id;
            this.forward = forward;
        }

        public int Id { get; }
        public string KeyPressDescription { get; private set; }
        public void HandleKeyDown()
        {
            if (forward)
            {
                this.animator.PlayForward = true;
            }
            else
            {
                this.animator.PlayBackwards = true;
            }

        }
        public void HandleKeyUp()
        {
            if (forward)
            {
                this.animator.PlayForward = false;
            }
            else
            {
                this.animator.PlayBackwards = false;
            }
        }
    }
}
