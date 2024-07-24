using PhysicEngine.CollisionDetection.BroadPhase;
using PhysicEngine.ExportData.RigidBody;
using PhysicEngine.MathHelper;

namespace PhysicEngine.RigidBody.Polygon
{
    //Polygon mit Masse/Schwerpunkt/Inertia/Rotate-/Move-Funktion aber ohne Kollisionsfunktion
    internal abstract class ConcavePolygon : IBoundingCircle, IForceable, IMoveable, IExportableBody, IClickable, IPublicRigidBody, IPublicRigidPolygon
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

        #region IForceable
        public Vec2D Force { get; set; }
        public float Torque { get; set; }
        #endregion

        private Vec2D[] localPoints;

        //points = Polygon dessen Schwerpunkt am Punkt [0,0] liegt. 
        protected ConcavePolygon(Vec2D center, Vec2D[] points, MassData massData, PolygonCollisionType polygonType)
        {
            points = PolygonHelper.OrderPointsCCW(points); //Für IsConvex/EdgeNormalen/IsEdgeOutside muss das Polygon immer CCW sein

            localPoints = points;

            this.massData = massData;
            Center = center;
            Angle = 0;
            Velocity = new Vec2D(0, 0);
            AngularVelocity = 0;
            float polygonArea = PolygonHelper.GetAreaFromPolygon(points);
            float mass = massData.GetMass(polygonArea);
            InverseMass = float.MaxValue == mass ? 0 : 1 / mass;
            InverseInertia = InverseMass == 0 ? 0 : 1.0f / PolygonHelper.GetInertiaFromPolygon(massData.GetDensity(polygonArea), points);
            Force = new Vec2D(0, 0);
            Torque = 0;

            Radius = points.Max(x => x.Length());
            PolygonType = polygonType;

            Vertex = localPoints.Select(x => Center + x).ToArray();
            this.SubPolys = new List<Vec2D[]> { this.Vertex };
        }

        protected abstract int GetCollisionCategory();

        public ConcavePolygon(PolygonExportData data)
            : this(data.Center, data.Points, data.MassData, data.PolygonType)
        {
            Velocity = data.Velocity;
            AngularVelocity = data.AngularVelocity;
            Friction = data.Friction;
            Restituion = data.Restituion;            
        }

        #region IMoveable
        public void MoveCenter(Vec2D v)
        {
            for (int i = 0; i < Vertex.Length; i++)
            {
                //Weise dem Vertexelementen kein neues Objekt zu, da dieses Objekt auch von den Kollisionserkennungsobjekten genutzt wird
                Vertex[i].X += v.X;
                Vertex[i].Y += v.Y;
            }

            Center += v;
        }
        public virtual void Rotate(float angle)
        {
            Angle += angle;

            RotateToWorld = Matrix2x2.Rotate(Angle);
            for (int i = 0; i < localPoints.Length; i++)
            {
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
                PolygonType = this.PolygonType,
                Points = this.localPoints,
                Center = this.Center,
                AngleInDegree = (float)(this.Angle / Math.PI * 180),
                Velocity = this.Velocity,
                AngularVelocity = this.AngularVelocity,
                MassData = this.massData,
                Friction = this.Friction,
                Restituion = this.Restituion,
                CollisionCategory = GetCollisionCategory(),
            };
        }
        #endregion

        #region IClickable
        public bool IsPointInside(Vec2D position)
        {
            if (this.InverseMass == 0) return false;

            return PolygonHelper.PointIsInsidePolygon(Vertex, position);
        }
        #endregion


        #region IPublicRigidPolygon
        public Vec2D[] Vertex { get; private set; }
        public PolygonCollisionType PolygonType { get; private set; }

        public List<Vec2D[]> SubPolys { get; protected set; } //Zur Testausgabe
        public bool[] IsConvex { get; protected set; } = null;
        #endregion
        
    }
}
