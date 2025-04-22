using GraphicMinimal;
using GraphicPanels;
using Part1.Model.Editor.EditorShape;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Part1.Model.Editor.Function
{
    class AddCircleFunction : IFunction
    {
        private List<IEditorShape> shapes = null;
        private bool shiftIsPressed = false;
        private int currentRadius = 20;
        private Vector2D currentMousePosition = new Vector2D(0, 0);

        public IFunction Init(List<IEditorShape> shapes)
        {
            this.shapes = shapes;
            return this;
        }

        public void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.LeftShift)
                this.shiftIsPressed = true;
        }

        public void HandleKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.LeftShift)
                this.shiftIsPressed = false;
        }

        public void HandleMouseClick(System.Windows.Forms.MouseEventArgs e)
        {
            Vector2D center = new Vector2D(e.X, e.Y);
            this.shapes.Add(new EditorCircle(center, this.currentRadius));
        }

        public void HandleMouseWheel(MouseEventArgs e)
        {
            if (this.shiftIsPressed == false)
                this.currentRadius += e.Delta / 100;
            else
                this.currentRadius += e.Delta / 10;

            this.currentRadius = Math.Max(2, this.currentRadius);
        }

        public void HandleMouseMove(MouseEventArgs e)
        {
            this.currentMousePosition = new Vector2D(e.X, e.Y);
        }
        public void HandleMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
        }
        public void HandleMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
        }

        public void Draw(GraphicPanel2D panel)
        {
            panel.DrawCircle(Pens.Black, this.currentMousePosition, this.currentRadius);
        }

        public FunctionHelpText GetHelpText()
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

        public void Dispose()
        {
        }
    }
}
