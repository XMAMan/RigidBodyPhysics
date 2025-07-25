using PhysicGlobal;
using RigidBodyPhysics.CollisionDetection.NearPhase;
using RigidBodyPhysics.ExportData.RigidBody;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.MaxForceTracking;

namespace RigidBodyPhysics.RuntimeObjects.RigidBody
{
    internal class RigidRectangle : IRigidBody, ICollidableRectangle, IPublicRigidRectangle, IBeamForceTracker, IBreakableBody
    {
        private MassData massData { get; init; } //Wird für die ExportFunktion benötigt
        #region IRigidBody
        public Vec2D Center { get; private set; } //Position of the Center of gravity
        public float Angle { get; private set; } //Oriantation around the Z-Aches with rotationpoint=Center [0..2PI]
        public Vec2D Velocity { get; set; } //Velocity from the Center-Point
        public float AngularVelocity { get; set; }

        public float InverseMass { get; init; } //1 / Mass
        public float InverseInertia { get; init; }
        public float Restituion { get; init; } = 1;
        public float Friction { get; init; } = 1;
        #endregion


        #region IBoundingCircle
        public float Radius { get; init; }
        #endregion

        #region ICollidableRectangle
        //0--TopLeft;1--TopRight;2--BottomRight;3--BottomLeft
        public Vec2D[] Vertex { get; init; }
        private Vec2D[] vertexLocal { get; init; }

        //0--Top;1--Right;2--Bottom;3--Left
        public Vec2D[] FaceNormal { get; private set; }
        #endregion

        #region IForceable
        public Vec2D Force { get; set; }
        public float Torque { get; set; }
        #endregion

        public Vec2D Size { get; init; }

        //Dieser Konstruktor weißt ausschließlich allen init/readonly-Properties ein Wert zu
        private RigidRectangle(RectangleExportData data, int isBeamFactor)
        {
            this.massData = data.MassData;
            this.Size = data.Size;
            this.Area = Size.X * Size.Y;
            float mass = massData.GetMass(Area);
            this.InverseMass = float.MaxValue == mass ? 0 : 1 / mass;
            this.InverseInertia = InverseMass == 0 ? 0 : 1.0f / (mass * (Size.X * Size.X + Size.Y * Size.Y) / 12f);
            this.Radius = (float)Math.Sqrt(Size.X * Size.X + Size.Y * Size.Y) / 2;
            
            this.vertexLocal = new Vec2D[]
            {
                new Vec2D(-Size.X / 2, -Size.Y / 2), //TopLeft
                new Vec2D(+Size.X / 2, -Size.Y / 2), //TopRight
                new Vec2D(+Size.X / 2, +Size.Y / 2), //BottomRight
                new Vec2D(-Size.X / 2, +Size.Y / 2), //BottomLeft
            };
            this.Vertex = new Vec2D[vertexLocal.Length];

            this.beamDirectionLocal = Size.X > Size.Y ? vertexLocal[1] - vertexLocal[0] : vertexLocal[3] - vertexLocal[0];
            this.inverseBeamLength = 1.0f / beamDirectionLocal.Length();
            this.beamDirectionLocal /= beamDirectionLocal.Length();
            this.rectangleIsBeam = Size.X > Size.Y * isBeamFactor || Size.Y > Size.X * isBeamFactor;

            this.Friction = data.Friction;
            this.Restituion = data.Restituion;
            this.CollisionCategory = data.CollisionCategory;
            this.BreakWhenMaxPushPullForceIsReached = data.BreakWhenMaxPushPullForceIsReached;
            this.MaxPushPullForce = data.MaxPushPullForce;
        }

        //Dieser Konstruktor ruft nur LoadExportData aber weißt keiner Variable ein Wert zu
        public RigidRectangle(RectangleExportData data)
            : this(data, 2)
        {
            LoadExportData(data);
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
        public void MoveTo(Vec2D position, float angle)
        {
            this.Center = position;
            this.Angle = angle;
            this.RotateToWorld = Matrix2x2.Rotate(Angle);

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
        public List<ICollidable> CollideExcludeList { get; init; } = new List<ICollidable>();
        public int CollisionCategory { get; init; } = 0;
        #endregion

        #region IExportable

        public IExportRigidBody GetExportData()
        {
            return new RectangleExportData()
            {
                Center = new Vec2D(Center),
                Size = Size,
                AngleInRad = Angle,
                Velocity = new Vec2D(Velocity),
                AngularVelocity = AngularVelocity,
                MassData = new MassData(massData),
                Friction = Friction,
                Restituion = Restituion,
                CollisionCategory = CollisionCategory,
                BreakWhenMaxPushPullForceIsReached = BreakWhenMaxPushPullForceIsReached,
                MaxPushPullForce = MaxPushPullForce,
                IsBroken = IsBroken,
            };
        }

        public void LoadExportData(IExportRigidBody exportData)
        {
            var data = (RectangleExportData)exportData;

            MoveTo(data.Center, data.AngleInRad);
            this.Velocity = new Vec2D(data.Velocity);
            this.AngularVelocity = data.AngularVelocity;

            this.IsBroken = data.IsBroken;
            this.Force = new Vec2D(0, 0);
            this.Torque = 0;
            ResetTrackForce();
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
        private bool rectangleIsBeam { get; init; } = false; //Nur wenn das Rechteck 5 mal so lang wie hoch ist gilt es als Stab
        private Vec2D beamDirectionLocal { get; init; } //Wenn das Rechteck wie ein Stab aussieht, dann zeigt dieser Vektor in Stabrichtung
        private Vec2D beamDirection;
        private float inverseBeamLength { get; init; }
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
        public bool BreakWhenMaxPushPullForceIsReached { get; init; }
        public float MaxPushPullForce { get; init; }
        public float Area { get; init; }
        #endregion
    }
}
