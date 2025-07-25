using PhysicGlobal;
using RigidBodyPhysics.CollisionDetection.NearPhase;
using RigidBodyPhysics.ExportData.RigidBody;
using RigidBodyPhysics.MathHelper;

namespace RigidBodyPhysics.RuntimeObjects.RigidBody
{
    internal class RigidCircle : IRigidBody, ICollidableCircle, IPublicRigidCircle
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


        #region IForceable
        public Vec2D Force { get; set; }
        public float Torque { get; set; }
        #endregion

        public float Radius { get; init; }

        //Dieser Konstruktor weißt ausschließlich allen init/readonly-Properties ein Wert zu
        private RigidCircle(CircleExportData data, bool notUsed)
        {
            this.massData = data.MassData;
            this.Radius = data.Radius;
            this.Area = Radius * Radius * (float)Math.PI;
            float mass = massData.GetMass(Area);
            this.InverseMass = float.MaxValue == mass ? 0 : 1 / mass;
            this.InverseInertia = InverseMass == 0 ? 0 : 1.0f / (mass * Radius * Radius / 12f);
            this.Friction = data.Friction;
            this.Restituion = data.Restituion;
            this.CollisionCategory = data.CollisionCategory;
        }

        //Dieser Konstruktor ruft nur LoadExportData aber weißt keiner Variable ein Wert zu
        public RigidCircle(CircleExportData data)
            : this(data, false)
        {
            LoadExportData(data);
        }

        #region IMoveable
        public void MoveTo(Vec2D position, float angle)
        {
            this.Center = position;
            this.Angle = angle;
            this.RotateToWorld = Matrix2x2.Rotate(this.Angle);
        }
        public Matrix2x2 RotateToWorld { get; private set; }
        #endregion

        #region ICollidable
        public bool IsNotMoveable { get => InverseMass == 0; }
        public CollidableType TypeId { get; } = CollidableType.Circle;
        public List<ICollidable> CollideExcludeList { get; init; } = new List<ICollidable>();
        public int CollisionCategory { get; init; } = 0;
        #endregion
        #region IExportable
        public IExportRigidBody GetExportData()
        {
            return new CircleExportData()
            {
                Center = new Vec2D(Center),
                Radius = Radius,
                AngleInRad = Angle,
                Velocity = new Vec2D(Velocity),
                AngularVelocity = AngularVelocity,
                MassData = new MassData(massData),
                Friction = Friction,
                Restituion = Restituion,
                CollisionCategory = CollisionCategory,
            };
        }
        public void LoadExportData(IExportRigidBody exportData)
        {
            var data = (CircleExportData)exportData;

            MoveTo(data.Center, data.AngleInRad);
            this.Velocity = new Vec2D(data.Velocity);
            this.AngularVelocity = data.AngularVelocity;
            this.Force = new Vec2D(0, 0);
            this.Torque = 0;
        }
        #endregion

        #region IClickable
        public bool IsPointInside(Vec2D position)
        {
            return (position - Center).Length() < Radius;
        }
        #endregion

        #region IPublicRigidBody
        public float Area { get; init; }
        #endregion
    }
}
