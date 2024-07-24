using GraphicPanels;
using PhysicSceneEditorControl.Controls.Editor.Model.EditorShape;
using RigidBodyPhysics.MathHelper;
using WpfControls.Model;

namespace PhysicSceneEditorControl.Controls.Editor.Model.Function.Shapes
{
    internal class AddCircleFunction : DummyFunction, IFunction
    {
        private List<IEditorShape> shapes = null;
        private bool shiftIsPressed = false;
        private int currentRadius = 20;
        private Vec2D currentMousePosition = new Vec2D(0, 0);
        private MouseGrid mouseGrid;

        public override IFunction Init(FunctionData functionData)
        {
            this.shapes = functionData.Shapes;
            this.mouseGrid = functionData.MouseGrid;
            return this;
        }

        public override void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.LeftShift)
                shiftIsPressed = true;
        }

        public override void HandleKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.LeftShift)
                shiftIsPressed = false;
        }

        public override void HandleMouseClick(MouseEventArgs e)
        {
            Vec2D center = new Vec2D(e.X, e.Y);
            center = this.mouseGrid.SnapMouse(new GraphicMinimal.Vector2D(e.X, e.Y)).ToPhx();
            shapes.Add(new EditorCircle(center, currentRadius));
        }

        public override void HandleMouseWheel(MouseEventArgs e)
        {
            if (shiftIsPressed == false)
                currentRadius += e.Delta / 100;
            else
                currentRadius += e.Delta / 10;



            currentRadius = Math.Max(2, currentRadius);
        }

        public override void HandleMouseMove(MouseEventArgs e)
        {
            currentMousePosition = new Vec2D(e.X, e.Y);
        }

        public override void Draw(GraphicPanel2D panel)
        {
            panel.DrawCircle(Pens.Black, currentMousePosition.ToGrx(), currentRadius);
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
