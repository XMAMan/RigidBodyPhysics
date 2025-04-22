using EditorControl.Model.EditorJoint;
using EditorControl.Model.EditorShape;

namespace EditorControl.Model.Function
{
    internal class MoveResizeFunction : MoveRotateResize, IFunction
    {
        public override IFunction Init(List<IEditorShape> shapes, List<IEditorJoint> joints)
        {
            this.shapes = shapes;
            this.joints = joints;
            return this;
        }

        public override void HandleMouseWheel(MouseEventArgs e)
        {
            if (this.selectedShape != null)
            {
                float size = Math.Min(1, Math.Max(-1, e.Delta / 150f)); //Clamp from -1 to +1

                if (this.shiftIsPressed)
                    this.selectedShape.Resize(1 + size / 2);
                else
                    this.selectedShape.Resize(1 + size / 20);
            }
        }

        public override FunctionHelpText GetHelpText()
        {
            return new FunctionHelpText()
            {
                Headline = "Move and Resize",
                Values = new[]
                    {
                        "CLICK AND DRAG: move shape or joint",
                        "MOUSE WHEEL: scale shape",
                        "HOLD SHIFT: increase scale speed",
                        "ESC: exit"
                    }
            };
        }
    }
}
