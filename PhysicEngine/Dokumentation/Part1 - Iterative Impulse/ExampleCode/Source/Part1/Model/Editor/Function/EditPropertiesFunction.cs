using GraphicMinimal;
using GraphicPanels;
using Part1.Model.Editor.EditorShape;
using Part1.ViewModel.Editor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Part1.Model.Editor.Function
{
    class EditPropertiesFunction : IFunction
    {
        private List<IEditorShape> shapes = null;
        private IEditorShape shapeWhereMouseIsOver = null;
        private IEditorShape selectedShape = null;
        private Action<IEditorShape> shapeSelectedHandler;

        public IFunction Init(List<IEditorShape> shapes)
        {
            this.shapes = shapes;
            return this;
        }

        //Später soll hier noch ein jointSelectedHandler dazu kommen
        public IFunction Init(List<IEditorShape> shapes, Action<IEditorShape> shapeSelectedHandler)
        {
            this.shapes = shapes;
            this.shapeSelectedHandler = shapeSelectedHandler;
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
            if (this.shapeWhereMouseIsOver != null)
            {
                if (this.selectedShape != null)
                    this.selectedShape.BorderPen = Pens.Black; //Deselect old Shape

                this.selectedShape = this.shapeWhereMouseIsOver;
                this.shapeSelectedHandler(this.shapeWhereMouseIsOver);
                this.selectedShape.BorderPen = new Pen(Color.Blue, 3);
            }
        }

        public void HandleMouseWheel(MouseEventArgs e)
        {
        }

        public void HandleMouseMove(MouseEventArgs e)
        {
            this.shapeWhereMouseIsOver = null;

            Vector2D mousePosition = new Vector2D(e.X, e.Y);
            foreach (var shape in this.shapes)
            {
                if (shape.IsPointInside(mousePosition))
                {
                    this.shapeWhereMouseIsOver = shape;
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
                Headline = "Edit Properties",
                Values = new[]
                    {
                        "CLICK: edit shape or joint",
                        "ESC: exit"
                    }
            };
        }

        public void Dispose()
        {
            foreach (var shape in this.shapes)
            {
                shape.Backcolor = Color.Transparent;
                shape.BorderPen = Pens.Black;
            }

            this.shapeSelectedHandler(null);
        }
    }
}
