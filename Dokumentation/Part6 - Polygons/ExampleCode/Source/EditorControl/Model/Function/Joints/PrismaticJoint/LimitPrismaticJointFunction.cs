using EditorControl.Model.EditorJoint;

namespace EditorControl.Model.Function.Joints.PrismaticJoint
{
    internal class LimitPrismaticJointFunction : LimitJointWithLinearMinMax, IFunction
    {
        public IFunction Init(EditorPrismaticJoint selectedJoint, Action selectOtherJoint)
        {
            base.Init(selectedJoint, selectOtherJoint, "Edit Prismatic Joint");
            return this;
        }
    }
}
