using PhysicEngine;
using PhysicEngine.Joints;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SimulatorControl.View.ManipulateJoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulatorControl.ViewModel.ManipulateJoint
{
    internal class ManipulateJointViewModel : ReactiveObject
    {
        private IPublicJoint[] joints;

        private string selectedJoint = "";
        public string SelectedJointName 
        {
            get => this.selectedJoint;
            set
            {
                this.selectedJoint = value;
                int index = this.JointNames.ToList().IndexOf(value);
                if (index != -1) ChangeSelectedJoint(joints[index]);
            }
        }

        public IEnumerable<string> JointNames { get; set; }

        [Reactive] public System.Windows.Controls.UserControl ContentUserControl { get; set; }

        public ManipulateJointViewModel(IPublicJoint[] joints)
        {
            this.joints = joints;
            this.JointNames = joints.Select((x, index) => index + " " + x.GetType().Name).ToList();
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
    }
}
