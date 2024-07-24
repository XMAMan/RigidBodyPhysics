using EditorControl.Model.EditorJoint;

namespace EditorControl.Model.Function.Joints.WheelJoint
{
    internal class LimitWheelJointFunction : LimitJointWithLinearMinMax, IFunction
    {
        public IFunction Init(EditorWheelJoint selectedJoint, Action selectOtherJoint)
        {
            base.Init(selectedJoint, selectOtherJoint, "Edit Wheel Joint");
            return this;
        }
    }
}
