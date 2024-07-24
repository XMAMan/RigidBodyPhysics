using PhysicSceneEditorControl.Controls.Editor.Model.EditorJoint;
using RigidBodyPhysics.MathHelper;

namespace PhysicSceneEditorControl.Controls.Editor.Model.Function.Joints.RevoluteJoint
{
    internal class LimitRevoluteJointFunction : DummyFunction, IFunction
    {
        private EditorRevoluteJoint joint;
        private bool armAIsClicked = false;
        private bool armBIsClicked = false;
        private Action selectOtherJoint;

        public override IFunction Init(FunctionData functionData)
        {
            return this;
        }

        public IFunction Init(EditorRevoluteJoint selectedJoint, Action selectOtherJoint)
        {
            joint = selectedJoint;
            this.selectOtherJoint = selectOtherJoint;

            joint.DrawLimits = true;

            joint.UpdateAfterMovingBodys();



            return this;
        }

        public override void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.E && joint.IsLimitDisabled())
            {
                joint.EnableLimits();
            }

            if (e.Key == System.Windows.Input.Key.D && joint.IsLimitDisabled() == false)
            {
                joint.DisableLimits();
            }

            if (e.Key == System.Windows.Input.Key.LeftShift)
            {
                this.selectOtherJoint();
            }
        }

        public override void HandleMouseMove(MouseEventArgs e)
        {
            Vec2D mousePosition = new Vec2D(e.X, e.Y);

            if (this.armAIsClicked)
            {
                float angle = joint.GetLimitAngle(mousePosition);
                joint.Properties.LowerAngle = angle;
                joint.UpdateAfterMovingBodys();
            }
            else
            {
                if (joint.IsPointAboveLimitArmA(mousePosition))
                {
                    joint.LimitArmAPen = new Pen(Color.Red, 10);
                }
                else
                {
                    joint.LimitArmAPen = new Pen(Color.Orange, 5);
                }
            }

            if (this.armBIsClicked)
            {
                float angle = joint.GetLimitAngle(mousePosition);
                joint.Properties.UpperAngle = angle;
                joint.UpdateAfterMovingBodys();
            }
            else
            {
                if (joint.IsPointAboveLimitArmB(mousePosition))
                {
                    joint.LimitArmBPen = new Pen(Color.Red, 10);
                }
                else
                {
                    joint.LimitArmBPen = new Pen(Color.BlueViolet, 5);
                }
            }
        }

        public override void HandleMouseDown(MouseEventArgs e)
        {
            Vec2D mousePosition = new Vec2D(e.X, e.Y);

            if (armAIsClicked == false && armBIsClicked == false)
            {
                if (joint.IsPointAboveLimitArmA(mousePosition))
                {
                    armAIsClicked = true;
                    joint.LimitArmAPen = new Pen(Color.Red, 10);
                }
                else if (joint.IsPointAboveLimitArmB(mousePosition))
                {
                    armBIsClicked = true;
                    joint.LimitArmBPen = new Pen(Color.Red, 10);
                }
            }


        }
        public override void HandleMouseUp(MouseEventArgs e)
        {
            if (armAIsClicked)
            {
                armAIsClicked = false;
                joint.LimitArmAPen = new Pen(Color.Orange, 5);
            }

            if (armBIsClicked)
            {
                armBIsClicked = false;
                joint.LimitArmBPen = new Pen(Color.BlueViolet, 5);
            }
        }

        public override FunctionHelpText GetHelpText()
        {
            return new FunctionHelpText()
            {
                Headline = "Define Min-Max-Range from Arm2",
                Values = new[]
                    {
                        "CLICK AND DRAG: move Limits",
                        joint.IsLimitDisabled() ? "e: Enable limits" : "d Disable limits",
                        "Shift: Select a different joint",
                        "ESC: exit"
                    }
            };
        }

        public override void Dispose()
        {
            joint.Backcolor = Color.Transparent;
            joint.BorderPen = Pens.Blue;

            joint.DrawLimits = false;
        }
    }
}
