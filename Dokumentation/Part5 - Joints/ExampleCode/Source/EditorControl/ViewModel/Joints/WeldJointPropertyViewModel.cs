using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace EditorControl.ViewModel.Joints
{
    internal class WeldJointPropertyViewModel : ReactiveObject
    {
        [Reactive] public bool CollideConnected { get; set; } = false;

        [Reactive] public SoftPropertyViewModel Soft { get; set; } = new SoftPropertyViewModel();    //Damit wird FixAngular soft gemacht

    }
}
