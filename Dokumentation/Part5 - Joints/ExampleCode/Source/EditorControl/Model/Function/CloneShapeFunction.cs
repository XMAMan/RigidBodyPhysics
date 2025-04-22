using EditorControl.Model.EditorJoint;
using EditorControl.Model.EditorShape;
using EditorControl.Model.ShapeExporter;
using GraphicPanels;
using PhysicEngine.MathHelper;

namespace EditorControl.Model.Function
{
    internal class CloneShapeFunction : DummyFunction, IFunction
    {
        private List<IEditorShape> shapes = null;
        private IEditorShape selectedShape = null;
        private IEditorShape clone = null;
        private Vec2D centerToMouseDown = null;
        private bool shiftIsPressed = false;
        private Vec2D mouseClickPosition = null;

        public override IFunction Init(List<IEditorShape> shapes, List<IEditorJoint> joints)
        {
            this.shapes = shapes;
            return this;
        }

        public override void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.LeftShift)
                this.shiftIsPressed = true;
        }

        public override void HandleKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.LeftShift)
                this.shiftIsPressed = false;
        }

        public override void HandleMouseClick(System.Windows.Forms.MouseEventArgs e)
        {
            //Es wurde noch kein Clone erzeugt
            if (this.clone == null)
            {
                if (this.selectedShape != null)
                {
                    //Erzeuge Kopie
                    this.clone = ExportHelper.ExportToEditorShape(this.selectedShape.GetExportData());

                    //Merke wo hingeklickt wurde
                    this.centerToMouseDown = new Vec2D(e.X, e.Y) - this.selectedShape.Center;
                    this.mouseClickPosition = new Vec2D(e.X, e.Y);
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

        public override void HandleMouseMove(MouseEventArgs e)
        {
            //Wenn die Taste nicht unten ist schaue ob die Mause über einen Objekt ist
            if (this.centerToMouseDown == null)
            {
                this.selectedShape = null;

                Vec2D mousePosition = new Vec2D(e.X, e.Y);
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
                    Vec2D p1 = this.mouseClickPosition;
                    Vec2D p2 = new Vec2D(e.X, e.Y);

                    Vec2D l = p2 - p1;

                    Vec2D p3 = null; //Korrigierte Mausposition

                    if (Math.Abs(l.X) > Math.Abs(l.Y))
                    {
                        //Verschiebe horizontal
                        p3 = new Vec2D(e.X, p1.Y);
                    }
                    else
                    {
                        //Verschiebe vertikal
                        p3 = new Vec2D(p1.X, e.Y);
                    }
                    this.clone.MoveTo(p3 - this.centerToMouseDown);
                }
                else
                {
                    this.clone.MoveTo(new Vec2D(e.X, e.Y) - this.centerToMouseDown);
                }

            }
        }

        public override void Draw(GraphicPanel2D panel)
        {
            if (this.clone != null)
                this.clone.Draw(panel);
        }

        public override FunctionHelpText GetHelpText()
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

        public override void Dispose()
        {
            foreach (var shape in this.shapes)
                shape.Backcolor = Color.Transparent;
        }
    }
}
