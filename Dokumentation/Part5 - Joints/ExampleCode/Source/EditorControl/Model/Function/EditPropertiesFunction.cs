using EditorControl.Model.EditorJoint;
using EditorControl.Model.EditorShape;
using GraphicPanels;
using PhysicEngine.MathHelper;
using System.Windows.Shapes;

namespace EditorControl.Model.Function
{
    //Diese Funktion dient zur Auswahl von ein Shape/Joint, was dann textuell editiert werden soll
    internal class EditPropertiesFunction : DummyFunction, IFunction
    {
        private List<IEditorShape> shapes = null;
        private List<IEditorJoint> joints = null;
        private IEditorShape shapeWhereMouseIsOver = null;
        private IEditorShape selectedShape = null;
        private IEditorJoint jointWhereMouseIsOver = null;
        private IEditorJoint selectedJoint = null;
        private Action<IEditorShape> shapeSelectedHandler;
        private Action<IEditorJoint> jointSelectedHandler;
        private bool shiftIsPressed = false;
        public override IFunction Init(List<IEditorShape> shapes, List<IEditorJoint> joints)
        {
            this.shapes = shapes;
            this.joints = joints;
            return this;
        }

        public IFunction Init(List<IEditorShape> shapes, List<IEditorJoint> joints, Action<IEditorShape> shapeSelectedHandler, Action<IEditorJoint> jointSelectedHandler)
        {
            this.shapes = shapes;
            this.joints = joints;
            this.shapeSelectedHandler = shapeSelectedHandler;
            this.jointSelectedHandler = jointSelectedHandler;
            return this;
        }

        public override void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.LeftShift)
            {
                this.shiftIsPressed = true;
                if (this.shapeWhereMouseIsOver != null)
                {
                    this.shapeWhereMouseIsOver.Backcolor = Color.Transparent;
                    this.shapeWhereMouseIsOver = null;
                }
                
            }
                
        }

        public override void HandleKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.LeftShift)
                this.shiftIsPressed = false;
        }

        public override void HandleMouseClick(System.Windows.Forms.MouseEventArgs e)
        {
            if (this.shapeWhereMouseIsOver != null)
            {
                if (this.selectedShape != null)
                    this.selectedShape.BorderPen = Pens.Black; //Deselect old Shape

                if (this.selectedJoint != null)
                    this.selectedJoint.BorderPen = Pens.Blue;  //Deselect old Joint

                this.selectedShape = this.shapeWhereMouseIsOver;
                this.shapeSelectedHandler(this.shapeWhereMouseIsOver);
                this.selectedShape.BorderPen = new Pen(Color.Blue, 3);
            }

            if (this.jointWhereMouseIsOver != null)
            {
                if (this.selectedShape != null)
                    this.selectedShape.BorderPen = Pens.Black; //Deselect old Shape

                if (this.selectedJoint != null)
                    this.selectedJoint.BorderPen = Pens.Blue;  //Deselect old Joint

                this.selectedJoint = this.jointWhereMouseIsOver;
                this.jointSelectedHandler(this.jointWhereMouseIsOver);
                this.selectedJoint.BorderPen = new Pen(Color.DarkGreen, 3);
            }
        }

        public override void HandleMouseMove(MouseEventArgs e)
        {
            Vec2D mousePosition = new Vec2D(e.X, e.Y);

            this.shapeWhereMouseIsOver = null;
            if (this.shiftIsPressed == false)
            {
                foreach (var shape in this.shapes)
                {
                    if (shape.IsPointInside(mousePosition))
                    {
                        this.shapeWhereMouseIsOver = shape;
                        shape.Backcolor = Color.Red;
                    }
                    else
                    {
                        shape.Backcolor = Color.Transparent;
                    }
                }
            }
            

            this.jointWhereMouseIsOver = null;
            foreach (var joint in this.joints)
            {
                if (joint.IsPointInside(mousePosition))
                {
                    this.jointWhereMouseIsOver = joint;
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
                Headline = "Edit Properties",
                Values = new[]
                    {
                        "CLICK: edit shape or joint",
                        "Hold Shift: select joint only",
                        "ESC: exit"
                    }
            };
        }

        public override void Dispose()
        {
            foreach (var shape in this.shapes)
            {
                shape.Backcolor = Color.Transparent;
                shape.BorderPen = Pens.Black;
            }
            foreach (var joint in this.joints)
            {
                joint.Backcolor = Color.Transparent;
                joint.BorderPen = Pens.Blue;
            }

            this.shapeSelectedHandler(null);
        }
    }
}
