using RigidBodyPhysics.MathHelper;

namespace PhysicSceneEditorControl.Controls.Editor.Model.Function
{
    //Diese Funktion dient zur Auswahl von ein Shape/Joint, was dann textuell editiert werden soll
    internal class EditPropertiesFunction : DummyFunction, IFunction
    {
        private List<ISelectableShape> shapes = new List<ISelectableShape>();
        private List<ISelectable> lineShapes = new List<ISelectable>();
        private ISelectable shapeWhereMouseIsOver = null;
        private ISelectable selectedShape = null;
        

        private Action<ISelectable> editorObjectSelectedHandler;

        private bool shiftIsPressed = false;
        public override IFunction Init(FunctionData functionData)
        {
            throw new NotImplementedException();
        }

        public IFunction Init(FunctionData functionData, Action<ISelectable> editorObjectSelectedHandler)
        {
            this.shapes.AddRange(functionData.Shapes);

            this.lineShapes.AddRange(functionData.Joints);
            this.lineShapes.AddRange(functionData.Thrusters);
            this.lineShapes.AddRange(functionData.RotaryMotors);
            this.lineShapes.AddRange(functionData.AxialFrictions);

            this.editorObjectSelectedHandler = editorObjectSelectedHandler;

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
                Deselect();

                this.selectedShape = this.shapeWhereMouseIsOver;
                this.editorObjectSelectedHandler(this.shapeWhereMouseIsOver);      
                
                if (this.selectedShape is ISelectableShape)
                {
                    this.selectedShape.BorderPen = new Pen(Color.Blue, 3); //Kreis, Rechteck, Polygon
                }else
                {
                    this.selectedShape.BorderPen = new Pen(Color.DarkGreen, 3); //Joint, Thruster, RotaryMotor, AxialFriction
                }
                
            }
        }

        private void Deselect()
        {
            if (this.selectedShape != null)
            {
                if (this.selectedShape is ISelectableShape)
                    this.selectedShape.BorderPen = Pens.Black; //Deselect old Shape
                else
                    this.selectedShape.BorderPen = Pens.Blue; //Deselect old LineShape
            }
        }

        public override void HandleMouseMove(MouseEventArgs e)
        {
            Vec2D mousePosition = new Vec2D(e.X, e.Y);

            if (this.shapeWhereMouseIsOver != null)
            {
                this.shapeWhereMouseIsOver.Backcolor = Color.Transparent;
            }

            this.shapeWhereMouseIsOver = ShapeHelper.GetLineShapeFromPoint(this.lineShapes, mousePosition);
            if (this.shiftIsPressed == false && this.shapeWhereMouseIsOver == null)
            {
                this.shapeWhereMouseIsOver = ShapeHelper.GetShapeFromPoint(shapes, mousePosition);
            }
        }

        public override FunctionHelpText GetHelpText()
        {
            string selectedShapeName = this.selectedShape != null ? (this.selectedShape.GetType().Name.Replace("Editor","")) : "";

            return new FunctionHelpText()
            {
                Headline = this.selectedShape == null ? "Edit Properties" : "Edit " + selectedShapeName,
                Values = new[]
                    {
                        "CLICK: edit shape, joint, thruster, motor or axial-friction",
                        "Hold Shift: select joint, thruster, motor or axial-friction only",
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
            foreach (var lineShape in this.lineShapes)
            {
                lineShape.Backcolor = Color.Transparent;
                lineShape.BorderPen = Pens.Blue;
            }

            this.editorObjectSelectedHandler(null);
        }
    }
}
