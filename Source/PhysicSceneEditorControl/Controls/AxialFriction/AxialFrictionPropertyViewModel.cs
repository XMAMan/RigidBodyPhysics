using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace PhysicSceneEditorControl.Controls.AxialFriction
{
    public class AxialFrictionPropertyViewModel : ReactiveObject
    {
        [Reactive] public float Friction { get; set; } = 1;
    }
}
