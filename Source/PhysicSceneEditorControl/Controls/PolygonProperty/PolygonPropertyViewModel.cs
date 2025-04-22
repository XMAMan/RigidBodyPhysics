using PhysicSceneEditorControl.Controls.ShapeProperty;
using ReactiveUI.Fody.Helpers;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace PhysicSceneEditorControl.Controls.PolygonProperty
{
    public class PolygonPropertyViewModel : ShapePropertyViewModel
    {
        [Reactive] public PolygonCollisionType PolygonType { get; set; } = PolygonCollisionType.Rigid;

        public IEnumerable<PolygonCollisionType> PolygonTypeValues
        {
            get
            {
                return Enum.GetValues(typeof(PolygonCollisionType))
                    .Cast<PolygonCollisionType>();
            }
        }
    }
}
