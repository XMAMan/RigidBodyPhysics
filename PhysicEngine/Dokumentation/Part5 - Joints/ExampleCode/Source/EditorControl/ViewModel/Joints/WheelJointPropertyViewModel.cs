using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using static PhysicEngine.Joints.IPublicJoint;

namespace EditorControl.ViewModel.Joints
{
    internal class WheelJointPropertyViewModel : ReactiveObject
    {
        [Reactive] public bool CollideConnected { get; set; } = false;
        [Reactive] public bool LimitIsEnabled { get; set; } = true;
        [Reactive] public float MinTranslation { get; set; } = 0;    //Prozentzahl (0..1) MinTranslationInPixel=MinTranslation * AnchorPointDistanceByStart
        [Reactive] public float MaxTranslation { get; set; } = 2;    //Prozentzahl (0..1)MaxTranslationInPixel=MaxTranslation * AnchorPointDistanceByStart

        [Reactive] public TranslationMotor Motor { get; set; } = TranslationMotor.Disabled;
        [Reactive] public float MotorSpeed { get; set; } = 0; //Mit der Geschwindigkeit bewegen sich die Körper aufeinander zu/weg
        [Reactive] public float MotorPosition { get; set; } = 0; //Sollabstand der Ankerpunkte. Geht von MinLength bis MaxLength
        [Reactive] public float MaxMotorForce { get; set; } = 100; //Maximale Kraft des Motors

        [Reactive] public SoftPropertyViewModel Soft { get; set; } = new SoftPropertyViewModel();    //Damit wird TranslationMotor soft gemacht
    }
}
