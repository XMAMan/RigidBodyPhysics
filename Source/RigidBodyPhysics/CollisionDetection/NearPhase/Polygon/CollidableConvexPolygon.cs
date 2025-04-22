using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.RigidBody.Polygon;

namespace RigidBodyPhysics.CollisionDetection.NearPhase.Polygon
{
    internal class CollidableConvexPolygon : IConvexSubPolygon
    {
        public Vec2D Center { get; private set; }
        public Vec2D[] Vertex { get; }
        public Vec2D[] FaceNormal { get; }
        public bool[] IsOutsideEdge { get; } //[0] = Bezieht sich auf die Kante Points[0]-Points[1]; [Length-1]=Points[Length-1]-Points[0]
        public Vec2D CenterFromParentPolygon { get => this.parentPolygon.Center; }

        #region Variables to Calculate the Center from V[0]-V[1]-Edge
        private RigidPolygon parentPolygon;
        private float edge1TangentDistance, normalDistance;
        #endregion

        public CollidableConvexPolygon(RigidPolygon concavePolygon, ConvexPolygon convexPolygon)
        {
            this.parentPolygon = concavePolygon;
            this.Vertex = new Vec2D[convexPolygon.Indizes.Length];
            for (int i = 0; i < convexPolygon.Indizes.Length; i++)
            {
                this.Vertex[i] = concavePolygon.Vertex[convexPolygon.Indizes[i]];
            }

            CalculateVariablesToGetCenterFromEdge0();

            this.IsOutsideEdge = convexPolygon.IsOutsideEdge;
            this.IsNotMoveable = concavePolygon.IsNotMoveable;

            this.FaceNormal = new Vec2D[Vertex.Length];
            UpdateNormalsAndCenter();
        }

        private void CalculateVariablesToGetCenterFromEdge0()
        {
            //Ich darf das Zentrum nicht fest definieren da das Elternpolygon zwar die Vertex-Werte beim bewegen ändert aber nicht diese lokale Variable hier
            var center = PolygonHelper.GetCenterOfMassFromPolygon(this.Vertex);

            //Ich merke mit deswegen stattdessen, welche Tangent- und Normaldistanz das Zentrum zur Edge1 hat
            var edge1Dir = (this.Vertex[0] - this.Vertex[1]).Normalize();
            this.edge1TangentDistance = (center - this.Vertex[0]) * edge1Dir;
            Vec2D pointOnEdge1 = this.Vertex[0] + edge1Dir * edge1TangentDistance;
            var edge1Normal = edge1Dir.Spin90();
            this.normalDistance = (center - pointOnEdge1) * edge1Normal;

            //Vec2D centerFromP0 = this.Vertex[0] + edge1Dir * edge1TangentDistance + edge1Normal * normalDistance;
        }

        public bool IsNotMoveable { get; }
        public CollidableType TypeId { get; } = CollidableType.Polygon;
        public List<ICollidable> CollideExcludeList { get; } = new List<ICollidable>();
        public int CollisionCategory { get; private set; } = 0;

        public void UpdateNormalsAndCenter()
        {
            for (int i = 0; i < this.Vertex.Length; i++)
            {
                var p1 = Vertex[i];
                var p2 = Vertex[(i + 1) % Vertex.Length];
                Vec2D p1ToP2Direction = (p1 - p2).Normalize();
                Vec2D normal = p1ToP2Direction.Spin90();

                this.FaceNormal[i] = normal;

                if (i == 0)
                {
                    this.Center = this.Vertex[0] + p1ToP2Direction * edge1TangentDistance + normal * normalDistance;
                }

            }


        }
    }
}
