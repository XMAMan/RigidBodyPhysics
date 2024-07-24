using EditorControl.ViewModel;
using GraphicPanels;
using PhysicEngine.ExportData.RigidBody;
using PhysicEngine.MathHelper;

namespace EditorControl.Model.EditorShape
{
    internal class EditorPolygon : IEditorShape
    {
        private Vec2D[] pointsLocal; //Lokal mit Center als Bezugssystem
        private Vec2D[] points; 

        //Konstruktor vom Editor
        public EditorPolygon(Vec2D[] points)
        {
            this.Center = PolygonHelper.GetCenterOfMassFromPolygon(points);
            this.pointsLocal = points.Select(x => x - this.Center).ToArray();
            this.points = points;
            this.AngleInDegree = 0;
        }

        private Vec2D[] GetWorldPoints()
        {
            return this.pointsLocal.Select(x => Vec2D.RotatePointAroundPivotPoint(this.Center, this.Center + x, this.AngleInDegree)).ToArray();
        }

        //Konstruktor fürs Laden
        public EditorPolygon(PolygonExportData ctor)
        {
            ((PolygonPropertyViewModel)this.Properties).PolygonType = ctor.PolygonType;
            this.pointsLocal = ctor.Points;
            this.Center = ctor.Center;
            this.AngleInDegree = ctor.AngleInDegree;
            this.Properties.VelocityX = ctor.Velocity.X;
            this.Properties.VelocityY = ctor.Velocity.Y;
            this.Properties.AngularVelocity = ctor.AngularVelocity;
            this.Properties.MassType1 = ctor.MassData.Type;
            this.Properties.Mass = ctor.MassData.Mass;
            this.Properties.Density = ctor.MassData.Density;
            this.Properties.Friction = ctor.Friction;
            this.Properties.Restituion = ctor.Restituion;
            this.Properties.CollisionCategory = ctor.CollisionCategory;

            this.points = GetWorldPoints();
        }

        public Color Backcolor { get; set; } = Color.Transparent;
        public Pen BorderPen { get; set; } = Pens.Black;
        public ShapePropertyViewModel Properties { get; set; } = new PolygonPropertyViewModel();
        public Vec2D Center { get; private set; }
        public float AngleInDegree { get; private set; }
        public void MoveTo(Vec2D position)
        {
            this.Center = position;

            this.points = GetWorldPoints();
        }
        public void Rotate(float angleInDegree)
        {
            this.AngleInDegree += angleInDegree;

            this.points = GetWorldPoints();
        }

        public void Resize(float size)
        {
            this.pointsLocal = this.pointsLocal.Select(x=> x*size).ToArray();
            this.points = GetWorldPoints();
        }
        public void Draw(GraphicPanel2D panel) //Zeichnet die Editor-Daten
        {
            if (this.Backcolor != Color.Transparent)
                panel.DrawFillPolygon(this.Backcolor, this.points.Select(x => x.ToGrx()).ToList());

            panel.DrawPolygon(this.BorderPen, this.points.Select(x => x.ToGrx()).ToList());
        }

        public IExportRigidBody GetExportData()
        {
            var p = this.Properties as PolygonPropertyViewModel;

            return new PolygonExportData()
            {
                PolygonType = p.PolygonType,
                Points = this.pointsLocal,
                Center = this.Center,
                AngleInDegree = this.AngleInDegree,
                Velocity = new Vec2D(p.VelocityX, p.VelocityY),
                AngularVelocity = p.AngularVelocity,
                MassData = new MassData(p.MassType1, p.Mass, p.Density),
                Friction = p.Friction,
                Restituion = p.Restituion,
                CollisionCategory = p.CollisionCategory,
            };
        }

        public bool IsPointInside(Vec2D position)
        {
            return PolygonHelper.PointIsInsidePolygon(this.points.ToArray(), position);
        }

        public float GetArea()
        {
            return PolygonHelper.GetAreaFromPolygon(this.points.ToArray());
        }

        public BoundingBox GetBoundingBox()
        {
            return PolygonHelper.GetBoundingBoxFromPolygon(this.points);
        }

        public Vec2D[] GetAnchorPoints()
        {
            List<Vec2D> points = new List<Vec2D>();
            points.Add(this.Center);
            points.AddRange(this.points);
            return points.ToArray();
        }
    }
}
