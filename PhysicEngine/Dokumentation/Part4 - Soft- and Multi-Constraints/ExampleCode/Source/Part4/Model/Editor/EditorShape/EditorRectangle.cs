using GraphicMinimal;
using GraphicPanels;
using Part4.ViewModel.Editor;
using PhysicEngine.ExportData.RigidBody;
using System.Drawing;
using System.Linq;

namespace Part4.Model.Editor.EditorShape
{
    class EditorRectangle : IEditorShape
    {
        private Vector2D size;

        public EditorRectangle(Vector2D center, Vector2D size, float angleInDegree)
        {
            this.Center = center;
            this.size = size;
            this.AngleInDegree = angleInDegree;
        }

        public EditorRectangle(RectangleExportData ctor)
        {
            this.Center = ctor.Center;
            this.size = ctor.Size;
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
        public Vector2D Center { get; private set; }
        public float AngleInDegree { get; private set; }
        public void MoveTo(Vector2D position)
        {
            this.Center = position;
        }
        public void Rotate(float angleInDegree)
        {
            this.AngleInDegree += angleInDegree;
        }
        public void Resize(float size)
        {
            this.size *= size;
        }
        public void Draw(GraphicPanel2D panel) //Zeichnet die Editor-Daten
        {
            Vector2D[] points = GetCornerPoints();

            if (this.Backcolor != Color.Transparent)
                panel.DrawFillPolygon(this.Backcolor, points.ToList());
            //panel.DrawFillPolygon("#FF0000", points.ToList(), false, Color.FromArgb(128, 255, 255, 255));

            panel.DrawPolygon(this.BorderPen, points.ToList());
        }

        private Vector2D[] GetCornerPoints()
        {
            return new Vector2D[]
                        {
                Vector2D.RotatePointAroundPivotPoint(this.Center, new Vector2D(this.Center.X + this.size.X / 2, this.Center.Y + this.size.Y / 2), this.AngleInDegree),
                Vector2D.RotatePointAroundPivotPoint(this.Center, new Vector2D(this.Center.X - this.size.X / 2, this.Center.Y + this.size.Y / 2), this.AngleInDegree),
                Vector2D.RotatePointAroundPivotPoint(this.Center, new Vector2D(this.Center.X - this.size.X / 2, this.Center.Y - this.size.Y / 2), this.AngleInDegree),
                Vector2D.RotatePointAroundPivotPoint(this.Center, new Vector2D(this.Center.X + this.size.X / 2, this.Center.Y - this.size.Y / 2), this.AngleInDegree),
                        };
        }

        public IExportRigidBody GetExportData()
        {
            var p = this.Properties;

            return new RectangleExportData()
            {
                Center = this.Center,
                Size = this.size,
                AngleInDegree = this.AngleInDegree,
                Velocity = new Vector2D(p.VelocityX, p.VelocityY),
                AngularVelocity = p.AngularVelocity,
                MassData = new MassData(p.MassType1, p.Mass, p.Density),
                Friction = p.Friction,
                Restituion = p.Restituion,
            };
        }

        private float ShapeArea()
        {
            return this.size.X * this.size.Y;
        }

        public bool IsPointInside(Vector2D position)
        {
            Vector2D[] points = GetCornerPoints();
            for (int i = 0; i < points.Length; i++)
            {
                Vector2D edge = (points[(i + 1) % points.Length] - points[i]).Normalize();
                bool isInside = edge * (position - points[i]) > 0;
                if (isInside == false) return false;
            }

            return true;
        }

        public BoundingBox GetBoundingBox()
        {
            Vector2D[] points = GetCornerPoints();
            return new BoundingBox(new Vector2D(points.Min(x => x.X), points.Min(x => x.Y)),
                new Vector2D(points.Max(x => x.X), points.Max(x => x.Y)));
        }
    }
}
