using PhysicSceneEditorControl.Controls.ShapeProperty;
using ReactiveUI.Fody.Helpers;

namespace PhysicSceneEditorControl.Controls.RectangleProperty
{
    public class RectanglePropertyViewModel : ShapePropertyViewModel
    {
        [Reactive] public bool BreakWhenMaxPushPullForceIsReached { get; set; } = false;
        [Reactive] public float MaxPushPullForce { get; set; } = 1;
    }
}
