using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace EditorControl.ViewModel
{
    public class ThrusterPropertyViewModel : ReactiveObject
    {
        [Reactive] public float ForceLength { get; set; } = 0.001f;
        [Reactive] public bool IsEnabled { get; set; } = false;
    }
}
