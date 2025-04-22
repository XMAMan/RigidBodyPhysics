using PhysicSceneEditorControl.Controls.Editor.Model.EditorJoint;
using PhysicSceneEditorControl.Controls.Editor.Model.EditorShape;
using RigidBodyPhysics.MathHelper;

namespace PhysicSceneEditorControl.Controls.Editor.Model.Function.Joints.PrismaticJoint
{
    internal class AddPrismaticJointFunction : PlaceTwoAnchorPointsFunction, IFunction
    {
        public override IFunction Init(FunctionData functionData)
        {
            base.Init(functionData, true, true);
            return this;
        }

        protected override void CreateJoint(IEditorShape shape1, IEditorShape shape2, Vec2D r1, Vec2D r2)
        {
            joints.Add(new EditorPrismaticJoint(shape1, shape2, r1, r2));
        }
    }
}
