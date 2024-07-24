using PhysicEngine.CollisionDetection.NearPhase;
using PhysicEngine.CollisionDetection.NearPhase.Polygon;
using PhysicEngine.ExportData.RigidBody;
using PhysicEngine.MathHelper;

namespace PhysicEngine.RigidBody.Polygon
{
    //Erweitert das ConcavePolygon um die Kollisionsabfrage (Siehe ICollidableContainer und ICollidable (Kommt durch IRigidBody))
    internal class EdgePolygon : ConcavePolygon, IRigidBody, ICollidableContainer
    {
        public ICollidable[] Colliables { get; }
        private PolygonEdge[] edges;
        public EdgePolygon(PolygonExportData data)
            :base(data)
        {
            CollisionCategory = data.CollisionCategory;

            this.Colliables = this.edges = new PolygonEdge[this.Vertex.Length];

            //Schritt 1: Berechnet für jeden Polygoneckpunkt, ob er eine Rechteckskante durchstoßen kann ohne dass es weitere Schnittpunkte gibt
            bool[] isConvex = new bool[this.Vertex.Length];
            for (int i=0; i<this.Vertex.Length; i++)
            {
                var p1 = this.Vertex[i];
                var p2 = this.Vertex[(i + 1) % this.Vertex.Length];
                var p3 = this.Vertex[(i + 2) % this.Vertex.Length];
                bool isP2Convex = Vec2D.ZValueFromCross(p1 - p2, p3 - p2) < 0;
                if (this.PolygonType == PolygonCollisionType.EdgeWithNormalsPointingInside)
                    isP2Convex = !isP2Convex;

                isConvex[(i + 1) % this.Vertex.Length] = isP2Convex;
            }
            this.IsConvex = isConvex;

            //Schritt 2: Berechne wie lang die Edges maximal sein dürfen indem von jeder Edge alle Schnittpunkte mit allen Nicht-Nachbarkanten berechnet werden
            float[] minValues = new float[this.Vertex.Length];
            float[] maxValues = new float[this.Vertex.Length];
            for (int i = 0; i < this.Vertex.Length; i++)
            {
                //Prüfe für Edge [i]-[i+1] und ermittle seine Min/Max-Werte

                float min = float.MinValue; //Größte negative Zahl ist gesucht
                float max = float.MaxValue; //Kleisnet Zahl, die größer als edgeLength ist, is gesucht

                float edgeLength = (this.Vertex[(i + 1) % this.Vertex.Length] - this.Vertex[i]).Length();

                //Gehe durch alle Edges durch, die nicht den Punkt [i] oder [i+1] enthalten
                for (int j=0;j<this.Vertex.Length;j++)
                {
                    if (j != i && j != i+1 && (j+1) != i && (j+1) != (i+1)) //Kante [j]-[j+1] ist kein Nachbar von [i]-[i+1]
                    {
                        var pi1 = this.Vertex[i];
                        var pi2 = this.Vertex[(i + 1) % this.Vertex.Length];
                        var pj1 = this.Vertex[j];
                        var pj2 = this.Vertex[(j + 1) % this.Vertex.Length];

                        PolygonHelper.IntersectionTwoRays(pi1, (pi2-pi1).Normalize(), pj1, (pj2-pj1).Normalize(), out float t1, out float t2);
                        if (float.IsNaN(t1) == false && float.IsInfinity(t1) == false)
                        {
                            if (t1 < 0)
                                min = Math.Max(min, t1);

                            if (t1 > edgeLength)
                                max = Math.Min(max, t1);
                        }                             
                    }
                }


                minValues[i] = min;
                maxValues[i] = max;
            }

            for (int i=0;i< this.Vertex.Length;i++)
            {               
                this.Colliables[i] = this.edges[i] = new PolygonEdge(this, i, isConvex[i], isConvex[(i+1)%isConvex.Length], minValues[i], maxValues[i]);
            }

            Rotate(data.AngleInDegree / 180 * (float)Math.PI);
        }

        public override void Rotate(float angle)
        {
            base.Rotate(angle);

            foreach (var edge in this.edges)
                edge.UpdateNormal();
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
