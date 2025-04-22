using EditorControl.Model.EditorJoint;
using EditorControl.Model.EditorRotaryMotor;
using EditorControl.Model.EditorShape;
using EditorControl.Model.EditorThruster;
using PhysicEngine.MathHelper;

namespace EditorControl.Model.Function
{
    //Diese Funktion dient zur Auswahl von ein Shape/Joint, was dann textuell editiert werden soll
    internal class EditPropertiesFunction : DummyFunction, IFunction
    {
        private List<IEditorShape> shapes = null;
        private List<IEditorJoint> joints = null;
        private List<IEditorThruster> thrusters = null;
        private List<IEditorRotaryMotor> motors = null;

        private IEditorShape shapeWhereMouseIsOver = null;
        private IEditorJoint jointWhereMouseIsOver = null;
        private IEditorThruster thrusterWhereMouseIsOver = null;
        private IEditorRotaryMotor motorWhereMouseIsOver = null;

        private IEditorShape selectedShape = null;
        private IEditorJoint selectedJoint = null;
        private IEditorThruster selectedThruster = null;
        private IEditorRotaryMotor selectedMotor = null;


        private Action<IEditorShape> shapeSelectedHandler;
        private Action<IEditorJoint> jointSelectedHandler;
        private Action<IEditorThruster> thrusterSelectedHandler;
        private Action<IEditorRotaryMotor> motorSelectedHandler;

        private bool shiftIsPressed = false;
        public override IFunction Init(FunctionData functionData)
        {
            this.shapes = functionData.Shapes;
            this.joints = functionData.Joints;
            return this;
        }

        public IFunction Init(FunctionData functionData, Action<IEditorShape> shapeSelectedHandler, Action<IEditorJoint> jointSelectedHandler, Action<IEditorThruster> thrusterSelectedHandler, Action<IEditorRotaryMotor> motorSelectedHandler)
        {
            this.shapes = functionData.Shapes;
            this.joints = functionData.Joints;
            this.thrusters = functionData.Thrusters;
            this.motors = functionData.RotaryMotors;
            this.shapeSelectedHandler = shapeSelectedHandler;
            this.jointSelectedHandler = jointSelectedHandler;
            this.thrusterSelectedHandler = thrusterSelectedHandler;
            this.motorSelectedHandler = motorSelectedHandler;
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

            if (this.thrusterWhereMouseIsOver != null)
            {
                if (this.selectedThruster != null)
                    this.selectedThruster.BorderPen = Pens.Black; //Deselect old Shape

                if (this.selectedThruster != null)
                    this.selectedThruster.BorderPen = Pens.Blue;  //Deselect old Joint

                this.selectedThruster = this.thrusterWhereMouseIsOver;
                this.thrusterSelectedHandler(this.thrusterWhereMouseIsOver);
                this.selectedThruster.BorderPen = new Pen(Color.DarkGreen, 3);
            }

            if (this.motorWhereMouseIsOver != null)
            {
                if (this.selectedMotor != null)
                    this.selectedMotor.BorderPen = Pens.Black; //Deselect old Shape

                if (this.selectedMotor != null)
                    this.selectedMotor.BorderPen = Pens.Blue;  //Deselect old Joint

                this.selectedMotor = this.motorWhereMouseIsOver;
                this.motorSelectedHandler(this.motorWhereMouseIsOver);
                this.selectedMotor.BorderPen = new Pen(Color.DarkGreen, 3);
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
                    this.shapeWhereMouseIsOver = ShapeHelper.GetShapeFromPoint(shapes, mousePosition);
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

            this.thrusterWhereMouseIsOver = null;
            foreach (var thruster in this.thrusters)
            {
                if (thruster.IsPointInside(mousePosition))
                {
                    this.thrusterWhereMouseIsOver = thruster;
                    thruster.Backcolor = Color.Red;
                }
                else
                {
                    thruster.Backcolor = Color.Transparent;
                }
            }

            this.motorWhereMouseIsOver = null;
            foreach (var motor in this.motors)
            {
                if (motor.IsPointInside(mousePosition))
                {
                    this.motorWhereMouseIsOver = motor;
                    motor.Backcolor = Color.Red;
                }
                else
                {
                    motor.Backcolor = Color.Transparent;
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
                        "CLICK: edit shape, joint, thruster or motor",
                        "Hold Shift: select joint, thruster or motor only",
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
            this.jointSelectedHandler(null);
            this.thrusterSelectedHandler(null);
            this.motorSelectedHandler(null);
        }
    }
}
