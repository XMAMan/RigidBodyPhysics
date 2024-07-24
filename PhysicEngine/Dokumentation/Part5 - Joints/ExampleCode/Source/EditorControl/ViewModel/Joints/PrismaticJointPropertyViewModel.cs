using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using static PhysicEngine.Joints.IPublicJoint;

namespace EditorControl.ViewModel.Joints
{
    internal class PrismaticJointPropertyViewModel : ReactiveObject
    {
        [Reactive] public bool CollideConnected { get; set; } = false;
        [Reactive] public bool LimitIsEnabled { get; set; } = true;  //Wenn true, muss der Abstand innerhalb der MinLength/MaxLenght-Schranken liegen
        [Reactive] public float MinTranslation { get; set; } = 0;         //Prozentzahl (0..1) MinLengthInPixel=MinLength * AnchorPointDistanceByStart
        [Reactive] public float MaxTranslation { get; set; } = 2;         //Prozentzahl (0..1) MaxLengthInPixel=MaxLength * AnchorPointDistanceByStart

        [Reactive] public TranslationMotor Motor { get; set; } = TranslationMotor.GoToReferencePosition;
        [Reactive] public float MotorSpeed { get; set; } = 0; //Mit der Geschwindigkeit bewegen sich die Körper aufeinander zu/weg
        [Reactive] public float MotorPosition { get; set; } = 0.5f; //Sollabstand der Ankerpunkte. Geht von MinLength bis MaxLength
        [Reactive] public float MaxMotorForce { get; set; } = 100; //Maximale Kraft des Motors

        [Reactive] public SoftPropertyViewModel Soft { get; set; } = new SoftPropertyViewModel();    //Damit wird TranslationMotor soft gemacht
    }
}
