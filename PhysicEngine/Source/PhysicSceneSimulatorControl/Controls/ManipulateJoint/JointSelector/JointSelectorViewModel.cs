﻿using PhysicSceneSimulatorControl.Controls.ManipulateJoint.DistanceJoint;
using PhysicSceneSimulatorControl.Controls.ManipulateJoint.PrismaticJoint;
using PhysicSceneSimulatorControl.Controls.ManipulateJoint.RevoluteJoint;
using PhysicSceneSimulatorControl.Controls.ManipulateJoint.RotaryMotor;
using PhysicSceneSimulatorControl.Controls.ManipulateJoint.Thruster;
using PhysicSceneSimulatorControl.Controls.ManipulateJoint.WeldJoint;
using PhysicSceneSimulatorControl.Controls.ManipulateJoint.WheelJoint;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using RigidBodyPhysics.RuntimeObjects.Thruster;
using RigidBodyPhysics.RuntimeObjects.RotaryMotor;
using RigidBodyPhysics.RuntimeObjects.Joints;

namespace PhysicSceneSimulatorControl.Controls.ManipulateJoint.JointSelector
{
    internal class JointSelectorViewModel : ReactiveObject
    {
        private IPublicJoint[] joints;
        private IPublicThruster[] thrusters;
        private IPublicRotaryMotor[] motors;

        private string selectedJoint = "";
        public string SelectedJointName
        {
            get => this.selectedJoint;
            set
            {
                this.selectedJoint = value;
                int index = this.JointNames.ToList().IndexOf(value);
                if (index != -1)
                {
                    if (index < joints.Length)
                        ChangeSelectedJoint(joints[index]);
                    else if (index < joints.Length + thrusters.Length)
                        ChangeSelectedThruster(thrusters[index - joints.Length]);
                    else
                        ChangeSelectedRotaryMotor(motors[index - joints.Length - thrusters.Length]);
                }
            }
        }

        public IEnumerable<string> JointNames { get; set; }

        [Reactive] public System.Windows.Controls.UserControl ContentUserControl { get; set; }

        public JointSelectorViewModel(IPublicJoint[] joints, IPublicThruster[] thrusters, IPublicRotaryMotor[] motors)
        {
            this.joints = joints;
            this.thrusters = thrusters;
            this.motors = motors;

            List<string> names = new List<string>();
            names.AddRange(joints.Select((x, index) => index + " " + x.GetType().Name).ToList());
            names.AddRange(thrusters.Select((x, index) => index + " " + x.GetType().Name).ToList());
            names.AddRange(motors.Select((x, index) => index + " " + x.GetType().Name).ToList());

            this.JointNames = names;
        }

        private void ChangeSelectedJoint(IPublicJoint joint)
        {
            if (joint is IPublicDistanceJoint)
            {
                this.ContentUserControl = new ManipulateDistanceJointControl() { DataContext = new ManipulateDistanceJointViewModel(joint as IPublicDistanceJoint) };
            }

            if (joint is IPublicRevoluteJoint)
            {
                this.ContentUserControl = new ManipulateRevoluteJointControl() { DataContext = new ManipulateRevoluteJointViewModel(joint as IPublicRevoluteJoint) };
            }

            if (joint is IPublicPrismaticJoint)
            {
                this.ContentUserControl = new ManipulatePrismaticJointControl() { DataContext = new ManipulatePrismaticJointViewModel(joint as IPublicPrismaticJoint) };
            }

            if (joint is IPublicWeldJoint)
            {
                this.ContentUserControl = new ManipulateWeldJointControl() { DataContext = new ManipulateWeldJointViewModel(joint as IPublicWeldJoint) };
            }

            if (joint is IPublicWheelJoint)
            {
                this.ContentUserControl = new ManipulateWheelJointControl() { DataContext = new ManipulateWheelJointViewModel(joint as IPublicWheelJoint) };
            }
        }

        private void ChangeSelectedThruster(IPublicThruster thruster)
        {
            this.ContentUserControl = new ManipulateThrusterControl() { DataContext = new ManipulateThrusterViewModel(thruster) };
        }

        private void ChangeSelectedRotaryMotor(IPublicRotaryMotor motor)
        {
            this.ContentUserControl = new ManipulateRotaryMotorControl() { DataContext = new ManipulateRotaryMotorViewModel(motor) };
        }
    }
}