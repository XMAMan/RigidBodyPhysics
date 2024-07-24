using EditorControl.Model.EditorShape;
using EditorControl.ViewModel.Joints;
using GraphicPanels;
using PhysicEngine.ExportData.Joints;
using PhysicEngine.MathHelper;

namespace EditorControl.Model.EditorJoint
{
    internal class EditorWeldJoint : IEditorJoint
    {
        public IEditorShape Body1 { get; }
        public IEditorShape Body2 { get; }
        public Color Backcolor { get; set; } = Color.Transparent;
        public Pen BorderPen { get; set; } = Pens.Blue;
        public Vec2D Center { get; private set; }
        public bool SupportsDefineLimit { get; } = false;

        public WeldJointPropertyViewModel Properties { get; }

        //Neu anlegen
        public EditorWeldJoint(IEditorShape body1, IEditorShape body2, Vec2D anchorWorldPosition)
        {
            this.Body1 = body1;
            this.Body2 = body2;

            this.Center = anchorWorldPosition;

            this.Properties = new WeldJointPropertyViewModel();
            UpdateAfterMovingBodys();
        }

        //Aus Datei einladen
        public EditorWeldJoint(WeldJointExportData ctor, List<IEditorShape> shapes)
        {
            this.Body1 = shapes[ctor.BodyIndex1];
            this.Body2 = shapes[ctor.BodyIndex2];
            this.Center = EditorShapeHelper.LocalBodyDirectionToWorldPosition(this.Body1, ctor.R1);

            this.Properties = new WeldJointPropertyViewModel()
            {
                CollideConnected = ctor.CollideConnected,
                Soft = new ViewModel.SoftPropertyViewModel(ctor.SoftData)
            };
            UpdateAfterMovingBodys();
        }

        public void MoveTo(Vec2D position)
        {
            this.Center = position;
            UpdateAfterMovingBodys();
        }
        public void UpdateAfterMovingBodys()
        {
        }

        public void Draw(GraphicPanel2D panel) //Zeichnet die Editor-Daten
        {
            if (this.Backcolor != Color.Transparent)
            {
                panel.DrawFillCircle(this.Backcolor, this.Center.ToGrx(), 5);
                DrawJoint(panel, new Pen(this.Backcolor, 5));
            }

            DrawJoint(panel, this.BorderPen);
        }

        private void DrawJoint(GraphicPanel2D panel, Pen pen)
        {
            int cornerCount = 7;
            float radius = 20;

            List<Vec2D> points = new List<Vec2D>();
            for (int i=0;i<cornerCount; i++)
            {
                points.Add(this.Center +  new Vec2D((float)Math.Cos(i / (float)cornerCount * 2 * Math.PI), (float)Math.Sin(i / (float)cornerCount * 2 * Math.PI)) * radius);

                panel.DrawLine(pen, this.Center.ToGrx(), points.Last().ToGrx());
            }

            panel.DrawPolygon(pen, points.Select(x => x.ToGrx()).ToList());
        }

        public IExportJoint GetExportData(List<IEditorShape> bodies)
        {
            return new WeldJointExportData()
            {
                BodyIndex1 = bodies.IndexOf(this.Body1),
                BodyIndex2 = bodies.IndexOf(this.Body2),
                R1 = EditorShapeHelper.GetLocalBodyDirection(this.Body1, this.Center),
                R2 = EditorShapeHelper.GetLocalBodyDirection(this.Body2, this.Center),
                CollideConnected = this.Properties.CollideConnected,
                SoftData = this.Properties.Soft.GetExportData()
            };
        }
        public bool IsPointInside(Vec2D position)
        {
            return (this.Center - position).Length() < 5;
        }
    }
}
