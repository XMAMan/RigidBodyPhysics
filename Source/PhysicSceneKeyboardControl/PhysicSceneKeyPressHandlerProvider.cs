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
                        KeyDownState state = new KeyDownState();
                        handler.Add(new AnimatorKeyPressHandler(animator, "Animation Backward " + i, handler.Count + 1, false, state));
                        handler.Add(new AnimatorKeyPressHandler(animator, "Animation Forward " + i, handler.Count + 1, true, state));
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

    //Der Trick mit den Bits für den AnimatorKeyPressHandler ist nötigt, da die Animation sonst kurz stoppt, wenn man erst die 
    //Forward-Taste gedrückt hält, dann drückt man zusätzlich noch die Backward-Taste und erst danach lässt man die Forwardtaste wieder 
    //los. Dann stoppt die Animation anstatt von Forward direkt zu Backward über zu gehen.
    internal class KeyDownState
    {
        public int Data = 0; //Bit 0 = KeyDown-State für die Forward-Taste; Bit 1 = KeyDown-State für die Backward-Taste
    }

    internal class AnimatorKeyPressHandler : IKeyPressHandler
    {
        private Animator animator;
        private bool forward;
        private KeyDownState state;
        public AnimatorKeyPressHandler(Animator animator, string description, int id, bool forward, KeyDownState keyDownState)
        {
            this.animator = animator;
            this.KeyPressDescription = description;
            this.Id = id;
            this.forward = forward;
            this.state = keyDownState;
        }

        public int Id { get; }
        public string KeyPressDescription { get; private set; }
        public void HandleKeyDown()
        {
            if (forward)
            {
                this.state.Data |= 1; // Setze das Forward-Bit auf 1
            }
            else
            {
                this.state.Data |= 2; // Setze das Backward-Bit auf 1
            }

            ControlWithBitStateTheAnimator();
        }
        public void HandleKeyUp()
        {
            if (forward)
            {
                this.state.Data &= ~1; // Setze das Forward-Bit auf 0
            }
            else
            {
                this.state.Data &= ~2; // Setze das Backward-Bit auf 0
            }

            ControlWithBitStateTheAnimator();
        }

        private void ControlWithBitStateTheAnimator()
        {
            if (this.state.Data == 0) //Beide Bits sind 0 -> halte die Animation an
            {
                this.animator.PlayForward = false;
                this.animator.PlayBackwards = false;
            }
            else if (this.state.Data == 1) //Bit 0 hat den Wert 1 -> spiele vorwärts
            {
                this.animator.PlayForward = true;
            }
            else //Bit 1 hat den Wert 1 -> spiele rückwärts
            {
                this.animator.PlayBackwards = true;
            }
        }
    }
}
