using ReactiveUI.Fody.Helpers;

namespace EditorControl.ViewModel
{
    public class RectanglePropertyViewModel : ShapePropertyViewModel
    {
        [Reactive] public bool BreakWhenMaxPushPullForceIsReached { get; set; } = false;
        [Reactive] public float MaxPushPullForce { get; set; } = 1;
    }
}
