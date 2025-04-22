using EditorControl.ViewModel;
using GraphicPanels;
using PhysicEngine.ExportData.RigidBody;
using PhysicEngine.MathHelper;

namespace EditorControl.Model.EditorShape
{
    internal class EditorCircle : IEditorShape
    {
        private float radius;

        public EditorCircle(Vec2D center, float radius)
        {
            this.Center = center;
            this.radius = radius;
            this.AngleInDegree = 0;
        }

        public EditorCircle(CircleExportData ctor)
        {
            this.Center = ctor.Center;
            this.radius = ctor.Radius;
            this.AngleInDegree = ctor.AngleInDegree;
            this.Properties.VelocityX = ctor.Velocity.X;
            this.Properties.VelocityY = ctor.Velocity.Y;
            this.Properties.AngularVelocity = ctor.AngularVelocity;
            this.Properties.MassType1 = ctor.MassData.Type;
            this.Properties.Mass = ctor.MassData.Mass;
            this.Properties.Density = ctor.MassData.Density;
            this.Properties.Friction = ctor.Friction;
            this.Properties.Restituion = ctor.Restituion;
        }

        public Color Backcolor { get; set; } = Color.Transparent;
        public Pen BorderPen { get; set; } = Pens.Black;
        public ShapePropertyViewModel Properties { get; set; } = new ShapePropertyViewModel();
        public Vec2D Center { get; private set; }
        public float AngleInDegree { get; private set; }
        public void MoveTo(Vec2D position)
        {
            this.Center = position;
        }
        public void Rotate(float angleInDegree)
        {
            this.AngleInDegree += angleInDegree;
        }

        public void Resize(float size)
        {
            this.radius *= size;
        }
        public void Draw(GraphicPanel2D panel) //Zeichnet die Editor-Daten
        {
            if (this.Backcolor != Color.Transparent)
                panel.DrawFillCircle(this.Backcolor, this.Center.ToGrx(), this.radius);

            panel.DrawCircle(this.BorderPen, this.Center.ToGrx(), this.radius);

            Vec2D r = Vec2D.DirectionFromPhi(this.AngleInDegree / 180 * (float)Math.PI);
            panel.DrawLine(Pens.Black, this.Center.ToGrx(), (this.Center + r * this.radius).ToGrx());
        }

        public IExportRigidBody GetExportData()
        {
            var p = this.Properties;

            return new CircleExportData()
            {
                Center = this.Center,
                Radius = this.radius,
                AngleInDegree = this.AngleInDegree,
                Velocity = new Vec2D(p.VelocityX, p.VelocityY),
                AngularVelocity = p.AngularVelocity,
                MassData = new MassData(p.MassType1, p.Mass, p.Density),
                Friction = p.Friction,
                Restituion = p.Restituion,
            };
        }

        public bool IsPointInside(Vec2D position)
        {
            return (position - this.Center).Length() < this.radius;
        }

        public BoundingBox GetBoundingBox()
        {
            return new BoundingBox(new Vec2D(this.Center.X - this.radius, this.Center.Y - this.radius),
                new Vec2D(this.Center.X + this.radius, this.Center.Y + this.radius));
        }
    }
}
