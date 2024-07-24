using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using RigidBodyPhysics.ExportData.RigidBody;

namespace PhysicSceneEditorControl.Controls.ShapeProperty
{
    public class ShapePropertyViewModel : ReactiveObject
    {
        [Reactive] public float VelocityX { get; set; }
        [Reactive] public float VelocityY { get; set; }
        [Reactive] public float AngularVelocity { get; set; } = 0;

        [Reactive] public MassData.MassType MassType1 { get; set; } = MassData.MassType.Density;
        [Reactive] public float Mass { get; set; } = 1;
        [Reactive] public float Density { get; set; } = 0.0001f;

        [Reactive] public float Friction { get; set; } = 0.03f;
        [Reactive] public float Restituion { get; set; } = 0.5f;

        [Reactive] public int CollisionCategory { get; set; } = 0;
    }
}
