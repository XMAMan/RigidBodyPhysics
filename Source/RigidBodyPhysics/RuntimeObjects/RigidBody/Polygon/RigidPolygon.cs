using PhysicGlobal;
using RigidBodyPhysics.CollisionDetection.NearPhase;
using RigidBodyPhysics.CollisionDetection.NearPhase.Polygon;
using RigidBodyPhysics.ExportData.RigidBody;

namespace RigidBodyPhysics.RuntimeObjects.RigidBody.Polygon
{
    //Erweitert das ConcavePolygon um die Kollisionsabfrage (Siehe ICollidableContainer und ICollidable (Kommt durch IRigidBody))
    internal class RigidPolygon : ConcavePolygon, IRigidBody, ICollidableContainer
    {
        public ICollidable[] Colliables { get; init; }
        private CollidableConvexPolygon[] subPolys { get; init; } = new CollidableConvexPolygon[0];

        public RigidPolygon(PolygonExportData data)
            : base(data)
        {
            var convexes = MathHelper.PolygonHelper.ConvertConcavePolygonToConvexes(Vertex);
            Colliables = subPolys = convexes.Select(x => new CollidableConvexPolygon(this, x)).ToArray();

            MoveTo(data.Center, data.AngleInRad);

            SubPolys = Colliables.Select(x => ((CollidableConvexPolygon)x).Vertex).ToList(); //Zum Testausgabe
        }

        public override void MoveTo(Vec2D position, float angle)
        {
            base.MoveTo(position, angle);

            foreach (var poly in subPolys)
                poly.UpdateNormalsAndCenter();
        }
    }
}
