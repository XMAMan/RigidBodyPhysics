using RigidBodyPhysics.MathHelper;

namespace PhysicSceneEditorControl.Controls.Editor.Model.Function.Joints
{
    internal interface IJointWithLinearMinMax
    {
        bool DrawLimits { get; set; }
        bool LimitIsEnabled { get; set; }
        float MinLength { get; set; }
        float MaxLength { get; set; }
        float GetMinMaxDistance(Vec2D position);
        bool IsPointAboveMinLimit(Vec2D position);
        bool IsPointAboveMaxLimit(Vec2D position);
        Pen MinLimitPen { get; set; }
        Pen MaxLimitPen { get; set; }
        Pen BorderPen { get; set; }
        Color Backcolor { get; set; }
    }

    internal class LimitJointWithLinearMinMax : DummyFunction, IFunction
    {
        private IJointWithLinearMinMax joint;
        private bool minLimitIsClicked = false;
        private bool maxLimitIsClicked = false;
        private string helperText = "";
        private Action selectOtherJoint;

        public override IFunction Init(FunctionData functionData)
        {
            return this;
        }

        public IFunction Init(IJointWithLinearMinMax selectedJoint, Action selectOtherJoint, string helperText)
        {
            joint = selectedJoint;
            this.selectOtherJoint = selectOtherJoint;

            joint.DrawLimits = true;

            this.helperText = helperText;

            return this;
        }

        public override void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.E && joint.LimitIsEnabled == false)
            {
                joint.LimitIsEnabled = true;
            }

            if (e.Key == System.Windows.Input.Key.D && joint.LimitIsEnabled == true)
            {
                joint.LimitIsEnabled = false;
            }
        }

        public override void HandleKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.LeftShift)
            {
                this.selectOtherJoint();
            }
        }

        public override void HandleMouseMove(MouseEventArgs e)
        {
            Vec2D mousePosition = new Vec2D(e.X, e.Y);

            if (minLimitIsClicked)
            {
                float length = joint.GetMinMaxDistance(mousePosition);

                //Sorge dafür, dass MinLength immer kleiner als MaxLength bleibt indem beim Vorbeiziehen das angklickte Limit wechselt
                if (length < joint.MaxLength)
                    joint.MinLength = length;
                else
                    SwitchClickedLimit();

            }
            else
            {
                if (joint.IsPointAboveMinLimit(mousePosition))
                {
                    joint.MinLimitPen = new Pen(Color.Red, 3);
                }
                else
                {
                    joint.MinLimitPen = joint.BorderPen;
                }
            }

            if (maxLimitIsClicked)
            {
                float length = joint.GetMinMaxDistance(mousePosition);

                //Sorge dafür, dass MinLength immer kleiner als MaxLength bleibt indem beim Vorbeiziehen das angklickte Limit wechselt
                if (length > joint.MinLength)
                    joint.MaxLength = length;
                else
                    SwitchClickedLimit();
            }
            else
            {
                if (joint.IsPointAboveMaxLimit(mousePosition))
                {
                    joint.MaxLimitPen = new Pen(Color.Red, 3);
                }
                else
                {
                    joint.MaxLimitPen = joint.BorderPen;
                }
            }
        }

        private void SwitchClickedLimit()
        {
            if (minLimitIsClicked && maxLimitIsClicked == false)
            {
                minLimitIsClicked = false;
                joint.MinLimitPen = joint.BorderPen;

                maxLimitIsClicked = true;
                joint.MaxLimitPen = new Pen(Color.Red, 3);
            }
            else

            if (minLimitIsClicked == false && maxLimitIsClicked)
            {
                minLimitIsClicked = true;
                joint.MinLimitPen = new Pen(Color.Red, 3);

                maxLimitIsClicked = false;
                joint.MaxLimitPen = joint.BorderPen;
            }
        }

        public override void HandleMouseDown(MouseEventArgs e)
        {
            Vec2D mousePosition = new Vec2D(e.X, e.Y);

            if (minLimitIsClicked == false && joint.IsPointAboveMinLimit(mousePosition))
            {
                minLimitIsClicked = true;
                joint.MinLimitPen = new Pen(Color.Red, 3);
            }

            if (maxLimitIsClicked == false && joint.IsPointAboveMaxLimit(mousePosition))
            {
                maxLimitIsClicked = true;
                joint.MaxLimitPen = new Pen(Color.Red, 3);
            }
        }
        public override void HandleMouseUp(MouseEventArgs e)
        {
            if (minLimitIsClicked)
            {
                minLimitIsClicked = false;
                joint.MinLimitPen = joint.BorderPen;
            }

            if (maxLimitIsClicked)
            {
                maxLimitIsClicked = false;
                joint.MaxLimitPen = joint.BorderPen;
            }
        }

        public override FunctionHelpText GetHelpText()
        {
            return new FunctionHelpText()
            {
                Headline = this.helperText,
                Values = new[]
                    {
                        "CLICK AND DRAG: move Limits",
                        joint.LimitIsEnabled == false ? "e: Enable limits" : "d Disable limits",
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
