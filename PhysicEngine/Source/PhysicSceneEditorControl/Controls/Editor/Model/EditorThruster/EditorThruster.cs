using GraphicPanels;
using PhysicSceneEditorControl.Controls.Editor.Model.EditorShape;
using PhysicSceneEditorControl.Controls.ThrusterProperty;
using RigidBodyPhysics.ExportData.Thruster;
using RigidBodyPhysics.MathHelper;

namespace PhysicSceneEditorControl.Controls.Editor.Model.EditorThruster
{
    internal class EditorThruster : IEditorThruster
    {
        private Vec2D r1;
        private Vec2D forceDirection;

        private Vec2D anchorWorldPosition;
        private Vec2D worldForceDirection;

        public IEditorShape Body { get; private set; }
        public Color Backcolor { get; set; } = Color.Transparent;
        public Pen BorderPen { get; set; } = Pens.Blue;
        public ThrusterPropertyViewModel Properties { get; set; }

        //Neu anlegen
        public EditorThruster(IEditorShape body, Vec2D r1, Vec2D forceDirection)
        {
            this.Body = body;
            this.r1 = r1;
            this.forceDirection = forceDirection;

            this.Properties = new ThrusterPropertyViewModel();

            UpdateAfterMovingBodys();
        }

        //Aus Datei einladen
        public EditorThruster(ThrusterExportData ctor, List<IEditorShape> shapes)
        {
            this.Body = shapes[ctor.BodyIndex];
            this.r1 = ctor.R1;
            this.forceDirection = ctor.ForceDirection;

            this.Properties = new ThrusterPropertyViewModel()
            {
                ForceLength = ctor.ForceLength,
                IsEnabled = ctor.IsEnabled,
            };
            UpdateAfterMovingBodys();
        }

        public void Draw(GraphicPanel2D panel) //Zeichnet die Editor-Daten
        {
            if (this.Backcolor != Color.Transparent)
            {
                DrawArrow(panel, this.anchorWorldPosition, this.worldForceDirection, new Pen(this.Backcolor, 5));
            }

            DrawArrow(panel, this.anchorWorldPosition, this.worldForceDirection, this.BorderPen);
        }

        public static void DrawArrow(GraphicPanel2D panel, Vec2D position, Vec2D direction, Pen pen)
        {
            float r = 50;
            var v1 = GraphicMinimal.Vector2D.GetV2FromAngle360(direction.ToGrx(), 45 + 90);
            var v2 = GraphicMinimal.Vector2D.GetV2FromAngle360(direction.ToGrx(), -45 - 90);

            panel.DrawLine(pen, (position - direction * r).ToGrx(), position.ToGrx());
            panel.DrawLine(pen, position.ToGrx(), position.ToGrx() + v1 * (r / 3));
            panel.DrawLine(pen, position.ToGrx(), position.ToGrx() + v2 * (r / 3));
        }

        public IExportThruster GetExportData(List<IEditorShape> bodies)
        {
            return new ThrusterExportData()
            {
                BodyIndex = bodies.IndexOf(this.Body),
                R1 = this.r1,
                ForceDirection = this.forceDirection,
                ForceLength = this.Properties.ForceLength,
                IsEnabled = this.Properties.IsEnabled
            };
        }
        public bool IsPointInside(Vec2D position)
        {
            float r = 50;
            var v1 = GraphicMinimal.Vector2D.GetV2FromAngle360(this.worldForceDirection.ToGrx(), 45 + 90).ToPhx();
            var v2 = GraphicMinimal.Vector2D.GetV2FromAngle360(this.worldForceDirection.ToGrx(), -45 - 90).ToPhx();

            if (EditorShapeHelper.IsPointAboveLine(this.anchorWorldPosition - this.worldForceDirection * r, this.anchorWorldPosition, position)) return true;
            if (EditorShapeHelper.IsPointAboveLine(this.anchorWorldPosition + v1 * (r / 3), this.anchorWorldPosition, position)) return true;
            if (EditorShapeHelper.IsPointAboveLine(this.anchorWorldPosition + v2 * (r / 3), this.anchorWorldPosition, position)) return true;

            return false;
        }

        public void UpdateAfterMovingBodys()
        {
            this.anchorWorldPosition = EditorShapeHelper.LocalBodyDirectionToWorldPosition(this.Body, this.r1);
            this.worldForceDirection = EditorShapeHelper.LocalBodyDirectionToWorldDirection(this.Body, this.forceDirection).Normalize();
        }
    }
}
