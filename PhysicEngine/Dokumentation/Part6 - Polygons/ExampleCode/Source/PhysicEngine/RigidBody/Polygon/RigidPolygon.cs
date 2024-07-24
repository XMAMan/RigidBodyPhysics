using PhysicEngine.CollisionDetection.NearPhase;
using PhysicEngine.CollisionDetection.NearPhase.Polygon;
using PhysicEngine.ExportData.RigidBody;
using PhysicEngine.MathHelper;

namespace PhysicEngine.RigidBody.Polygon
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

            var convexes = PolygonHelper.ConvertConcavePolygonToConvexes(this.Vertex);
            this.Colliables = this.subPolys = convexes.Select(x => new CollidableConvexPolygon(this, x)).ToArray();

            Rotate(data.AngleInDegree / 180 * (float)Math.PI);

            this.SubPolys = this.Colliables.Select(x => ((CollidableConvexPolygon)x).Vertex).ToList(); //Zum Testausgabe
        }

        public override void Rotate(float angle)
        {
            base.Rotate(angle);

            foreach (var poly in this.subPolys)
                poly.UpdateNormalsAndCenter();
        }

        #region ICollidable
        public bool IsNotMoveable { get => this.InverseMass == 0; }
        public CollidableType TypeId { get; } = CollidableType.Container;
        public List<ICollidable> CollideExcludeList { get; } = new List<ICollidable>();
        public int CollisionCategory { get; private set; } = 0;
        #endregion

        protected override int GetCollisionCategory()
        {
            return this.CollisionCategory;
        }
    }
}
