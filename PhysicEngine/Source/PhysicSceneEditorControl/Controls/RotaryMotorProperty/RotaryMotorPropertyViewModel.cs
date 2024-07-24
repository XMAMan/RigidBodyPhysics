using ReactiveUI.Fody.Helpers;
using ReactiveUI;

namespace PhysicSceneEditorControl.Controls.RotaryMotorProperty
{
    public class RotaryMotorPropertyViewModel : ReactiveObject
    {
        [Reactive] public float RotaryForce { get; set; } = 1f;
        [Reactive] public float MaxSpeed { get; set; } = 1f;
        [Reactive] public bool IsEnabled { get; set; } = false;
        [Reactive] public bool BrakeIsEnabled { get; set; } = false;
    }
}
