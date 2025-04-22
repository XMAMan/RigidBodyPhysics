using EditorControl.Model.EditorJoint;
using EditorControl.Model.EditorShape;
using PhysicEngine.MathHelper;

namespace EditorControl.Model.Function.Joints.WeldJoint
{
    internal class AddWeldJointFunction : PlaceOneAnchorPointFunction, IFunction
    {
        public override IFunction Init(List<IEditorShape> shapes, List<IEditorJoint> joints)
        {
            base.Init1(shapes, joints);
            return this;
        }

        protected override void CreateJoint(IEditorShape shape1, IEditorShape shape2, Vec2D anchorWorldPosition)
        {
            joints.Add(new EditorWeldJoint(shape1, shape2, anchorWorldPosition));
        }
    }
}
