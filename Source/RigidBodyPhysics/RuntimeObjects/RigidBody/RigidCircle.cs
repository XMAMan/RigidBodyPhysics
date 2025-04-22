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

        public float InverseMass { get; private set; } //1 / Mass
        public float InverseInertia { get; private set; }
        public float Restituion { get; private set; } = 1;
        public float Friction { get; private set; } = 1;
        #endregion


        #region IForceable
        public Vec2D Force { get; set; }
        public float Torque { get; set; }
        #endregion

        public float Radius { get; private set; }


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
            : this(data.Center, data.Radius, data.AngleInDegree / 180 * (float)Math.PI, data.MassData)
        {
            Velocity = new Vec2D(data.Velocity);
            AngularVelocity = data.AngularVelocity;
            Friction = data.Friction;
            Restituion = data.Restituion;
            CollisionCategory = data.CollisionCategory;
        }

        #region IMoveable
        public void MoveCenter(Vec2D v)
        {
            Center += v;
        }

        public void Rotate(float angle)
        {
            Angle += angle;
            RotateToWorld = Matrix2x2.Rotate(Angle);
        }
        public Matrix2x2 RotateToWorld { get; private set; }
        #endregion

        #region ICollidable
        public bool IsNotMoveable { get => InverseMass == 0; }
        public CollidableType TypeId { get; } = CollidableType.Circle;
        public List<ICollidable> CollideExcludeList { get; } = new List<ICollidable>();
        public int CollisionCategory { get; private set; } = 0;
        #endregion
        #region IExportable
        public IExportRigidBody GetExportData()
        {
            return new CircleExportData()
            {
                Center = Center,
                Radius = Radius,
                AngleInDegree = (float)(Angle / (2 * Math.PI) * 360),
                Velocity = Velocity,
                AngularVelocity = AngularVelocity,
                MassData = massData,
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
