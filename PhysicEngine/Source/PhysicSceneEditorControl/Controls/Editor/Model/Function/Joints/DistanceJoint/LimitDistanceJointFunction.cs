using PhysicSceneEditorControl.Controls.Editor.Model.EditorJoint;

namespace PhysicSceneEditorControl.Controls.Editor.Model.Function.Joints.DistanceJoint
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
