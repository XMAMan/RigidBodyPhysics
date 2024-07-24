using GraphicMinimal;
using GraphicPanels;
using Part4.Model.Editor.EditorJoint;
using Part4.Model.Editor.EditorShape;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Part4.Model.Editor.Function
{
    class MoveRotateFunction : MoveRotateResize, IFunction
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
                if (this.shiftIsPressed)
                    this.selectedShape.Rotate(e.Delta / 10);
                else
                    this.selectedShape.Rotate(e.Delta / 150f);
            }
        }

        public override FunctionHelpText GetHelpText()
        {
            return new FunctionHelpText()
            {
                Headline = "Move and Rotate",
                Values = new[]
                    {
                        "CLICK AND DRAG: move shape or joint",
                        "MOUSE WHEEL: rotate shape",
                        "HOLD SHIFT: increase rotation speed",
                        "ESC: exit"
                    }
            };
        }
    }
}
