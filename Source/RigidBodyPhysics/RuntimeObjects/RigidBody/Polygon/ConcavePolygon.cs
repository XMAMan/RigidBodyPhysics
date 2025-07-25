using PhysicGlobal;
using RigidBodyPhysics.CollisionDetection.BroadPhase;
using RigidBodyPhysics.CollisionDetection.NearPhase;
using RigidBodyPhysics.ExportData.RigidBody;
using RigidBodyPhysics.MathHelper;

namespace RigidBodyPhysics.RuntimeObjects.RigidBody.Polygon
{
    //Polygon mit Masse/Schwerpunkt/Inertia/Rotate-/Move-Funktion aber ohne Kollisionsfunktion
    internal abstract class ConcavePolygon : IBoundingCircle, IForceable, IMoveable, IExportableBody, IClickable, IPublicRigidBody, IPublicRigidPolygon, ICollidable
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

        #region IForceable
        public Vec2D Force { get; set; }
        public float Torque { get; set; }
        #endregion

        private Vec2D[] localPoints { get; init; }

        //Dieser Konstruktor weißt ausschließlich allen init/readonly-Properties ein Wert zu
        //points = Polygon dessen Schwerpunkt am Punkt [0,0] liegt. 
        private ConcavePolygon(PolygonExportData data, bool notUsed)
        {
            //Für IsConvex/EdgeNormalen/IsEdgeOutside muss das Polygon immer CCW sein
            this.localPoints = PhysicGlobal.PolygonHelper.OrderPointsCounterClockWise(data.Points);

            this.massData = data.MassData;
            this.Area = PhysicGlobal.PolygonHelper.GetAreaFromPolygon(this.localPoints);
            float mass = massData.GetMass(Area);
            this.InverseMass = float.MaxValue == mass ? 0 : 1 / mass;
            this.InverseInertia = InverseMass == 0 ? 0 : 1.0f / MathHelper.PolygonHelper.GetInertiaFromPolygon(massData.GetDensity(Area), localPoints);

            this.Radius = this.localPoints.Max(x => x.Length());
            this.PolygonType = data.PolygonType;

            this.Vertex = localPoints.Select(x => data.Center + x).ToArray();
            this.SubPolys = new List<Vec2D[]> { Vertex };

            this.Friction = data.Friction;
            this.Restituion = data.Restituion;

            this.CollisionCategory = data.CollisionCategory;
        }


        //Dieser Konstruktor ruft nur LoadExportData aber weißt keiner Variable ein Wert zu
        public ConcavePolygon(PolygonExportData data)
            : this(data, false)
        {
            LoadExportData(data);
        }

        #region IMoveable
        public virtual void MoveTo(Vec2D position, float angle)
        {
            this.Center = position;
            this.Angle = angle;
            this.RotateToWorld = Matrix2x2.Rotate(this.Angle);
            for (int i = 0; i < localPoints.Length; i++)
            {
                //Weise dem Vertexelementen kein neues Objekt zu, da dieses Objekt auch von den Kollisionserkennungsobjekten genutzt wird
                Vec2D newPos = Center + RotateToWorld * localPoints[i];
                Vertex[i].X = newPos.X;
                Vertex[i].Y = newPos.Y;
            }
        }
        public Matrix2x2 RotateToWorld { get; private set; }
        #endregion

        #region IExportable
        public IExportRigidBody GetExportData()
        {
            return new PolygonExportData()
            {
                PolygonType = PolygonType,
                Points = localPoints.Select(x => new Vec2D(x)).ToArray(),
                Center = new Vec2D(Center),
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
            var data = (PolygonExportData)exportData;
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
            if (InverseMass == 0) return false;

            return PhysicGlobal.PolygonHelper.PointIsInsidePolygon(Vertex, position);
        }
        #endregion

        #region IPublicRigidBody
        public float Area { get; init; }
        #endregion

        #region IPublicRigidPolygon
        public Vec2D[] Vertex { get; init; }
        public PolygonCollisionType PolygonType { get; init; }

        public List<Vec2D[]> SubPolys { get; init; } //Zur Testausgabe
        public bool[] IsConvex { get; init; } = null;
        #endregion

        #region ICollidable
        public bool IsNotMoveable { get => InverseMass == 0; }
        public CollidableType TypeId { get; } = CollidableType.Container;
        public List<ICollidable> CollideExcludeList { get; init; } = new List<ICollidable>();
        public int CollisionCategory { get; init; } = 0;
        #endregion
    }
}
