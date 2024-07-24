using PhysicSceneEditorControl.Controls.Editor.Model.EditorRotaryMotor;
using PhysicSceneEditorControl.Controls.Editor.Model.EditorShape;
using RigidBodyPhysics.MathHelper;

namespace PhysicSceneEditorControl.Controls.Editor.Model.Function
{
    internal class AddRotaryMotorFunction : DummyFunction, IFunction
    {
        private List<IEditorShape> shapes = null;
        private List<IEditorRotaryMotor> motors = null;
        private IEditorShape selectedShape = null;

        public override IFunction Init(FunctionData functionData)
        {
            this.shapes = functionData.Shapes;
            this.motors = functionData.RotaryMotors;
            return this;
        }

        public override void HandleMouseClick(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Vec2D mousePosition = new Vec2D(e.X, e.Y);
                selectedShape = ShapeHelper.GetShapeFromPoint(shapes, mousePosition);

                if (selectedShape != null)
                {
                    this.motors.Add(new EditorRotaryMotor.EditorRotaryMotor(selectedShape));
                }
            }
        }

        public override void HandleMouseMove(MouseEventArgs e)
        {
            Vec2D mousePosition = new Vec2D(e.X, e.Y);
            selectedShape = ShapeHelper.GetShapeFromPoint(shapes, mousePosition);
        }

        public override FunctionHelpText GetHelpText()
        {
            return new FunctionHelpText()
            {
                Headline = "Select Body",
                Values = new[]
                        {
                            "CLICK: select shape",
                            "ESC: exit"
                        }
            };
        }
    }
}
