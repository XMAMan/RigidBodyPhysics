using PhysicSceneEditorControl.Controls.Editor.Model.EditorJoint;
using RigidBodyPhysics.MathHelper;

namespace PhysicSceneEditorControl.Controls.Editor.Model.Function.Joints
{
    //Diese Funktion dient zur Auswahl von ein Joint, was dann grafisch editiert werden soll
    internal class SelectJointFunction : DummyFunction, IFunction
    {
        private List<IEditorJoint> joints = null;
        private IEditorJoint jointWhereMouseIsOver = null;
        private IEditorJoint selectedJoint = null;
        private Action<IEditorJoint> jointSelectedHandler;

        public override IFunction Init(FunctionData functionData)
        {
            this.joints = functionData.Joints;
            return this;
        }

        public IFunction Init(FunctionData functionData, Action<IEditorJoint> jointSelectedHandler)
        {
            this.joints = functionData.Joints;
            this.jointSelectedHandler = jointSelectedHandler;
            return this;
        }

        public override void HandleMouseClick(MouseEventArgs e)
        {
            if (jointWhereMouseIsOver != null)
            {
                if (selectedJoint != null)
                    selectedJoint.BorderPen = Pens.Blue;  //Deselect old Joint

                selectedJoint = jointWhereMouseIsOver;
                jointSelectedHandler(jointWhereMouseIsOver);
                selectedJoint.BorderPen = new Pen(Color.DarkGreen, 3);
            }
        }

        public override void HandleMouseMove(MouseEventArgs e)
        {
            Vec2D mousePosition = new Vec2D(e.X, e.Y);

            jointWhereMouseIsOver = null;
            foreach (var joint in joints)
            {
                if (joint.IsPointInside(mousePosition) && joint.SupportsDefineLimit)
                {
                    jointWhereMouseIsOver = joint;
                    joint.Backcolor = Color.Red;
                }
                else
                {
                    joint.Backcolor = Color.Transparent;
                }
            }
        }

        public override FunctionHelpText GetHelpText()
        {
            return new FunctionHelpText()
            {
                Headline = "Limit Joint",
                Values = new[]
                    {
                        "CLICK: edit joint limits",
                        "ESC: exit"
                    }
            };
        }

        public override void Dispose()
        {
            foreach (var joint in joints)
            {
                joint.Backcolor = Color.Transparent;
                joint.BorderPen = Pens.Blue;
            }

            //this.jointSelectedHandler(null);
        }
    }
}
