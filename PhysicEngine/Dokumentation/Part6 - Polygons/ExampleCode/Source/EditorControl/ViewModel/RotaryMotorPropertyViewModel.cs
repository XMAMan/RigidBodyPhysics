using ReactiveUI.Fody.Helpers;
using ReactiveUI;

namespace EditorControl.ViewModel
{
    public class RotaryMotorPropertyViewModel : ReactiveObject
    {
        [Reactive] public float RotaryForce { get; set; } = 1f;
        [Reactive] public bool IsEnabled { get; set; } = false;
    }
}
