using EditorControl.Model.EditorShape;
using EditorControl.ViewModel;
using GraphicPanels;
using PhysicEngine.ExportData.RotaryMotor;
using PhysicEngine.MathHelper;

namespace EditorControl.Model.EditorRotaryMotor
{
    internal class EditorRotaryMotor : IEditorRotaryMotor
    {
        public IEditorShape Body { get; private set; }
        public Color Backcolor { get; set; } = Color.Transparent;
        public Pen BorderPen { get; set; } = Pens.Blue;
        public RotaryMotorPropertyViewModel Properties { get; set; }

        //Neu anlegen
        public EditorRotaryMotor(IEditorShape body)
        {
            this.Body = body;
            
            this.Properties = new RotaryMotorPropertyViewModel();

            UpdateAfterMovingBodys();
        }

        //Aus Datei einladen
        public EditorRotaryMotor(RotaryMotorExportData ctor, List<IEditorShape> shapes)
        {
            this.Body = shapes[ctor.BodyIndex];

            this.Properties = new RotaryMotorPropertyViewModel()
            {
                RotaryForce = ctor.RotaryForce,
                IsEnabled = ctor.IsEnabled
            };
            UpdateAfterMovingBodys();
        }

        public void Draw(GraphicPanel2D panel) //Zeichnet die Editor-Daten
        {
            if (this.Backcolor != Color.Transparent)
            {
                Draw(new Pen(this.Backcolor, 5), panel);
            }

            Draw(this.BorderPen, panel);
        }

        private void Draw(Pen pen, GraphicPanel2D panel)
        {
            panel.DrawCircleArc(pen, this.Body.Center.ToGrx(), 20, 30, 320, false);
            var p = Vec2D.RotatePointAroundPivotPoint(this.Body.Center, this.Body.Center + new Vec2D(20, 0), 320);
            var dir1 = Vec2D.RotatePointAroundPivotPoint(this.Body.Center, this.Body.Center + new Vec2D(20 + 10, 0 - 10), 320);
            var dir2 = Vec2D.RotatePointAroundPivotPoint(this.Body.Center, this.Body.Center + new Vec2D(20 - 10, 0 - 10), 320);

            panel.DrawLine(pen, p.ToGrx(), dir1.ToGrx());
            panel.DrawLine(pen, p.ToGrx(), dir2.ToGrx());
        }

        public IExportRotaryMotor GetExportData(List<IEditorShape> bodies)
        {
            return new RotaryMotorExportData()
            {
                BodyIndex = bodies.IndexOf(this.Body),
                RotaryForce = this.Properties.RotaryForce,
                IsEnabled = this.Properties.IsEnabled
            };
        }
        public bool IsPointInside(Vec2D position)
        {
            return (this.Body.Center - position).Length() < 20;
        }

        public void UpdateAfterMovingBodys()
        {
        }
    }
}
