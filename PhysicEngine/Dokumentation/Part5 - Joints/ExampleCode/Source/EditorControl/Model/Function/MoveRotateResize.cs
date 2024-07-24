using EditorControl.Model.EditorJoint;
using EditorControl.Model.EditorShape;
using PhysicEngine.MathHelper;

namespace EditorControl.Model.Function
{
    internal abstract class MoveRotateResize : DummyFunction, IFunction
    {
        protected List<IEditorShape> shapes = null;
        protected List<IEditorJoint> joints = null;
        protected bool shiftIsPressed = false;
        protected IEditorShape selectedShape = null;
        private Vec2D centerToMouseDown = null;

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

        public override void HandleMouseMove(MouseEventArgs e)
        {
            //Wenn die Taste nicht unten ist schaue ob die Mause über einen Objekt ist
            if (this.centerToMouseDown == null)
            {
                Vec2D mousePosition = new Vec2D(e.X, e.Y);

                this.selectedShape = null;
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

            //Wenn die Taste unten ist und ein Objekt selektiert wurde dann verschiebe es
            if (this.selectedShape != null && this.centerToMouseDown != null)
            {
                this.selectedShape.MoveTo(new Vec2D(e.X, e.Y) - this.centerToMouseDown);
                var jointsToUpdate = this.joints.Where(x => x.Body1 == this.selectedShape || x.Body2 == this.selectedShape).ToList();
                jointsToUpdate.ForEach(x => x.UpdateAfterMovingBodys());
            }
        }

        public override void HandleMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            if (this.selectedShape != null)
            {
                this.centerToMouseDown = new Vec2D(e.X, e.Y) - this.selectedShape.Center;
            }
        }
        public override void HandleMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            this.selectedShape = null;
            this.centerToMouseDown = null;
        }

        public override void Dispose()
        {
            foreach (var shape in this.shapes)
                shape.Backcolor = Color.Transparent;
        }
    }
}
