﻿using EditorControl.Model.EditorJoint;
using EditorControl.Model.EditorShape;
using PhysicEngine.MathHelper;

namespace EditorControl.Model.Function.Joints.WheelJoint
{
    internal class AddWheelJointFunction : PlaceTwoAnchorPointsFunction, IFunction
    {
        public override IFunction Init(List<IEditorShape> shapes, List<IEditorJoint> joints)
        {
            base.Init(shapes, joints, true, true);
            return this;
        }

        protected override void CreateJoint(IEditorShape shape1, IEditorShape shape2, Vec2D r1, Vec2D r2)
        {
            joints.Add(new EditorWheelJoint(shape1, shape2, r1, r2));
        }
    }
}