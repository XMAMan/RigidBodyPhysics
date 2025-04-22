using EditorControl.Model.EditorJoint;
using EditorControl.Model.EditorShape;
using GraphicPanels;
using PhysicEngine.MathHelper;

namespace EditorControl.Model.Function
{
    internal class AddCircleFunction : DummyFunction, IFunction
    {
        private List<IEditorShape> shapes = null;
        private bool shiftIsPressed = false;
        private int currentRadius = 20;
        private Vec2D currentMousePosition = new Vec2D(0, 0);

        public override IFunction Init(List<IEditorShape> shapes, List<IEditorJoint> joints)
        {
            this.shapes = shapes;
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
            Vec2D center = new Vec2D(e.X, e.Y);
            this.shapes.Add(new EditorCircle(center, this.currentRadius));
        }

        public override void HandleMouseWheel(MouseEventArgs e)
        {
            if (this.shiftIsPressed == false)
                this.currentRadius += e.Delta / 100;
            else
                this.currentRadius += e.Delta / 10;

            this.currentRadius = Math.Max(2, this.currentRadius);
        }

        public override void HandleMouseMove(MouseEventArgs e)
        {
            this.currentMousePosition = new Vec2D(e.X, e.Y);
        }

        public override void Draw(GraphicPanel2D panel)
        {
            panel.DrawCircle(Pens.Black, this.currentMousePosition.ToGrx(), this.currentRadius);
        }

        public override FunctionHelpText GetHelpText()
        {
            return new FunctionHelpText()
            {
                Headline = "Add Circle",
                Values = new[]
                {
                    "MOUSE WHEEL: scale circle ",
                    "HOLD SHIFT: increase scale speed",
                    "ESC: exit"
                }
            };
        }
    }
}
