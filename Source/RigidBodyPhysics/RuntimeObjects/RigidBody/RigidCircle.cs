using PhysicGlobal;
using RigidBodyPhysics.CollisionDetection.NearPhase;
using RigidBodyPhysics.ExportData.RigidBody;
using RigidBodyPhysics.MathHelper;

namespace RigidBodyPhysics.RuntimeObjects.RigidBody
{
    internal class RigidCircle : IRigidBody, ICollidableCircle, IPublicRigidCircle
    {
        private readonly MassData massData; //Wird für die ExportFunktion benötigt
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


        public RigidCircle(Vec2D center, float radius, float angle, MassData massData)
        {
            this.massData = massData;
            Center = center;
            Angle = angle;
            Velocity = new Vec2D(0, 0);
            AngularVelocity = 0;
            Area = radius * radius * (float)Math.PI;
            float mass = massData.GetMass(Area);
            InverseMass = float.MaxValue == mass ? 0 : 1 / mass;
            InverseInertia = InverseMass == 0 ? 0 : 1.0f / (mass * radius * radius / 12f);
            Force = new Vec2D(0, 0);
            Torque = 0;
            Radius = radius;
            RotateToWorld = Matrix2x2.Rotate(Angle);
        }

        public RigidCircle(CircleExportData data)
            : this(data.Center, data.Radius, data.AngleInRad, data.MassData)
        {
            Velocity = new Vec2D(data.Velocity);
            AngularVelocity = data.AngularVelocity;
            Friction = data.Friction;
            Restituion = data.Restituion;
            CollisionCategory = data.CollisionCategory;
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
        public List<ICollidable> CollideExcludeList { get; } = new List<ICollidable>();
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
        #endregion

        #region IClickable
        public bool IsPointInside(Vec2D position)
        {
            return (position - Center).Length() < Radius;
        }
        #endregion

        #region IPublicRigidBody
        public float Area { get; }
        #endregion
    }
}
