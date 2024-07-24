using GraphicMinimal;
using PhysicEngine.CollisionDetection.NearPhase;
using PhysicEngine.ExportData.RigidBody;

namespace PhysicEngine.RigidBody
{
    public class RigidCircle : IRigidBody, ICollidableCircle
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


        #region IForceable
        public Vector2D Force { get; set; }
        public float Torque { get; set; }
        #endregion

        public float Radius { get; private set; }
        

        public RigidCircle(Vector2D center, float radius, float angle, MassData massData)
        {
            this.massData = massData;
            this.Center = center;
            this.Angle = angle;
            this.Velocity = new Vector2D(0, 0);
            this.AngularVelocity = 0;
            float mass = massData.GetMass(radius * radius * (float)Math.PI);
            this.InverseMass = float.MaxValue == mass ? 0 : 1 / mass;
            this.InverseInertia = this.InverseMass == 0 ? 0 : 1.0f / (mass * radius * radius / 12f);
            this.Force = new Vector2D(0, 0);
            this.Torque = 0;
            this.Radius = radius;
        }

        public RigidCircle(CircleExportData data)
            : this(data.Center, data.Radius, data.AngleInDegree / 180 * (float)Math.PI, data.MassData)
        {
            this.Velocity = data.Velocity;
            this.AngularVelocity = data.AngularVelocity;
            this.Friction = data.Friction;
            this.Restituion = data.Restituion;
        }

        #region IMoveable
        public void MoveCenter(Vector2D v)
        {
            this.Center += v;
        }

        public void Rotate(float angle)
        {
            this.Angle += angle;
        }
        #endregion

        #region ICollidable
        public CollisionInfo[] CollideWith(ICollidable collidable)
        {
            var c = NearPhaseTests.CircleOther(this, collidable);
            if (c != null)
            {
                return new CollisionInfo[] { c };
            }

            return null;
        }
        #endregion
        #region IExportable
        public IExportRigidBody GetExportData()
        {
            return new CircleExportData()
            {
                Center = this.Center,
                Radius = this.Radius,
                AngleInDegree = (float)(this.Angle / (2 * Math.PI) * 360),
                Velocity = this.Velocity,
                AngularVelocity = this.AngularVelocity,
                MassData = this.massData,
                Friction = this.Friction,
                Restituion = this.Restituion
            };
        }
        #endregion

        #region IClickable
        public bool IsPointInside(Vector2D position)
        {
            return (position - this.Center).Length() < this.Radius;
        }
        #endregion
    }
}
