using ReactiveUI.Fody.Helpers;
using ReactiveUI;

namespace EditorControl.ViewModel.Joints
{
    internal class DistanceJointPropertyViewModel : ReactiveObject
    {
        [Reactive] public bool CollideConnected { get; set; } = true;
        [Reactive] public bool LimitIsEnabled { get; set; } = true;          //Wenn true, muss der Abstand innerhalb der MinLength/MaxLenght-Schranken liegen
        [Reactive] public bool JointIsRope { get; set; } = false;        
        [Reactive] public float MinLength { get; set; } = 0;                 //Minimallänge in Pixeln
        [Reactive] public float MaxLength { get; set; } = 100;               //Maximallänge in Pixeln

        [Reactive] public bool BreakWhenMaxForceIsReached { get; set; } = false;
        [Reactive] public float MaxForceToBreak { get; set; } = 1;

        [Reactive] public SoftPropertyViewModel Soft { get; set; } = new SoftPropertyViewModel();    //Damit wird die LengthPosition soft gemacht
    }
}
