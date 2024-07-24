using EditorControl.Model.EditorJoint;
using EditorControl.Model.EditorShape;
using EditorControl.Model.EditorThruster;
using PhysicEngine.MathHelper;
using WpfControls.Model;

namespace EditorControl.Model.Function.Shapes
{
    internal abstract class MoveRotateResize : DummyFunction, IFunction
    {
        protected List<IEditorShape> shapes = null;
        protected List<IEditorJoint> joints = null;
        protected List<IEditorThruster> thrusters = null;
        protected MouseGrid mouseGrid = null;
        protected bool shiftIsPressed = false;
        protected IEditorShape selectedShape = null;
        private Vec2D centerToMouseDown = null;

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

        public override void HandleMouseMove(MouseEventArgs e)
        {
            //Wenn die Taste nicht unten ist schaue ob die Mause über einen Objekt ist
            if (centerToMouseDown == null)
            {
                Vec2D mousePosition = new Vec2D(e.X, e.Y);

                selectedShape = null;
                this.selectedShape = ShapeHelper.GetShapeFromPoint(shapes, mousePosition);
            }

            //Wenn die Taste unten ist und ein Objekt selektiert wurde dann verschiebe es
            if (selectedShape != null && centerToMouseDown != null)
            {
                var newPos = new Vec2D(e.X, e.Y) - centerToMouseDown;
                newPos = this.mouseGrid.SnapMouse(newPos.ToGrx()).ToPhx();
                selectedShape.MoveTo(newPos);

                UpdateJointsAndThrusters(this.selectedShape);
            }
        }

        protected void UpdateJointsAndThrusters(IEditorShape selectedShape)
        {
            var jointsToUpdate = joints.Where(x => x.Body1 == selectedShape || x.Body2 == selectedShape).ToList();
            jointsToUpdate.ForEach(x => x.UpdateAfterMovingBodys());

            var thrustersToUpdate = thrusters.Where(x => x.Body == selectedShape).ToList();
            thrustersToUpdate.ForEach(x => x.UpdateAfterMovingBodys());
        }

        public override void HandleMouseDown(MouseEventArgs e)
        {
            if (selectedShape != null)
            {
                centerToMouseDown = new Vec2D(e.X, e.Y) - selectedShape.Center;
            }
        }
        public override void HandleMouseUp(MouseEventArgs e)
        {
            selectedShape = null;
            centerToMouseDown = null;
        }

        public override void Dispose()
        {
            foreach (var shape in shapes)
                shape.Backcolor = Color.Transparent;
        }
    }
}
