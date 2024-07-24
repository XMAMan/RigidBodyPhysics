using GameHelper.Simulation;
using GraphicPanels;
using ReactiveUI;
using System;
using RigidBodyPhysics.RuntimeObjects.AxialFriction;
using RigidBodyPhysics.RuntimeObjects.Joints;
using RigidBodyPhysics.RuntimeObjects.Thruster;
using System.Windows.Input;

namespace CarDrifterControl.Model
{
    internal class Car
    {
        private float wheelfriction = 0.0002f;    //Reibung zwischen Reifen und Straße
        private float motorPower = 0.0002f;       //Motorkraft
        private float steerRange = 0.1f;  //So stark lenkt das Auto 0.05f;

        private IPublicThruster thrusterBackLeft, thrusterBackRight, thrusterFrontLeft, thrusterFrontRight;
        private IPublicRevoluteJoint revoluteLeft, revoluteRight;
        private IPublicAxialFriction brakeFrontLeft, brakeFrontRight, brakeBackLeft, brakeBackRight;
        private IPublicAxialFriction[] axialFriction; //Seitliche Reibungskraft von allen 4 Rädern

        private bool leftIsDown, rightIsDown, upIsDown, downIsDown;
        private bool leftCtrlIsDown = false;

        private CarIsSlidingTracker slidingTracker;

        public Car(GameSimulator simulator, Sounds sounds) 
        {
            this.thrusterBackLeft = simulator.GetThrusterByTagName("thrusterBackLeft");
            this.thrusterBackRight = simulator.GetThrusterByTagName("thrusterBackRight");
            this.thrusterFrontLeft = simulator.GetThrusterByTagName("thrusterFrontLeft");
            this.thrusterFrontRight = simulator.GetThrusterByTagName("thrusterFrontRight");
            this.revoluteLeft = simulator.GetJointByTagName<IPublicRevoluteJoint>("revoluteLeft");
            this.revoluteRight = simulator.GetJointByTagName<IPublicRevoluteJoint>("revoluteRight");
            this.brakeFrontLeft = simulator.GetAxialFrictionByTagName("brakeFrontLeft");
            this.brakeFrontRight = simulator.GetAxialFrictionByTagName("brakeFrontRight");
            this.brakeBackLeft = simulator.GetAxialFrictionByTagName("brakeBackLeft");
            this.brakeBackRight = simulator.GetAxialFrictionByTagName("brakeBackRight");

            this.axialFriction = new IPublicAxialFriction[4];
            this.axialFriction[0] = simulator.GetAxialFrictionByTagName("axialFriction1");
            this.axialFriction[1] = simulator.GetAxialFrictionByTagName("axialFriction2");
            this.axialFriction[2] = simulator.GetAxialFrictionByTagName("axialFriction3");
            this.axialFriction[3] = simulator.GetAxialFrictionByTagName("axialFriction4");

            for (int i=0;i<axialFriction.Length;i++)
            {
                this.axialFriction[i].Friction = wheelfriction;
            }

            this.slidingTracker = new CarIsSlidingTracker(this.axialFriction);
            this.slidingTracker.WhenAnyValue(x => x.IsSliding).Subscribe(isSliding =>
            {
                if (isSliding)
                    sounds.StartBrake();
                else
                    sounds.StopBrake();
            });
        }

        public void HandleKeyDown(System.Windows.Input.Key key)
        {
            switch (key)
            {
                case Key.Up:
                    this.upIsDown = true;
                    //Vorderradantrieb
                    //this.thrusterFrontLeft.ForceLength = motorPower;
                    //this.thrusterFrontRight.ForceLength = motorPower;
                    //this.thrusterFrontLeft.IsEnabled = this.thrusterFrontRight.IsEnabled = true;

                    //Hinterradantrieb
                    this.thrusterBackLeft.ForceLength = motorPower;
                    this.thrusterBackRight.ForceLength = motorPower;
                    this.thrusterBackLeft.IsEnabled = this.thrusterBackRight.IsEnabled = true;
                    break;         
                    
                case Key.Down:
                    this.downIsDown = true;
                    //Vorderradantrieb
                    //this.thrusterFrontLeft.ForceLength = -motorPower;
                    //this.thrusterFrontRight.ForceLength = -motorPower;
                    //this.thrusterFrontLeft.IsEnabled = this.thrusterFrontRight.IsEnabled = true;

                    //Hinterradantrieb
                    this.thrusterBackLeft.ForceLength = -motorPower;
                    this.thrusterBackRight.ForceLength = -motorPower;
                    this.thrusterBackLeft.IsEnabled = this.thrusterBackRight.IsEnabled = true;
                    break;

                case Key.Left:
                    this.leftIsDown = true;
                    this.revoluteLeft.MotorPosition = steerRange;
                    this.revoluteRight.MotorPosition = steerRange;
                    break;

                case Key.Right:
                    this.rightIsDown = true;
                    this.revoluteLeft.MotorPosition = -steerRange;
                    this.revoluteRight.MotorPosition = -steerRange;
                    break;

                case Key.LeftCtrl:
                    this.leftCtrlIsDown = true;

                    //Aktiviere die Bremse
                    this.brakeFrontLeft.Friction = wheelfriction;
                    this.brakeFrontRight.Friction = wheelfriction;

                    SetFrictionFromBackWheels(0.2f);
                    break;
            }
        }

        public void HandleKeyUp(System.Windows.Input.Key key)
        {
            switch (key)
            {
                case Key.Up:
                    upIsDown = false;
                    DisableMotor();
                    break;
                case Key.Down:
                    downIsDown = false;
                    DisableMotor();
                    break;

                case Key.Left:
                    leftIsDown = false;
                    DisableSteering();
                    break;
                case Key.Right:
                    rightIsDown = false;
                    DisableSteering();
                    break;

                case Key.LeftCtrl:
                    this.leftCtrlIsDown = false;

                    this.brakeFrontLeft.Friction = 0;
                    this.brakeFrontRight.Friction = 0;

                    SetFrictionFromBackWheels(1);

                    break;
            }
        }

        private void DisableSteering()
        {
            if (leftIsDown == false && rightIsDown == false)
            {
                this.revoluteLeft.MotorPosition = 0;
                this.revoluteRight.MotorPosition = 0;
            }
        }

        private void DisableMotor()
        {
            if (upIsDown == false && downIsDown == false)
            {
                //this.thrusterFrontLeft.IsEnabled = this.thrusterFrontRight.IsEnabled = false;
                this.thrusterBackLeft.IsEnabled = this.thrusterBackRight.IsEnabled = false;
            }
        }

        public void MoveOneStep(float dt)
        {
            this.slidingTracker.StoreTrackingPoint();
        }

        private void SetFrictionFromBackWheels(float f)
        {
            for (int i = 2; i < axialFriction.Length; i++)
            {
                this.axialFriction[i].Friction = wheelfriction * f;
            }
        }

        public void Draw(GraphicPanel2D panel)
        {
            this.slidingTracker.Draw(panel);
        }
    }
}
