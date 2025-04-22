using GraphicMinimal;
using PhysicEngine.CollisionDetection.NearPhase;
using PhysicEngine.ExportData;

namespace PhysicEngine.RigidBody
{
    public class RigidRectangle : IRigidBody, ICollidableRectangle
    {
        private readonly MassData massData; //Wird für die ExportFunktion benötigt
        #region IRigidBody
        public Vector2D Center { get; private set; } //Position of the Center of gravity
        public float Angle { get; private set; } //Oriantation around the Z-Aches with rotationpoint=Center [0..2PI]
        public Vector2D Velocity { get; set; } //Velocity from the Center-Point
        public float AngularVelocity { get; set; }

        public float InverseMass { get; private set; } //1 / Mass
        public float InverseInertia { get; private set; }
        public float Restituion { get; private set; } = 1;
        public float Friction { get; private set; } = 1;
        #endregion


        #region IBoundingCircle
        public float Radius { get; private set; }
        #endregion

        #region ICollidableRectangle
        //0--TopLeft;1--TopRight;2--BottomRight;3--BottomLeft
        public Vector2D[] Vertex { get; private set; }

        //0--Top;1--Right;2--Bottom;3--Left
        public Vector2D[] FaceNormal { get; private set; }
        #endregion

        #region IForceable
        public Vector2D Force { get; set; }
        public float Torque { get; set; }
        #endregion

        public Vector2D Size { get; private set; }

        public RigidRectangle(Vector2D center, Vector2D size, float angle, MassData massData)
        {
            this.massData = massData;
            this.Center = center;
            this.Angle = 0; //Hier darf ich nicht angle sondenr 0 zuweisen, weil ich mit Rotate(angle) am Ende von dieser Funktion dann den richtigen Wert einstelle
            this.Velocity = new Vector2D(0, 0);
            this.AngularVelocity = 0;
            float mass = massData.GetMass(size.X * size.Y);
            this.InverseMass = float.MaxValue == mass ? 0 : 1 / mass;
            this.InverseInertia = this.InverseMass == 0 ? 0 : 1.0f / (mass * (size.X * size.X + size.Y * size.Y) / 12f);
            this.Force = new Vector2D(0, 0);
            this.Torque = 0;

            this.Radius = (float)Math.Sqrt(size.X * size.X + size.Y * size.Y) / 2;

            this.Size = size;

            this.Vertex = new Vector2D[]
            {
                new Vector2D(center.X - size.X / 2, center.Y - size.Y / 2), //TopLeft
                new Vector2D(center.X + size.X / 2, center.Y - size.Y / 2), //TopRight
                new Vector2D(center.X + size.X / 2, center.Y + size.Y / 2), //BottomRight
                new Vector2D(center.X - size.X / 2, center.Y + size.Y / 2), //BottomLeft
            };

            Rotate(angle);
        }

        public RigidRectangle(RectangleExportData data)
            :this(data.Center, data.Size, data.AngleInDegree / 180 * (float)Math.PI, data.MassData)
        {
            this.Velocity = data.Velocity;
            this.AngularVelocity = data.AngularVelocity;
            this.Friction = data.Friction;
            this.Restituion = data.Restituion;
        }

        private void UpdateFaceNormal()
        {
            this.FaceNormal = new Vector2D[]
            {
                (this.Vertex[1] - this.Vertex[2]).Normalize(), //Top
                (this.Vertex[2] - this.Vertex[3]).Normalize(), //Right
                (this.Vertex[3] - this.Vertex[0]).Normalize(), //Bottom
                (this.Vertex[0] - this.Vertex[1]).Normalize(), //Left
            };
        }

        #region IMoveable
        public void MoveCenter(Vector2D v)
        {
            for (int i=0; i<this.Vertex.Length; i++)
            {
                this.Vertex[i] += v;
            }

            this.Center += v;
        }
        public void Rotate(float angle)
        {
            this.Angle += angle;

            float angleInDegree = (float)(angle / (2 * Math.PI) * 360);
            for (int i = 0; i < this.Vertex.Length; i++)
            {
                this.Vertex[i] = Vector2D.RotatePointAroundPivotPoint(this.Center, this.Vertex[i], angleInDegree);
            }

            UpdateFaceNormal();
        }
        #endregion

        #region ICollidable
        public CollisionInfo[] CollideWith(ICollidable collidable)
        {
            return NearPhaseTests.RectangleOther(this, collidable);
        }
        #endregion

        #region IExportable
        public IExportShape GetExportData()
        {
            return new RectangleExportData()
            {
                Center = this.Center,
                Size = this.Size,
                AngleInDegree = (float)(this.Angle / Math.PI * 180),
                Velocity = this.Velocity,
                AngularVelocity = this.AngularVelocity,
                MassData = this.massData,
                Friction = this.Friction,
                Restituion = this.Restituion,
            };
        }
        #endregion
    }
}
