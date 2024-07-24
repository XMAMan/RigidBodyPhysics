using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using RigidBodyPhysics.RuntimeObjects.RigidBody.Polygon;

namespace RigidBodyPhysics.CollisionDetection.NearPhase.Polygon
{
    internal class PolygonEdge : ICollideablePolygonEdge
    {
        public Vec2D Center { get; }
        public Vec2D P1 { get; }
        public Vec2D P2 { get; }
        public float Min { get; } //Wenn Min = 0, dann startet die Linie bei P1. 
        public float Max { get; } //Wenn Max = Length, dann endet die Linie bei P2
        public Vec2D Normal { get; private set; }
        public Vec2D P1ToP2Direction { get; private set; }
        public float Length { get; }
        public bool IsP1Convex { get; }

        private PolygonCollisionType polygonType;

        public PolygonEdge(EdgePolygon polygon, int p1Index, bool isP1Convex, bool isP2Convex, float min, float max)
        {
            if (polygon.PolygonType == PolygonCollisionType.Rigid)
                throw new ArgumentException("This class should only be use if the Polygontyp is a EdgePolygon");

            this.Center = polygon.Center;
            this.P1 = polygon.Vertex[p1Index];
            this.P2 = polygon.Vertex[(p1Index + 1) % polygon.Vertex.Length];
            this.P1ToP2Direction = (P2 - P1).Normalize();
            this.Length = (P2 - P1).Length();
            this.Normal = P1ToP2Direction.Spin90();
            if (polygon.PolygonType == PolygonCollisionType.EdgeWithNormalsPointingOutside)
                this.Normal = -this.Normal;
            this.IsP1Convex = isP1Convex;

            this.IsNotMoveable = polygon.IsNotMoveable;
            this.CollideExcludeList = polygon.CollideExcludeList;
            this.CollisionCategory = polygon.CollisionCategory;

            this.polygonType = polygon.PolygonType;

            if (isP1Convex)
                this.Min = 0;
            else
                this.Min = min;

            if (isP2Convex)
                this.Max = this.Length;
            else
                this.Max = max;
        }

        public bool IsNotMoveable { get; }
        public CollidableType TypeId { get; } = CollidableType.Edge;
        public List<ICollidable> CollideExcludeList { get; } = new List<ICollidable>();
        public int CollisionCategory { get; private set; } = 0;

        public void UpdateNormal()
        {
            this.P1ToP2Direction = (P2 - P1).Normalize();
            this.Normal = P1ToP2Direction.Spin90();
            if (this.polygonType == PolygonCollisionType.EdgeWithNormalsPointingOutside)
                this.Normal = -this.Normal;
        }
    }
}
