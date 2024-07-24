namespace PhysicSceneSimulatorControl.Dialogs.PrintSettings
{
    public class PrintSettingsViewModel
    {
        public bool ShowCollisionPoints { get; set; } = false;
        public bool ShowJoints { get; set; } = true;
        public bool ShowThrusters { get; set; } = true;
        public bool ShowRotaryMotors { get; set; } = true;
        public bool ShowAxialFrictions { get; set; } = true;
        public bool ShowSubPolys { get; set; } = false;

        public bool ShowNoBodyText { get; set; } = true;
        public bool ShowPushPullForce { get; set; } = false;
        public bool ShowBodyIndex { get; set; } = false;
        public bool ShowOrientation { get; set; } = false;
        public bool ShowNoJointText { get; set; } = true;
        public bool ShowJointPosition { get; set; } = false;


        public bool VisualizePushPullForce { get; set; } = false;
        public float MaxPushPullForce { get; set; } = 0.02f;
    }
}
