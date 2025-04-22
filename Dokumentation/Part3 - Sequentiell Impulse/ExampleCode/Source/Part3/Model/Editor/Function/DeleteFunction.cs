using GraphicMinimal;
using GraphicPanels;
using Part3.Model.Editor.EditorShape;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Part3.Model.Editor.Function
{
    class DeleteFunction : IFunction
    {
        private List<IEditorShape> shapes = null;
        private IEditorShape selectedShape = null;

        public IFunction Init(List<IEditorShape> shapes)
        {
            this.shapes = shapes;
            return this;
        }

        public void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
        }

        public void HandleKeyUp(System.Windows.Input.KeyEventArgs e)
        {
        }

        public void HandleMouseClick(System.Windows.Forms.MouseEventArgs e)
        {
            if (this.selectedShape != null)
            {
                this.shapes.Remove(this.selectedShape);
            }
        }

        public void HandleMouseWheel(MouseEventArgs e)
        {
        }

        public void HandleMouseMove(MouseEventArgs e)
        {
            this.selectedShape = null;

            Vector2D mousePosition = new Vector2D(e.X, e.Y);
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

        public void HandleMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
        }
        public void HandleMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
        }

        public void Draw(GraphicPanel2D panel)
        {
        }

        public FunctionHelpText GetHelpText()
        {
            return new FunctionHelpText()
            {
                Headline = "Delete shapes and joints",
                Values = new[]
                    {
                        "CLICK: delete shape or joint",
                        "ESC: exit"
                    }
            };
        }

        public void Dispose()
        {
            foreach (var shape in this.shapes)
                shape.Backcolor = Color.Transparent;
        }
    }
}
