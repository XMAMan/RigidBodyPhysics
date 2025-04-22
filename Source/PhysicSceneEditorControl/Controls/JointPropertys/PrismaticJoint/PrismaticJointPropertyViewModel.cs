using PhysicSceneEditorControl.Controls.SoftProperty;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using static RigidBodyPhysics.RuntimeObjects.Joints.IPublicJoint;

namespace PhysicSceneEditorControl.Controls.JointPropertys.PrismaticJoint
{
    internal class PrismaticJointPropertyViewModel : ReactiveObject
    {
        [Reactive] public bool CollideConnected { get; set; } = false;
        [Reactive] public bool LimitIsEnabled { get; set; } = true;  //Wenn true, muss der Abstand innerhalb der MinLength/MaxLenght-Schranken liegen
        [Reactive] public float MinTranslation { get; set; } = 0;         //Faktor für R1-Length (float.Min..float.Max) MinLengthInPixel=MinLength * R1.Length
        [Reactive] public float MaxTranslation { get; set; } = 2;         //Faktor für R1-Length (float.Min..float.Max) MaxLengthInPixel=MinLength * R1.Length

        [Reactive] public TranslationMotor Motor { get; set; } = TranslationMotor.GoToReferencePosition;
        [Reactive] public float MotorSpeed { get; set; } = 0; //Mit der Geschwindigkeit bewegen sich die Körper aufeinander zu/weg
        [Reactive] public float MaxMotorForce { get; set; } = 100; //Maximale Kraft des Motors

        [Reactive] public bool BreakWhenMaxForceIsReached { get; set; } = false;
        [Reactive] public float MaxForceToBreak { get; set; } = 1;

        [Reactive] public SoftPropertyViewModel Soft { get; set; } = new SoftPropertyViewModel();    //Damit wird TranslationMotor soft gemacht
    }
}
