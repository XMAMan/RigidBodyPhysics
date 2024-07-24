using GraphicMinimal;
using GraphicPanels;
using Part3.ViewModel.Editor;
using PhysicEngine.ExportData;
using System;
using System.Drawing;

namespace Part3.Model.Editor.EditorShape
{
    class EditorCircle : IEditorShape
    {
        private float radius;
        private float angleInDegree;

        public EditorCircle(Vector2D center, float radius)
        {
            this.Center = center;
            this.radius = radius;
            this.angleInDegree = 0;
        }

        public EditorCircle(CircleExportData ctor)
        {
            this.Center = ctor.Center;
            this.radius = ctor.Radius;
            this.angleInDegree= ctor.AngleInDegree;
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
        public void MoveTo(Vector2D position)
        {
            this.Center = position;
        }
        public void Rotate(float angleInDegree)
        {
            this.angleInDegree += angleInDegree;
        }

        public void Resize(float size)
        {
            this.radius *= size;
        }
        public void Draw(GraphicPanel2D panel) //Zeichnet die Editor-Daten
        {
            if (this.Backcolor != Color.Transparent)
                panel.DrawFillCircle(this.Backcolor, this.Center, this.radius);

            panel.DrawCircle(this.BorderPen, this.Center, this.radius);

            Vector2D r = Vector2D.DirectionFromPhi(this.angleInDegree / 180 * (float)Math.PI);
            panel.DrawLine(Pens.Black, this.Center, this.Center + r * this.radius);
        }

        public IExportShape GetExportData()
        {
            var p = this.Properties;

            return new CircleExportData()
            {
                Center = this.Center,
                Radius = this.radius,
                AngleInDegree = this.angleInDegree,
                Velocity = new Vector2D(p.VelocityX, p.VelocityY),
                AngularVelocity = p.AngularVelocity,
                MassData = new MassData(p.MassType1, p.Mass, p.Density),
                Friction = p.Friction,
                Restituion = p.Restituion,
            };
        }

        private float ShapeArea()
        {
            return this.radius * this.radius * (float)Math.PI;
        }

        public bool IsPointInside(Vector2D position)
        {
            return (position - this.Center).Length() < this.radius;
        }
    }
}
