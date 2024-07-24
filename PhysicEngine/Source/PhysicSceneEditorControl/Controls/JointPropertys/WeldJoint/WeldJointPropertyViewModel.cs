using PhysicSceneEditorControl.Controls.SoftProperty;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;

namespace PhysicSceneEditorControl.Controls.JointPropertys.WeldJoint
{
    internal class WeldJointPropertyViewModel : ReactiveObject
    {
        [Reactive] public bool CollideConnected { get; set; } = false;
        [Reactive] public bool BreakWhenMaxForceIsReached { get; set; } = false;
        [Reactive] public float MaxForceToBreak { get; set; } = 1;

        [Reactive] public SoftPropertyViewModel Soft { get; set; } = new SoftPropertyViewModel();    //Damit wird FixAngular soft gemacht

    }
}
