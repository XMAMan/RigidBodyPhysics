using PhysicSceneEditorControl.Controls.SoftProperty;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using static RigidBodyPhysics.RuntimeObjects.Joints.IPublicJoint;

namespace PhysicSceneEditorControl.Controls.JointPropertys.RevoluteJoint
{
    internal class RevoluteJointPropertyViewModel : ReactiveObject
    {
        [Reactive] public bool CollideConnected { get; set; } = false;
        [Reactive] public bool LimitIsEnabled { get; set; } = true;
        [Reactive] public float LowerAngle { get; set; } = 160; //0..360   Minimal erlaubter Winkel zwischen den Hebelarmen
        [Reactive] public float UpperAngle { get; set; } = 200; //0..360 Maximal erlaubter Winkel zwischen den Hebelarmen

        [Reactive] public AngularMotor Motor { get; set; } = AngularMotor.GoToReferenceAngle;
        [Reactive] public float MotorSpeed { get; set; } = 0; //Mit der Winkelgeschwindigkeit wird das Gelenk bewegt
        [Reactive] public float MaxMotorTorque { get; set; } = 100f; //Maximale Kraft des Motors

        [Reactive] public bool BreakWhenMaxForceIsReached { get; set; } = false;
        [Reactive] public float MaxForceToBreak { get; set; } = 1;

        [Reactive] public SoftPropertyViewModel Soft { get; set; } = new SoftPropertyViewModel();    //Damit wird AngularMotor soft gemacht
    }
}
