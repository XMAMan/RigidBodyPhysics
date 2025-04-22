using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using static PhysicEngine.Joints.IPublicJoint;

namespace EditorControl.ViewModel.Joints
{
    internal class RevoluteJointPropertyViewModel : ReactiveObject
    {
        [Reactive] public bool CollideConnected { get; set; } = false;
        [Reactive] public bool LimitIsEnabled { get; set; } = true;
        [Reactive] public float LowerAngle { get; set; } = 160; //0..360   Minimal erlaubter Winkel zwischen den Hebelarmen
        [Reactive] public float UpperAngle { get; set; } = 200; //0..360 Maximal erlaubter Winkel zwischen den Hebelarmen

        [Reactive] public AngularMotor Motor { get; set; } = AngularMotor.GoToReferenceAngle;
        [Reactive] public float MotorSpeed { get; set; } = 0; //Mit der Winkelgeschwindigkeit wird das Gelenk bewegt
        [Reactive] public float MotorPosition { get; set; } = 0.5f; //Sollwinkel zwischen r1 und r2 (0..1)
        [Reactive] public float MaxMotorTorque { get; set; } = 100f; //Maximale Kraft des Motors

        [Reactive] public SoftPropertyViewModel Soft { get; set; } = new SoftPropertyViewModel();    //Damit wird AngularMotor soft gemacht
    }
}
