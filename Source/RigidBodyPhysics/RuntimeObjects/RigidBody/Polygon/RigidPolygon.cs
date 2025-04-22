using RigidBodyPhysics.CollisionDetection.NearPhase;
using RigidBodyPhysics.CollisionDetection.NearPhase.Polygon;
using RigidBodyPhysics.ExportData.RigidBody;
using RigidBodyPhysics.MathHelper;

namespace RigidBodyPhysics.RuntimeObjects.RigidBody.Polygon
{
    //Erweitert das ConcavePolygon um die Kollisionsabfrage (Siehe ICollidableContainer und ICollidable (Kommt durch IRigidBody))
    internal class RigidPolygon : ConcavePolygon, IRigidBody, ICollidableContainer
    {
        public ICollidable[] Colliables { get; }
        private CollidableConvexPolygon[] subPolys;

        public RigidPolygon(PolygonExportData data)
            : base(data)
        {
            CollisionCategory = data.CollisionCategory;

            var convexes = PolygonHelper.ConvertConcavePolygonToConvexes(Vertex);
            Colliables = subPolys = convexes.Select(x => new CollidableConvexPolygon(this, x)).ToArray();

            Rotate(data.AngleInDegree / 180 * (float)Math.PI);

            SubPolys = Colliables.Select(x => ((CollidableConvexPolygon)x).Vertex).ToList(); //Zum Testausgabe
        }

        public override void Rotate(float angle)
        {
            base.Rotate(angle);

            foreach (var poly in subPolys)
                poly.UpdateNormalsAndCenter();
        }

        #region ICollidable
        public bool IsNotMoveable { get => InverseMass == 0; }
        public CollidableType TypeId { get; } = CollidableType.Container;
        public List<ICollidable> CollideExcludeList { get; } = new List<ICollidable>();
        public int CollisionCategory { get; private set; } = 0;
        #endregion

        protected override int GetCollisionCategory()
        {
            return CollisionCategory;
        }
    }
}
