using EditorControl.ViewModel;
using GraphicPanels;
using PhysicEngine.ExportData.RigidBody;
using PhysicEngine.MathHelper;

namespace EditorControl.Model.EditorShape
{
    internal class EditorRectangle : IEditorShape
    {
        private Vec2D size;

        public EditorRectangle(Vec2D center, Vec2D size, float angleInDegree)
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
            this.Properties.CollisionCategory = ctor.CollisionCategory;

            var p = (RectanglePropertyViewModel)this.Properties;
            p.BreakWhenMaxPushPullForceIsReached = ctor.BreakWhenMaxPushPullForceIsReached;
            p.MaxPushPullForce = ctor.MaxPushPullForce;
        }

        public Color Backcolor { get; set; } = Color.Transparent;
        public Pen BorderPen { get; set; } = Pens.Black;
        public ShapePropertyViewModel Properties { get; set; } = new RectanglePropertyViewModel();
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
            this.size *= size;
        }
        public void Draw(GraphicPanel2D panel) //Zeichnet die Editor-Daten
        {
            Vec2D[] points = GetCornerPoints();

            if (this.Backcolor != Color.Transparent)
                panel.DrawFillPolygon(this.Backcolor, points.ToGrx().ToList());
            //panel.DrawFillPolygon("#FF0000", points.ToGrx().ToList(), false, Color.FromArgb(128, 255, 255, 255));

            panel.DrawPolygon(this.BorderPen, points.ToGrx().ToList());
        }

        private Vec2D[] GetCornerPoints()
        {
            return new Vec2D[]
                        {
                Vec2D.RotatePointAroundPivotPoint(this.Center, new Vec2D(this.Center.X + this.size.X / 2, this.Center.Y + this.size.Y / 2), this.AngleInDegree),
                Vec2D.RotatePointAroundPivotPoint(this.Center, new Vec2D(this.Center.X - this.size.X / 2, this.Center.Y + this.size.Y / 2), this.AngleInDegree),
                Vec2D.RotatePointAroundPivotPoint(this.Center, new Vec2D(this.Center.X - this.size.X / 2, this.Center.Y - this.size.Y / 2), this.AngleInDegree),
                Vec2D.RotatePointAroundPivotPoint(this.Center, new Vec2D(this.Center.X + this.size.X / 2, this.Center.Y - this.size.Y / 2), this.AngleInDegree),
                        };
        }

        public IExportRigidBody GetExportData()
        {
            var p = (RectanglePropertyViewModel)this.Properties;

            return new RectangleExportData()
            {
                Center = this.Center,
                Size = this.size,
                AngleInDegree = this.AngleInDegree,
                Velocity = new Vec2D(p.VelocityX, p.VelocityY),
                AngularVelocity = p.AngularVelocity,
                MassData = new MassData(p.MassType1, p.Mass, p.Density),
                Friction = p.Friction,
                Restituion = p.Restituion,
                CollisionCategory = p.CollisionCategory,
                BreakWhenMaxPushPullForceIsReached = p.BreakWhenMaxPushPullForceIsReached,
                MaxPushPullForce = p.MaxPushPullForce,
            };
        }

        public bool IsPointInside(Vec2D position)
        {
            Vec2D[] points = GetCornerPoints();
            for (int i = 0; i < points.Length; i++)
            {
                Vec2D edge = (points[(i + 1) % points.Length] - points[i]).Normalize();
                bool isInside = edge * (position - points[i]) > 0;
                if (isInside == false) return false;
            }

            return true;
        }

        public float GetArea()
        {
            return this.size.X * this.size.Y;
        }

        public BoundingBox GetBoundingBox()
        {
            Vec2D[] points = GetCornerPoints();
            return new BoundingBox(new Vec2D(points.Min(x => x.X), points.Min(x => x.Y)),
                new Vec2D(points.Max(x => x.X), points.Max(x => x.Y)));
        }

        public Vec2D[] GetAnchorPoints()
        {
            return new Vec2D[]
                        {
                this.Center,
                Vec2D.RotatePointAroundPivotPoint(this.Center, new Vec2D(this.Center.X + this.size.X / 2, this.Center.Y), this.AngleInDegree),
                Vec2D.RotatePointAroundPivotPoint(this.Center, new Vec2D(this.Center.X - this.size.X / 2, this.Center.Y), this.AngleInDegree),
                Vec2D.RotatePointAroundPivotPoint(this.Center, new Vec2D(this.Center.X, this.Center.Y + this.size.Y / 2), this.AngleInDegree),
                Vec2D.RotatePointAroundPivotPoint(this.Center, new Vec2D(this.Center.X, this.Center.Y - this.size.Y / 2), this.AngleInDegree),

                Vec2D.RotatePointAroundPivotPoint(this.Center, new Vec2D(this.Center.X + this.size.X / 2, this.Center.Y + this.size.Y / 2), this.AngleInDegree),
                Vec2D.RotatePointAroundPivotPoint(this.Center, new Vec2D(this.Center.X - this.size.X / 2, this.Center.Y + this.size.Y / 2), this.AngleInDegree),
                Vec2D.RotatePointAroundPivotPoint(this.Center, new Vec2D(this.Center.X - this.size.X / 2, this.Center.Y - this.size.Y / 2), this.AngleInDegree),
                Vec2D.RotatePointAroundPivotPoint(this.Center, new Vec2D(this.Center.X + this.size.X / 2, this.Center.Y - this.size.Y / 2), this.AngleInDegree),
                        };
        }
    }
}
