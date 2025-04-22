using GraphicMinimal;
using GraphicPanels;
using Part1.Model.Editor.EditorShape;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Part1.Model.Editor.Function
{
    class MoveResizeFunction : MoveRotateResize, IFunction
    {
        public override IFunction Init(List<IEditorShape> shapes)
        {
            this.shapes = shapes;
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
