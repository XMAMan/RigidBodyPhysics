using GraphicMinimal;
using GraphicPanels;
using Part3.Model.Editor.EditorShape;
using Part3.Model.ShapeExporter;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Part3.Model.Editor.Function
{
    class CloneShapeFunction : IFunction
    {
        private List<IEditorShape> shapes = null;
        private IEditorShape selectedShape = null;
        private IEditorShape clone = null;
        private Vector2D centerToMouseDown = null;
        private bool shiftIsPressed = false;
        private Vector2D mouseClickPosition = null;

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
            //Es wurde noch kein Clone erzeugt
            if (this.clone == null)
            {
                if (this.selectedShape != null)
                {
                    //Erzeuge Kopie
                    this.clone = ExportHelper.ExportToEditorShape(this.selectedShape.GetExportData());

                    //Merke wo hingeklickt wurde
                    this.centerToMouseDown = new Vector2D(e.X, e.Y) - this.selectedShape.Center;
                    this.mouseClickPosition = new Vector2D(e.X, e.Y);
                }
            }
            else
            {
                //Clone an Mausposition einfügen
                this.shapes.Add(this.clone);
                this.clone = null;
                this.centerToMouseDown = null;
                this.mouseClickPosition = null;
            }
        }

        public void HandleMouseWheel(MouseEventArgs e)
        {
        }

        public void HandleMouseMove(MouseEventArgs e)
        {
            //Wenn die Taste nicht unten ist schaue ob die Mause über einen Objekt ist
            if (this.centerToMouseDown == null)
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

            //Wenn Objekt angeklickt wurde dann verschiebe seinen Clone
            if (this.clone != null && this.centerToMouseDown != null)
            {
                if (this.shiftIsPressed)
                {
                    //Verschiebe nur Horizontal/Vertikal
                    Vector2D p1 = this.mouseClickPosition;
                    Vector2D p2 = new Vector2D(e.X, e.Y);

                    Vector2D l = p2 - p1;

                    Vector2D p3 = null; //Korrigierte Mausposition
                    
                    if (Math.Abs(l.X) >Math.Abs(l.Y))
                    {
                        //Verschiebe horizontal
                        p3 = new Vector2D(e.X, p1.Y);
                    }else
                    {
                        //Verschiebe vertikal
                        p3 = new Vector2D(p1.X, e.Y);
                    }
                    this.clone.MoveTo(p3 - this.centerToMouseDown);
                }
                else
                {
                    this.clone.MoveTo(new Vector2D(e.X, e.Y) - this.centerToMouseDown);
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
            if (this.clone != null)
                this.clone.Draw(panel);
        }

        public FunctionHelpText GetHelpText()
        {
            return new FunctionHelpText()
            {
                Headline = "Select shape to clone",
                Values = new[]
                    {
                        "CLICK: select shape",
                        "HOLD SHIFT: Move only horizontal/vertical",
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
