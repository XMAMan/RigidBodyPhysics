using GraphicPanels;
using PhysicSceneEditorControl.Controls.Editor.Model.EditorShape;
using PhysicSceneEditorControl.Controls.Editor.Model.ShapeExporter;
using RigidBodyPhysics.MathHelper;

namespace PhysicSceneEditorControl.Controls.Editor.Model.Function.Shapes
{
    internal class CloneShapeFunction : DummyFunction, IFunction
    {
        private List<IEditorShape> shapes = null;
        private IEditorShape selectedShape = null;
        private IEditorShape clone = null;
        private Vec2D centerToMouseDown = null;
        private bool shiftIsPressed = false;
        private Vec2D mouseClickPosition = null;

        public override IFunction Init(FunctionData functionData)
        {
            this.shapes = functionData.Shapes;
            return this;
        }

        public override void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
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
            //Es wurde noch kein Clone erzeugt
            if (clone == null)
            {
                if (selectedShape != null)
                {
                    //Erzeuge Kopie
                    clone = ExportHelper.ExportToEditorShape(selectedShape.GetExportData());

                    //Merke wo hingeklickt wurde
                    centerToMouseDown = new Vec2D(e.X, e.Y) - selectedShape.Center;
                    mouseClickPosition = new Vec2D(e.X, e.Y);
                }
            }
            else
            {
                //Clone an Mausposition einfügen
                shapes.Add(clone);
                clone = null;
                centerToMouseDown = null;
                mouseClickPosition = null;
            }
        }

        public override void HandleMouseMove(MouseEventArgs e)
        {
            //Wenn die Taste nicht unten ist schaue ob die Mause über einen Objekt ist
            if (centerToMouseDown == null)
            {
                selectedShape = null;

                Vec2D mousePosition = new Vec2D(e.X, e.Y);
                this.selectedShape = ShapeHelper.GetShapeFromPoint(shapes, mousePosition);
            }

            //Wenn Objekt angeklickt wurde dann verschiebe seinen Clone
            if (clone != null && centerToMouseDown != null)
            {
                if (shiftIsPressed)
                {
                    //Verschiebe nur Horizontal/Vertikal
                    Vec2D p1 = mouseClickPosition;
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
                    clone.MoveTo(p3 - centerToMouseDown);
                }
                else
                {
                    clone.MoveTo(new Vec2D(e.X, e.Y) - centerToMouseDown);
                }

            }
        }

        public override void Draw(GraphicPanel2D panel)
        {
            if (clone != null)
                clone.Draw(panel);
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
            foreach (var shape in shapes)
                shape.Backcolor = Color.Transparent;
        }
    }
}
