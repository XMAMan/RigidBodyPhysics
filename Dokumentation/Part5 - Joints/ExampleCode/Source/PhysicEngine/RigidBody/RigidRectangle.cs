using PhysicEngine.CollisionDetection.NearPhase;
using PhysicEngine.ExportData.RigidBody;
using PhysicEngine.MathHelper;

namespace PhysicEngine.RigidBody
{
    class RigidRectangle : IRigidBody, ICollidableRectangle, IPublicRigidRectangle
    {
        private readonly MassData massData; //Wird für die ExportFunktion benötigt
        #region IRigidBody
        public Vec2D Center { get; private set; } //Position of the Center of gravity
        public float Angle { get; private set; } //Oriantation around the Z-Aches with rotationpoint=Center [0..2PI]
        public Vec2D Velocity { get; set; } //Velocity from the Center-Point
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
        public Vec2D[] Vertex { get; private set; }
        private Vec2D[] vertexLocal;

        //0--Top;1--Right;2--Bottom;3--Left
        public Vec2D[] FaceNormal { get; private set; }
        #endregion

        #region IForceable
        public Vec2D Force { get; set; }
        public float Torque { get; set; }
        #endregion

        public Vec2D Size { get; private set; }

        public RigidRectangle(Vec2D center, Vec2D size, float angle, MassData massData)
        {
            this.massData = massData;
            this.Center = center;
            this.Angle = 0; //Hier darf ich nicht angle sondenr 0 zuweisen, weil ich mit Rotate(angle) am Ende von dieser Funktion dann den richtigen Wert einstelle
            this.Velocity = new Vec2D(0, 0);
            this.AngularVelocity = 0;
            float mass = massData.GetMass(size.X * size.Y);
            this.InverseMass = float.MaxValue == mass ? 0 : 1 / mass;
            this.InverseInertia = this.InverseMass == 0 ? 0 : 1.0f / (mass * (size.X * size.X + size.Y * size.Y) / 12f);
            this.Force = new Vec2D(0, 0);
            this.Torque = 0;

            this.Radius = (float)Math.Sqrt(size.X * size.X + size.Y * size.Y) / 2;

            this.Size = size;

            this.vertexLocal = new Vec2D[]
            {
                new Vec2D(-size.X / 2, -size.Y / 2), //TopLeft
                new Vec2D(+size.X / 2, -size.Y / 2), //TopRight
                new Vec2D(+size.X / 2, +size.Y / 2), //BottomRight
                new Vec2D(-size.X / 2, +size.Y / 2), //BottomLeft
            };

            this.Vertex = new Vec2D[this.vertexLocal.Length];

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
            this.FaceNormal = new Vec2D[]
            {
                (this.Vertex[1] - this.Vertex[2]).Normalize(), //Top
                (this.Vertex[2] - this.Vertex[3]).Normalize(), //Right
                (this.Vertex[3] - this.Vertex[0]).Normalize(), //Bottom
                (this.Vertex[0] - this.Vertex[1]).Normalize(), //Left
            };
        }

        #region IMoveable
        public void MoveCenter(Vec2D v)
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

            this.RotateToWorld = Matrix2x2.Rotate(this.Angle);
            for (int i = 0; i < this.vertexLocal.Length; i++)
            {
                this.Vertex[i] = this.Center + this.RotateToWorld * this.vertexLocal[i];
            }

            UpdateFaceNormal();
        }
        public Matrix2x2 RotateToWorld { get; private set; }
        #endregion

        #region ICollidable
        public CollisionInfo[] CollideWith(ICollidable collidable)
        {
            return NearPhaseTests.RectangleOther(this, collidable);
        }
        public List<ICollidable> CollideExcludeList { get; } = new List<ICollidable>();
        #endregion

        #region IExportable
        public IExportRigidBody GetExportData()
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

        #region IClickable
        public bool IsPointInside(Vec2D position)
        {
            Vec2D[] points = this.Vertex;
            for (int i = 0; i < points.Length; i++)
            {
                Vec2D edge = (points[(i + 1) % points.Length] - points[i]).Normalize();
                bool isInside = edge * (position - points[i]) > 0;
                if (isInside == false) return false;
            }

            return true;
        }
        #endregion
    }
}
