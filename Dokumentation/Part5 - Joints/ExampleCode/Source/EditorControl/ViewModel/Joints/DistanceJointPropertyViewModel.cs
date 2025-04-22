using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using static PhysicEngine.Joints.IPublicJoint;

namespace EditorControl.ViewModel.Joints
{
    internal class DistanceJointPropertyViewModel : ReactiveObject
    {
        [Reactive] public bool CollideConnected { get; set; } = true;
        [Reactive] public float LengthPosition { get; set; } = 1;            //Die Sollwertlänge (0..1) 0=MinLength; 1=MaxLength
        [Reactive] public bool LimitIsEnabled { get; set; } = true;          //Wenn true, muss der Abstand innerhalb der MinLength/MaxLenght-Schranken liegen
        [Reactive] public float MinLength { get; set; } = 0;                 //Prozentzahl (0..1) MinLengthInPixel=MinLength * AnchorPointDistanceByStart
        [Reactive] public float MaxLength { get; set; } = 2;                 //Prozentzahl (0..1) MaxLengthInPixel=MaxLength * AnchorPointDistanceByStart


        [Reactive] public SoftPropertyViewModel Soft { get; set; } = new SoftPropertyViewModel();    //Damit wird die LengthPosition soft gemacht
    }
}
