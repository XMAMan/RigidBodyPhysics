using EditorControl.Model.EditorShape;
using EditorControl.ViewModel.Joints;
using GraphicPanels;
using PhysicEngine.ExportData.Joints;
using PhysicEngine.MathHelper;

namespace EditorControl.Model.EditorJoint
{
    internal class EditorWeldJoint : IEditorJoint
    {
        private Vec2D r1;
        private Vec2D r2;

        private Vec2D anchorWorldPosition1;
        private Vec2D anchorWorldPosition2;

        public IEditorShape Body1 { get; }
        public IEditorShape Body2 { get; }
        public Color Backcolor { get; set; } = Color.Transparent;
        public Pen BorderPen { get; set; } = Pens.Blue;        
        public bool SupportsDefineLimit { get; } = false;

        public WeldJointPropertyViewModel Properties { get; }

        //Neu anlegen
        public EditorWeldJoint(IEditorShape body1, IEditorShape body2, Vec2D r1, Vec2D r2)
        {
            this.Body1 = body1;
            this.Body2 = body2;

            this.r1 = r1;
            this.r2 = r2;


            this.Properties = new WeldJointPropertyViewModel();
            UpdateAfterMovingBodys();
        }

        //Aus Datei einladen
        public EditorWeldJoint(WeldJointExportData ctor, List<IEditorShape> shapes)
        {
            this.Body1 = shapes[ctor.BodyIndex1];
            this.Body2 = shapes[ctor.BodyIndex2];
            this.r1 = ctor.R1;
            this.r2 = ctor.R2;

            this.Properties = new WeldJointPropertyViewModel()
            {
                CollideConnected = ctor.CollideConnected,
                Soft = new ViewModel.SoftPropertyViewModel(ctor.SoftData),
                BreakWhenMaxForceIsReached = ctor.BreakWhenMaxForceIsReached,
                MaxForceToBreak = ctor.MaxForceToBreak,
            };
            UpdateAfterMovingBodys();
        }

        public void UpdateAfterMovingBodys()
        {
            this.anchorWorldPosition1 = EditorShapeHelper.LocalBodyDirectionToWorldPosition(this.Body1, this.r1);
            this.anchorWorldPosition2 = EditorShapeHelper.LocalBodyDirectionToWorldPosition(this.Body2, this.r2);
        }

        public void Draw(GraphicPanel2D panel) //Zeichnet die Editor-Daten
        {
            if (this.Backcolor != Color.Transparent)
            {
                panel.DrawFillCircle(this.Backcolor, this.anchorWorldPosition1.ToGrx(), 5);
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
                points.Add(this.anchorWorldPosition1 +  new Vec2D((float)Math.Cos(i / (float)cornerCount * 2 * Math.PI), (float)Math.Sin(i / (float)cornerCount * 2 * Math.PI)) * radius);

                panel.DrawLine(pen, this.anchorWorldPosition1.ToGrx(), points.Last().ToGrx());
            }

            panel.DrawPolygon(pen, points.Select(x => x.ToGrx()).ToList());
        }

        public IExportJoint GetExportData(List<IEditorShape> bodies)
        {
            return new WeldJointExportData()
            {
                BodyIndex1 = bodies.IndexOf(this.Body1),
                BodyIndex2 = bodies.IndexOf(this.Body2),
                R1 = this.r1,
                R2 = this.r2,
                CollideConnected = this.Properties.CollideConnected,
                SoftData = this.Properties.Soft.GetExportData(),
                BreakWhenMaxForceIsReached = this.Properties.BreakWhenMaxForceIsReached,
                MaxForceToBreak = this.Properties.MaxForceToBreak,
            };
        }
        public bool IsPointInside(Vec2D position)
        {
            return (this.anchorWorldPosition1 - position).Length() < 5;
        }
    }
}
