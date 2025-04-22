using PhysicSceneEditorControl.Controls.Editor.Model.EditorJoint;

namespace PhysicSceneEditorControl.Controls.Editor.Model.Function.Joints.WheelJoint
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
