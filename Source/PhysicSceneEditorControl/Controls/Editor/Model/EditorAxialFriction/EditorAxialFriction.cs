using GraphicPanels;
using PhysicSceneEditorControl.Controls.AxialFriction;
using PhysicSceneEditorControl.Controls.Editor.Model.EditorShape;
using RigidBodyPhysics.ExportData.AxialFriction;
using RigidBodyPhysics.MathHelper;

namespace PhysicSceneEditorControl.Controls.Editor.Model.EditorAxialFriction
{
    internal class EditorAxialFriction : IEditorAxialFriction
    {
        private Vec2D r1;
        private Vec2D forceDirection;

        private Vec2D anchorWorldPosition;
        private Vec2D worldForceDirection;

        public IEditorShape Body { get; private set; }
        public Color Backcolor { get; set; } = Color.Transparent;
        public Pen BorderPen { get; set; } = Pens.Blue;
        public AxialFrictionPropertyViewModel Properties { get; set; }

        //Neu anlegen
        public EditorAxialFriction(IEditorShape body, Vec2D r1, Vec2D forceDirection)
        {
            this.Body = body;
            this.r1 = r1;
            this.forceDirection = forceDirection;

            this.Properties = new AxialFrictionPropertyViewModel();

            UpdateAfterMovingBodys();
        }

        //Aus Datei einladen
        public EditorAxialFriction(AxialFrictionExportData ctor, List<IEditorShape> shapes)
        {
            this.Body = shapes[ctor.BodyIndex];
            this.r1 = ctor.R1;
            this.forceDirection = ctor.ForceDirection;

            this.Properties = new AxialFrictionPropertyViewModel()
            {
                Friction = ctor.Friction,
            };
            UpdateAfterMovingBodys();
        }

        public void Draw(GraphicPanel2D panel) //Zeichnet die Editor-Daten
        {
            if (this.Backcolor != Color.Transparent)
            {
                DrawStick(panel, this.anchorWorldPosition, this.worldForceDirection, new Pen(this.Backcolor, 5));
            }

            DrawStick(panel, this.anchorWorldPosition, this.worldForceDirection, this.BorderPen);
        }

        public static void DrawStick(GraphicPanel2D panel, Vec2D position, Vec2D direction, Pen pen)
        {
            float r = 25;
            var p1 = position - direction * r;
            var p2 = position + direction * r;
            panel.DrawLine(pen, p1.ToGrx(), p2.ToGrx());

            int count = 5;
            float l = 10;
            Vec2D normal = direction.Spin90();
            for (int i=0;i<=count;i++)
            {
                float f = (float)i / count;
                var p = (1-f) * p1 + f * p2;
                panel.DrawLine(pen, (p - normal * l).ToGrx(), (p + normal * l).ToGrx());
            }
        }

        public IExportAxialFriction GetExportData(List<IEditorShape> bodies)
        {
            return new AxialFrictionExportData()
            {
                BodyIndex = bodies.IndexOf(this.Body),
                R1 = this.r1,
                ForceDirection = this.forceDirection,
                Friction = this.Properties.Friction,
            };
        }
        public bool IsPointInside(Vec2D position)
        {
            float r = 25;
            var p1 = this.anchorWorldPosition - this.worldForceDirection * r;
            var p2 = this.anchorWorldPosition + this.worldForceDirection * r;
            return EditorShapeHelper.IsPointAboveLine(p1, p2, position);
        }

        public void UpdateAfterMovingBodys()
        {
            this.anchorWorldPosition = EditorShapeHelper.LocalBodyDirectionToWorldPosition(this.Body, this.r1);
            this.worldForceDirection = EditorShapeHelper.LocalBodyDirectionToWorldDirection(this.Body, this.forceDirection).Normalize();
        }
    }
}
