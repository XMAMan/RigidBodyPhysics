using RigidBodyPhysics.CollisionDetection.NearPhase;
using RigidBodyPhysics.ExportData.RigidBody;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.MaxForceTracking;

namespace RigidBodyPhysics.RuntimeObjects.RigidBody
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
            Center = center;
            Angle = 0; //Hier darf ich nicht angle sondenr 0 zuweisen, weil ich mit Rotate(angle) am Ende von dieser Funktion dann den richtigen Wert einstelle
            Velocity = new Vec2D(0, 0);
            AngularVelocity = 0;
            Area = size.X * size.Y;
            float mass = massData.GetMass(Area);
            InverseMass = float.MaxValue == mass ? 0 : 1 / mass;
            InverseInertia = InverseMass == 0 ? 0 : 1.0f / (mass * (size.X * size.X + size.Y * size.Y) / 12f);
            Force = new Vec2D(0, 0);
            Torque = 0;

            Radius = (float)Math.Sqrt(size.X * size.X + size.Y * size.Y) / 2;

            Size = size;

            vertexLocal = new Vec2D[]
            {
                new Vec2D(-size.X / 2, -size.Y / 2), //TopLeft
                new Vec2D(+size.X / 2, -size.Y / 2), //TopRight
                new Vec2D(+size.X / 2, +size.Y / 2), //BottomRight
                new Vec2D(-size.X / 2, +size.Y / 2), //BottomLeft
            };

            beamDirectionLocal = size.X > size.Y ? vertexLocal[1] - vertexLocal[0] : vertexLocal[3] - vertexLocal[0];
            inverseBeamLength = 1.0f / beamDirectionLocal.Length();
            beamDirectionLocal /= beamDirectionLocal.Length();
            rectangleIsBeam = size.X > size.Y * 2 || size.Y > size.X * 2;

            Vertex = new Vec2D[vertexLocal.Length];

            Rotate(angle);
        }

        public RigidRectangle(RectangleExportData data)
            : this(data.Center, data.Size, data.AngleInDegree / 180 * (float)Math.PI, data.MassData)
        {
            Velocity = new Vec2D(data.Velocity);
            AngularVelocity = data.AngularVelocity;
            Friction = data.Friction;
            Restituion = data.Restituion;
            CollisionCategory = data.CollisionCategory;
            BreakWhenMaxPushPullForceIsReached = data.BreakWhenMaxPushPullForceIsReached;
            MaxPushPullForce = data.MaxPushPullForce;
        }

        private void UpdateFaceNormal()
        {
            FaceNormal = new Vec2D[]
            {
                (Vertex[1] - Vertex[2]).Normalize(), //Top
                (Vertex[2] - Vertex[3]).Normalize(), //Right
                (Vertex[3] - Vertex[0]).Normalize(), //Bottom
                (Vertex[0] - Vertex[1]).Normalize(), //Left
            };
        }

        #region IMoveable
        public void MoveCenter(Vec2D v)
        {
            for (int i = 0; i < Vertex.Length; i++)
            {
                Vertex[i] += v;
            }

            Center += v;
        }
        public void Rotate(float angle)
        {
            Angle += angle;

            RotateToWorld = Matrix2x2.Rotate(Angle);
            for (int i = 0; i < vertexLocal.Length; i++)
            {
                Vertex[i] = Center + RotateToWorld * vertexLocal[i];
            }

            UpdateFaceNormal();

            beamDirection = RotateToWorld * beamDirectionLocal;
        }
        public Matrix2x2 RotateToWorld { get; private set; }
        #endregion

        #region ICollidable
        public bool IsNotMoveable { get => InverseMass == 0; }
        public CollidableType TypeId { get; } = CollidableType.Rectangle;
        public List<ICollidable> CollideExcludeList { get; } = new List<ICollidable>();
        public int CollisionCategory { get; private set; } = 0;
        #endregion

        #region IExportable
        public IExportRigidBody GetExportData()
        {
            return new RectangleExportData()
            {
                Center = Center,
                Size = Size,
                AngleInDegree = (float)(Angle / Math.PI * 180),
                Velocity = Velocity,
                AngularVelocity = AngularVelocity,
                MassData = massData,
                Friction = Friction,
                Restituion = Restituion,
                CollisionCategory = CollisionCategory,
                BreakWhenMaxPushPullForceIsReached = BreakWhenMaxPushPullForceIsReached,
                MaxPushPullForce = MaxPushPullForce,
            };
        }
        #endregion

        #region IClickable
        public bool IsPointInside(Vec2D position)
        {
            Vec2D[] points = Vertex;
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
            forceOnLeftBeam = 0;
            forceOnRightBeam = 0;
        }
        public void AddTrackForce(Vec2D forcePosition, Vec2D forceDirection)
        {
            if (InverseMass == 0 || rectangleIsBeam == false) return; //Tracke die die Kräfte, wenn das Rechteck eine Stabform hat

            Vec2D d = forcePosition - Vertex[0];
            float f = d * beamDirection * inverseBeamLength; //f=0 -> Kraft wirkt an linker Balkenecke; 1=Kraft wirkt an rechter Balkenecke
            float forceInBeamDirection = forceDirection * beamDirection;

            forceOnLeftBeam += (1 - f) * forceInBeamDirection;
            forceOnRightBeam += f * forceInBeamDirection;
        }
        public float GetPushPullForce()
        {
            return forceOnLeftBeam - forceOnRightBeam;
        }
        #endregion

        #region IPublicRigidRectangle
        public bool IsBroken { get; set; } = false; //Dieses Flag wird vom MaxForceTracker gesetzt
        public bool BreakWhenMaxPushPullForceIsReached { get; }
        public float MaxPushPullForce { get; }
        public float Area { get; }
        #endregion
    }
}
