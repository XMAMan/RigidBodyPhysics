using GraphicPanels;
using PhysicSceneEditorControl.Controls.Editor.Model.EditorShape;
using RigidBodyPhysics.MathHelper;
using WpfControls.Model;

namespace PhysicSceneEditorControl.Controls.Editor.Model.Function.Shapes
{
    //Idee für diese Art der Rechteckerzeugung: https://www.thebigcb.com/projects/CreatureCreator/
    internal class AddRectangleFunction : DummyFunction, IFunction
    {
        private enum Mode { CornerMode, LineMode }

        private List<IEditorShape> shapes = null;
        private Point? firstMouseClick = null;
        private Point currentMousePosition = new Point(0, 0);
        private Mode mode = Mode.CornerMode;
        private int currentLineModeWidth = 5;
        private bool shiftIsPressed = false;
        private MouseGrid mouseGrid;

        public override IFunction Init(FunctionData functionData)
        {
            this.shapes = functionData.Shapes;
            this.mouseGrid = functionData.MouseGrid;
            return this;
        }

        public override void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Tab)
                mode = mode == Mode.CornerMode ? Mode.LineMode : Mode.CornerMode;

            if (e.Key == System.Windows.Input.Key.LeftShift)
                shiftIsPressed = true;
        }

        public override void HandleKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.LeftShift)
                shiftIsPressed = false;
        }

        public override void HandleMouseClick(MouseEventArgs e)
        {
            //Beginne ein neues Rechteck aufzuziehen
            if (firstMouseClick == null)
                firstMouseClick = this.mouseGrid.SnapMouse(new GraphicMinimal.Vector2D(e.X, e.Y)).ToPoint();
            else
            {
                var p1 = this.mouseGrid.SnapMouse(firstMouseClick.Value.ToGrx()).ToPoint();
                var p2 = this.mouseGrid.SnapMouse(currentMousePosition.ToGrx()).ToPoint();

                //Erzeuge ein neues Rechteck
                if (mode == Mode.CornerMode)
                {

                    var r = GetAxialAlignedRectangle(p1, p2);
                    var recShape = new EditorRectangle(new Vec2D(r.X + r.Width / 2, r.Y + r.Height / 2), new Vec2D(r.Width, r.Height), 0);
                    shapes.Add(recShape);
                }
                else
                {
                    var r = GetRotatedRectangle(p1, p2);
                    var recShape = new EditorRectangle(r.Center, r.Size, r.AngleInDegree);
                    shapes.Add(recShape);
                }

                firstMouseClick = null;
            }
        }

        public override void HandleMouseWheel(MouseEventArgs e)
        {
            if (mode == Mode.LineMode)
            {
                if (shiftIsPressed == false)
                    currentLineModeWidth += e.Delta / 100;
                else
                    currentLineModeWidth += e.Delta / 10;

                currentLineModeWidth = Math.Max(2, currentLineModeWidth);
            }
        }

        public override void HandleMouseMove(MouseEventArgs e)
        {
            if (mode == Mode.CornerMode && shiftIsPressed && firstMouseClick != null)
            {
                //Verschiebe nur so dass beim aufgezogenen Rechteck width=height gilt
                Vec2D p1 = new Vec2D(firstMouseClick.Value.X, firstMouseClick.Value.Y);
                Vec2D p2 = new Vec2D(e.X, e.Y);

                Vec2D dir = (p2 - p1).SignZeroIsOne().Normalize();
                Vec2D p3 = p1 + dir * ((p2 - p1) * dir);
                currentMousePosition = new Point((int)p3.X, (int)p3.Y);
            }
            else
            {
                currentMousePosition = new Point(e.X, e.Y);
            }

        }

        public override void Draw(GraphicPanel2D panel)
        {
            //Es wird gerade ein neues Rechteck aufgezogen
            if (firstMouseClick != null)
            {
                if (mode == Mode.CornerMode)
                {
                    var r = GetAxialAlignedRectangle(firstMouseClick.Value, currentMousePosition);
                    panel.DrawRectangle(Pens.Black, r.X, r.Y, r.Width, r.Height);
                }
                if (mode == Mode.LineMode)
                {
                    var r = GetRotatedRectangle(firstMouseClick.Value, currentMousePosition);

                    //panel.DrawFillRectangle(Color.Black, r.Center.Xi, r.Center.Yi, r.Size.Xi, r.Size.Yi, r.AngleInDegree);

                    Vec2D[] points = new Vec2D[]
                    {
                        Vec2D.RotatePointAroundPivotPoint(r.Center, new Vec2D(r.Center.X + r.Size.X / 2, r.Center.Y + r.Size.Y / 2), r.AngleInDegree),
                        Vec2D.RotatePointAroundPivotPoint(r.Center, new Vec2D(r.Center.X - r.Size.X / 2, r.Center.Y + r.Size.Y / 2), r.AngleInDegree),
                        Vec2D.RotatePointAroundPivotPoint(r.Center, new Vec2D(r.Center.X - r.Size.X / 2, r.Center.Y - r.Size.Y / 2), r.AngleInDegree),
                        Vec2D.RotatePointAroundPivotPoint(r.Center, new Vec2D(r.Center.X + r.Size.X / 2, r.Center.Y - r.Size.Y / 2), r.AngleInDegree),
                    };
                    panel.DrawPolygon(Pens.Black, points.ToGrx().ToList());
                }
            }
        }

        private Rectangle GetAxialAlignedRectangle(Point p1, Point p2)
        {
            Point min = new Point(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y));
            Point max = new Point(Math.Max(p1.X, p2.X), Math.Max(p1.Y, p2.Y));
            return new Rectangle(min.X, min.Y, max.X - min.X + 1, max.Y - min.Y + 1);
        }

        struct RotatedRectangle
        {
            public Vec2D Center;
            public Vec2D Size;
            public float AngleInDegree;
        }

        private RotatedRectangle GetRotatedRectangle(Point p1, Point p2)
        {
            var r = GetAxialAlignedRectangle(p1, p2);
            Vec2D center = new Vec2D(r.X + r.Width / 2, r.Y + r.Height / 2);
            Vec2D p1ToP2 = new Vec2D(p2.X, p2.Y) - new Vec2D(p1.X, p1.Y);
            float angle = Vec2D.Angle360(new Vec2D(1, 0), p1ToP2.Normalize());

            return new RotatedRectangle()
            {
                Center = center,
                Size = new Vec2D(p1ToP2.Length(), currentLineModeWidth),
                AngleInDegree = angle
            };
        }

        public override FunctionHelpText GetHelpText()
        {
            if (mode == Mode.CornerMode)
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
            }
            else
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
    }
}
