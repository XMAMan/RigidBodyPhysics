using EditorControl.Model.EditorJoint;

namespace EditorControl.Model.Function.Joints.DistanceJoint
{
    internal class LimitDistanceJointFunction : LimitJointWithLinearMinMax, IFunction
    {
        public IFunction Init(EditorDistanceJoint selectedJoint, Action selectOtherJoint)
        {
            base.Init(selectedJoint, selectOtherJoint, "Edit Distance Joint");
            return this;
        }
    }
}
