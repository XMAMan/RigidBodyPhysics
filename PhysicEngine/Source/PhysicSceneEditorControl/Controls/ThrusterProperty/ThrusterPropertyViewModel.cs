using ReactiveUI.Fody.Helpers;
using ReactiveUI;

namespace PhysicSceneEditorControl.Controls.ThrusterProperty
{
    public class ThrusterPropertyViewModel : ReactiveObject
    {
        [Reactive] public float ForceLength { get; set; } = 0.001f;
        [Reactive] public bool IsEnabled { get; set; } = false;
    }
}
