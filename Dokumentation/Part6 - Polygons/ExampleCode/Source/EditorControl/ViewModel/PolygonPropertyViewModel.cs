using PhysicEngine.RigidBody;
using ReactiveUI.Fody.Helpers;

namespace EditorControl.ViewModel
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
