using EditorControl.Model.EditorJoint;
using EditorControl.Model.EditorShape;
using GraphicPanels;
using PhysicEngine.MathHelper;

namespace EditorControl.Model.Function
{
    internal class DeleteFunction : DummyFunction, IFunction
    {
        private List<IEditorShape> shapes = null;
        private List<IEditorJoint> joints = null;
        private IEditorShape selectedShape = null;
        private IEditorJoint selectedJoint = null;
        private bool shiftIsPressed = false;

        public override IFunction Init(List<IEditorShape> shapes, List<IEditorJoint> joints)
        {
            this.shapes = shapes;
            this.joints = joints;
            return this;
        }

        public override void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.LeftShift)
                this.shiftIsPressed = true;
        }

        public override void HandleKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.LeftShift)
                this.shiftIsPressed = false;
        }

        public override void HandleMouseClick(System.Windows.Forms.MouseEventArgs e)
        {
            if (this.selectedShape != null)
            {
                this.shapes.Remove(this.selectedShape);

                if (this.joints.Any(x => x.Body1 == this.selectedShape || x.Body2 == this.selectedShape))
                {
                    var jointsToDelete = this.joints.Where(x => x.Body1 == this.selectedShape || x.Body2 == this.selectedShape).ToList();
                    foreach (var joint in jointsToDelete)
                    {
                        this.joints.Remove(joint);
                    }
                }
            }

            if (this.selectedJoint != null)
            {
                this.joints.Remove(this.selectedJoint);
            }
        }

        public override void HandleMouseMove(MouseEventArgs e)
        {
            Vec2D mousePosition = new Vec2D(e.X, e.Y);

            this.selectedShape = null;
            if (this.shiftIsPressed == false)
            {
                foreach (var shape in this.shapes)
                {
                    if (shape.IsPointInside(mousePosition))
                    {
                        this.selectedShape = shape;
                        shape.Backcolor = Color.Red;
                    }
                    else
                    {
                        shape.Backcolor = Color.Transparent;
                    }
                }
            }            

            this.selectedJoint = null;
            foreach (var joint in this.joints)
            {
                if (joint.IsPointInside(mousePosition))
                {
                    this.selectedJoint = joint;
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
                Headline = "Delete shapes and joints",
                Values = new[]
                    {
                        "CLICK: delete shape or joint",
                        "Hold Shift: select joint only",
                        "ESC: exit"
                    }
            };
        }

        public override void Dispose()
        {
            foreach (var shape in this.shapes)
                shape.Backcolor = Color.Transparent;

            foreach (var joint in this.joints)
                joint.Backcolor = Color.Transparent;
        }
    }
}
