using GraphicPanels;
using PhysicSceneEditorControl.Controls.Editor.Model.EditorShape;
using RigidBodyPhysics.MathHelper;
using WpfControls.Model;

namespace PhysicSceneEditorControl.Controls.Editor.Model.Function.Shapes
{
    //Erstellt ein Polygon, wo sich keine Kante mit einer anderen Kante schneidet; Wo jede Kantenlänge mindestens 5 Pixel groß ist und wo alle Punkte in CCW-Richtung liegen.
    internal class AddPolygonFunction : DummyFunction, IFunction
    {
        private static readonly float MinPointDistance = 5;

        private List<IEditorShape> shapes = null;
        private List<Vec2D> points = new List<Vec2D>();
        private Vec2D currentMousePosition = new Vec2D(0, 0);
        private bool currentLineIntersects = false; //Gibt es ein Schnittpunkt zwischen der Linie: points.Last-currentMousePosition und den Linine points[0..Lenght-2]
        private MouseGrid mouseGrid;
        private bool shiftIsPressed = false;

        public override IFunction Init(FunctionData functionData)
        {
            this.shapes = functionData.Shapes;
            this.mouseGrid = functionData.MouseGrid;
            return this;
        }

        private Vec2D GetP2WhichIsHorizontalVerticalToP1(Vec2D p1, Vec2D p2MousePosition)
        {
            //Verschiebe nur Horizontal/Vertikal
            Vec2D p2 = p2MousePosition;

            Vec2D l = p2 - p1;

            Vec2D p3 = null; //Korrigierte Mausposition

            if (Math.Abs(l.X) > Math.Abs(l.Y))
            {
                //Verschiebe horizontal
                p3 = new Vec2D(p2MousePosition.X, p1.Y);
            }
            else
            {
                //Verschiebe vertikal
                p3 = new Vec2D(p1.X, p2MousePosition.Y);
            }

            return p3;
        }

        public override void HandleMouseMove(MouseEventArgs e)
        {
            currentMousePosition = new Vec2D(e.X, e.Y);

            if (shiftIsPressed && points.Any())
            {
                currentMousePosition = GetP2WhichIsHorizontalVerticalToP1(points.Last(), currentMousePosition);
            }

            this.currentLineIntersects = false;
            for (int i = 0; i < points.Count - 2; i++)
            {
                if (PolygonHelper.IntersectLines(points[i], points[i + 1], points.Last(), currentMousePosition))
                {
                    this.currentLineIntersects = true;
                    break;
                }
            }

            if (this.points.Any() && (this.points.Last() - currentMousePosition).Length() < MinPointDistance)
                this.currentLineIntersects = true;
        }

        public override void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                CreatePolygon();
            }

            if (e.Key == System.Windows.Input.Key.LeftShift)
                shiftIsPressed = true;
        }

        public override void HandleKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.LeftShift)
                shiftIsPressed = false;
        }

        public override void HandleMouseClick(System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && this.currentLineIntersects == false)
            {
                var currentMousePosition = new Vec2D(e.X, e.Y);
                if (shiftIsPressed && points.Any())
                {
                    currentMousePosition = GetP2WhichIsHorizontalVerticalToP1(points.Last(), currentMousePosition);
                }

                var point = this.mouseGrid.SnapMouse(currentMousePosition.ToGrx()).ToPhx();
                points.Add(point);
            }

            if (e.Button == MouseButtons.Right)
            {
                CreatePolygon();
            }
        }

        private void CreatePolygon()
        {
            if (this.points.Count >= 3)
            {
                //Stelle sicher, dass das Polygon immer in CCW-Richtung angegeben ist
                Vec2D[] polygon = PolygonHelper.OrderPointsCCW(this.points.ToArray());

                this.shapes.Add(new EditorPolygon(polygon));
                this.points.Clear();

                //So habe ich geprüft, ob die PolygonHelper-Klasse korrekt ist:
                //var poly = this.shapes.Last();

                //var img = ShapeHelper.ConvertToBitmap(poly);
                //img.Save("Poly.bmp");
                //img.Dispose();

                //float area1 = ShapeHelper.GetArea(poly);
                //float area2 = PolygonHelper.GetAreaFromPolygon(polygon);
                //float area3 = PolygonHelper.GetSignedAreaFromPolygon(polygon);

                //Vec2D center1 = ShapeHelper.CenterOfGravity(poly);
                //Vec2D center2 = PolygonHelper.GetCenterOfMassFromPolygon(polygon);

                //float inertia1 = ShapeHelper.GetInertia(poly);
                //float inertia2 = PolygonHelper.GetInertiaFromPolygon(poly.Properties.Density, polygon);
            }
        }

        public override void Draw(GraphicPanel2D panel)
        {
            if (this.points.Any())
            {
                for (int i = 0; i < this.points.Count - 1; i++)
                {
                    panel.DrawLine(new Pen(Color.Black, 2), this.points[i].ToGrx(), this.points[i + 1].ToGrx());
                }

                panel.DrawLine(new Pen(this.currentLineIntersects ? Color.Red : Color.Black, 2), this.points.Last().ToGrx(), this.currentMousePosition.ToGrx());
            }
        }

        public override FunctionHelpText GetHelpText()
        {
            return new FunctionHelpText()
            {
                Headline = "Add Polygon",
                Values = new[]
                {
                    "LEFT CLICK: add vertex to polygon ",
                    "RIGHT CLICK: finish polygon",
                    "ENTER: finish polygon",
                    "ESC: exit"
                }
            };
        }
    }
}
