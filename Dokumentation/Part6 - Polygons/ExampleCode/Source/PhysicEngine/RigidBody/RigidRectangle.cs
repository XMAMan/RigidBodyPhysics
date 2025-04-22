using PhysicEngine.CollisionDetection.NearPhase;
using PhysicEngine.ExportData.RigidBody;
using PhysicEngine.MathHelper;
using PhysicEngine.MaxForceTracking;

namespace PhysicEngine.RigidBody
{
    internal class RigidRectangle : IRigidBody, ICollidableRectangle, IPublicRigidRectangle, IBeamForceTracker, IBreakableBody
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

            this.beamDirectionLocal = size.X > size.Y ? (this.vertexLocal[1] - this.vertexLocal[0]) : (this.vertexLocal[3] - this.vertexLocal[0]);
            this.inverseBeamLength = 1.0f / this.beamDirectionLocal.Length();
            this.beamDirectionLocal /= this.beamDirectionLocal.Length();
            this.rectangleIsBeam = size.X > size.Y*2 ||size.Y > size.X*2;

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
            this.CollisionCategory = data.CollisionCategory;
            this.BreakWhenMaxPushPullForceIsReached = data.BreakWhenMaxPushPullForceIsReached;
            this.MaxPushPullForce = data.MaxPushPullForce;
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

            this.beamDirection = this.RotateToWorld * this.beamDirectionLocal;
        }
        public Matrix2x2 RotateToWorld { get; private set; }
        #endregion

        #region ICollidable
        public bool IsNotMoveable { get => this.InverseMass == 0; }
        public CollidableType TypeId { get; } = CollidableType.Rectangle;
        public List<ICollidable> CollideExcludeList { get; } = new List<ICollidable>();
        public int CollisionCategory { get; private set; } = 0;
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
                CollisionCategory = this.CollisionCategory,
                BreakWhenMaxPushPullForceIsReached = this.BreakWhenMaxPushPullForceIsReached,
                MaxPushPullForce = this.MaxPushPullForce,
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

        #region IBeamForceTracker
        private bool rectangleIsBeam = false; //Nur wenn das Rechteck 5 mal so lang wie hoch ist gilt es als Stab
        private Vec2D beamDirectionLocal; //Wenn das Rechteck wie ein Stab aussieht, dann zeigt dieser Vektor in Stabrichtung
        private Vec2D beamDirection;
        private float inverseBeamLength;
        private float forceOnLeftBeam = 0;
        private float forceOnRightBeam = 0;
        public void ResetTrackForce()
        {
            this.forceOnLeftBeam = 0;
            this.forceOnRightBeam = 0;
        }
        public void AddTrackForce(Vec2D forcePosition, Vec2D forceDirection)
        {
            if (this.InverseMass == 0 || this.rectangleIsBeam == false) return; //Tracke die die Kräfte, wenn das Rechteck eine Stabform hat

            Vec2D d = forcePosition - this.Vertex[0];
            float f = (d * this.beamDirection) * this.inverseBeamLength; //f=0 -> Kraft wirkt an linker Balkenecke; 1=Kraft wirkt an rechter Balkenecke
            float forceInBeamDirection = forceDirection * this.beamDirection;

            this.forceOnLeftBeam += (1 - f) * forceInBeamDirection;
            this.forceOnRightBeam += f * forceInBeamDirection;
        }
        public float GetPushPullForce()
        {
            return this.forceOnLeftBeam - this.forceOnRightBeam;
        }
        #endregion

        #region IPublicRigidRectangle
        public bool BreakWhenMaxPushPullForceIsReached { get; }
        public float MaxPushPullForce { get; }
        #endregion
    }
}
