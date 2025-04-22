using EditorControl.Model.EditorJoint;
using EditorControl.Model.EditorShape;
using PhysicEngine.MathHelper;

namespace EditorControl.Model.Function.Joints.WeldJoint
{
    internal class AddWeldJointFunction : PlaceTwoAnchorPointsFunction, IFunction
    {
        public override IFunction Init(FunctionData functionData)
        {
            base.Init(functionData, false, false);
            return this;
        }

        protected override void CreateJoint(IEditorShape shape1, IEditorShape shape2, Vec2D r1, Vec2D r2)
        {
            joints.Add(new EditorWeldJoint(shape1, shape2, r1, r2));
        }
    }
}
