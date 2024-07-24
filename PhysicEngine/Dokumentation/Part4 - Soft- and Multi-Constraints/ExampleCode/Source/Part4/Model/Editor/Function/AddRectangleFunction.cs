using GraphicMinimal;
using GraphicPanels;
using Part4.Model.Editor.EditorJoint;
using Part4.Model.Editor.EditorShape;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Part4.Model.Editor.Function
{
    //Idee für diese Art der Rechteckerzeugung: https://www.thebigcb.com/projects/CreatureCreator/
    class AddRectangleFunction : IFunction
    {
        private enum Mode { CornerMode, LineMode}

        private List<IEditorShape> shapes = null;
        private Point? firstMouseClick = null;
        private Point currentMousePosition = new Point(0,0);
        private Mode mode = Mode.CornerMode;
        private int currentLineModeWidth = 5;
        private bool shiftIsPressed = false;

        public IFunction Init(List<IEditorShape> shapes, List<IEditorJoint> joints)
        {
            this.shapes = shapes;
            return this;
        }

        public void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Tab)
                this.mode = (this.mode == Mode.CornerMode) ? Mode.LineMode : Mode.CornerMode;

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
            //Beginne ein neues Rechteck aufzuziehen
            if (this.firstMouseClick == null)
                this.firstMouseClick = new Point(e.X, e.Y);
            else
            {
                //Erzeuge ein neues Rechteck
                if (this.mode == Mode.CornerMode)
                {
                    var r = GetAxialAlignedRectangle(this.firstMouseClick.Value, this.currentMousePosition);
                    var recShape = new EditorRectangle(new Vector2D(r.X + r.Width / 2, r.Y + r.Height / 2), new Vector2D(r.Width, r.Height), 0);
                    this.shapes.Add(recShape);
                }else
                {
                    var r = GetRotatedRectangle(this.firstMouseClick.Value, this.currentMousePosition);
                    var recShape = new EditorRectangle(r.Center, r.Size, r.AngleInDegree);
                    this.shapes.Add(recShape);
                }                

                this.firstMouseClick = null;
            }
        }

        public void HandleMouseWheel(MouseEventArgs e)
        {
            if (this.mode == Mode.LineMode)
            {
                if (this.shiftIsPressed == false)
                    this.currentLineModeWidth += e.Delta / 100;
                else
                    this.currentLineModeWidth += e.Delta / 10;

                this.currentLineModeWidth = Math.Max(2, this.currentLineModeWidth);
            }            
        }

        public void HandleMouseMove(MouseEventArgs e)
        {
            if (this.mode == Mode.CornerMode && this.shiftIsPressed && this.firstMouseClick != null)
            {
                //Verschiebe nur so dass beim aufgezogenen Rechteck width=height gilt
                Vector2D p1 = new Vector2D(this.firstMouseClick.Value.X, this.firstMouseClick.Value.Y);
                Vector2D p2 = new Vector2D(e.X, e.Y);

                Vector2D dir = (p2 - p1).SignZeroIsOne().Normalize();
                Vector2D p3 = p1 + dir * ((p2 - p1) * dir);
                this.currentMousePosition = new Point(p3.Xi, p3.Yi);
            }
            else
            {
                this.currentMousePosition = new Point(e.X, e.Y);
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
            //Es wird gerade ein neues Rechteck aufgezogen
            if (this.firstMouseClick != null)
            {
                if (this.mode == Mode.CornerMode)
                {
                    var r = GetAxialAlignedRectangle(this.firstMouseClick.Value, this.currentMousePosition);
                    panel.DrawRectangle(Pens.Black, r.X, r.Y, r.Width, r.Height);
                }
                if (this.mode == Mode.LineMode)
                {
                    var r = GetRotatedRectangle(this.firstMouseClick.Value, this.currentMousePosition);

                    //panel.DrawFillRectangle(Color.Black, r.Center.Xi, r.Center.Yi, r.Size.Xi, r.Size.Yi, r.AngleInDegree);

                    Vector2D[] points = new Vector2D[]
                    {
                        Vector2D.RotatePointAroundPivotPoint(r.Center, new Vector2D(r.Center.X + r.Size.X / 2, r.Center.Y + r.Size.Y / 2), r.AngleInDegree),
                        Vector2D.RotatePointAroundPivotPoint(r.Center, new Vector2D(r.Center.X - r.Size.X / 2, r.Center.Y + r.Size.Y / 2), r.AngleInDegree),
                        Vector2D.RotatePointAroundPivotPoint(r.Center, new Vector2D(r.Center.X - r.Size.X / 2, r.Center.Y - r.Size.Y / 2), r.AngleInDegree),
                        Vector2D.RotatePointAroundPivotPoint(r.Center, new Vector2D(r.Center.X + r.Size.X / 2, r.Center.Y - r.Size.Y / 2), r.AngleInDegree),
                    };
                    panel.DrawPolygon(Pens.Black, points.ToList());
                }
            }
        }

        private Rectangle GetAxialAlignedRectangle(Point p1, Point p2)
        {
            Point min = new Point(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y));
            Point max = new Point(Math.Max(p1.X, p2.X), Math.Max(p1.Y, p2.Y));
            return new Rectangle(min.X, min.Y, (max.X - min.X) + 1, (max.Y - min.Y) + 1);
        }

        struct RotatedRectangle
        {
            public Vector2D Center;
            public Vector2D Size;
            public float AngleInDegree;
        }

        private RotatedRectangle GetRotatedRectangle(Point p1, Point p2)
        {
            var r = GetAxialAlignedRectangle(p1, p2);
            Vector2D center = new Vector2D(r.X + r.Width / 2, r.Y + r.Height / 2);
            Vector2D p1ToP2 = new Vector2D(p2.X, p2.Y) - new Vector2D(p1.X, p1.Y);
            float angle = Vector2D.Angle360(new Vector2D(1, 0), p1ToP2.Normalize());

            return new RotatedRectangle()
            {
                Center = center,
                Size = new Vector2D(p1ToP2.Length(), this.currentLineModeWidth),
                AngleInDegree = angle
            };
        }

        public FunctionHelpText GetHelpText()
        {
            if (this.mode == Mode.CornerMode)
            {
                return new FunctionHelpText()
                {
                    Headline = "Corners Mode",
                    Values = new[]
                    {
                        "CLICK: you figure it out",
                        "TAB: change mode",
                        "HOLD SHIFT: Width=Height",
                        "ESC: exit"
                    }
                };
            }else
            {
                return new FunctionHelpText()
                {
                    Headline = "Line Mode",
                    Values = new[]
                    {
                        "CLICK: you figure it out",
                        "TAB: change mode",
                        "MOUSE WHEEL: scale width",
                        "HOLD SHIFT: increase scale speed",
                        "ESC: exit"
                    }
                };  
            }
        }

        public void Dispose()
        {
        }
    }
}
