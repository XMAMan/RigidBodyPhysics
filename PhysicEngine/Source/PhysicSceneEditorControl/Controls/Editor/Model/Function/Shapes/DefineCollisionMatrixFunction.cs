using PhysicSceneEditorControl.Controls.Editor.Model.EditorShape;
using RigidBodyPhysics.MathHelper;
using WpfControls.Controls.CollisionMatrix;

namespace PhysicSceneEditorControl.Controls.Editor.Model.Function.Shapes
{
    //Über die rechte Matrix am rand wird definiert, welche CollisionCategory mit welcher anderen kollidiert.
    internal class DefineCollisionMatrixFunction : DummyFunction
    {
        protected List<IEditorShape> shapes = null;
        protected IEditorShape selectedShape = null;
        private CollisionMatrixViewModel model;

        public override IFunction Init(FunctionData functionData)
        {
            this.shapes = functionData.Shapes;
            return this;
        }

        public IFunction Init(FunctionData functionData, CollisionMatrixViewModel model)
        {
            this.model = model;
            this.shapes = functionData.Shapes;
            UpdateAllBorderColors();
            return this;
        }

        private void UpdateAllBorderColors()
        {
            foreach (var shape in this.shapes)
            {
                shape.BorderPen = new Pen(model.Colors[shape.Properties.CollisionCategory], 3);
            }
        }

        public override void HandleMouseClick(MouseEventArgs e)
        {
            if (this.selectedShape != null)
            {
                this.selectedShape.Properties.CollisionCategory = model.SelectedIndex;
                UpdateAllBorderColors();
            }
        }

        public override void HandleMouseMove(MouseEventArgs e)
        {
            Vec2D mousePosition = new Vec2D(e.X, e.Y);

            selectedShape = null;
            this.selectedShape = ShapeHelper.GetShapeFromPoint(shapes, mousePosition);
        }

        public override FunctionHelpText GetHelpText()
        {
            return new FunctionHelpText()
            {
                Headline = "Step 1: Select color and click on shape; Step 2: Define Matrix",
                Values = new[]
                    {
                        "CLICK on Shape: Define Color(CollisionCategory)",
                        "ESC: exit"
                    }
            };
        }

        public override void Dispose()
        {
            foreach (var shape in shapes)
            {
                shape.Backcolor = Color.Transparent;
                shape.BorderPen = Pens.Black;
            }

        }
    }
}
