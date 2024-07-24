using ReactiveUI;
using RigidBodyPhysics.RuntimeObjects.Thruster;

namespace PhysicSceneSimulatorControl.Controls.ManipulateJoint.Thruster
{
    internal class ManipulateThrusterViewModel : ReactiveObject
    {
        private IPublicThruster thruster;

        public float Force
        {
            get => this.thruster.ForceLength;
            set => this.thruster.ForceLength = value;
        }

        public bool IsEnabled
        {
            get => this.thruster.IsEnabled;
            set => this.thruster.IsEnabled = value;
        }

        public ManipulateThrusterViewModel(IPublicThruster thruster)
        {
            this.thruster = thruster;
        }
    }
}
