using GraphicMinimal;

namespace PhysicEngine.CollisionDetection.NearPhase
{
    //Das ist ähnlich wie die RectangleRectangleCollision1-Klasse aus dem "building-a-2d-physics-game-engine"-Buch 
    //aber ich habe das so geändert, dass nun mehrere Kontaktpunkte zurück gegeben werden können.
    //Ein Kontaktpunkt kann immer nur auf der Ecke von ein Rechteck liegen
    public static class RectangleRectangleCollision2
    {
        struct SupportStruct
        {
            public SupportStruct(float supportPointDist)
            {
                this.SupportPointDist = supportPointDist;
            }

            public List<Vector2D> SupportPoints = new List<Vector2D>();
            public float SupportPointDist = 0;
        }

        private static SupportStruct FindSupportPoint(ICollidableRectangle r, Vector2D dir, Vector2D p1OnEdge, Vector2D p2OnEdge)
        {
            var tmpSupport = new SupportStruct(-9999999);

            //check each vector of other object
            for (int i = 0; i < r.Vertex.Length; i++)
            {
                //the longest project length
                Vector2D p1ToRi = r.Vertex[i] - p1OnEdge;
                float normalCheck = p1ToRi * dir;

                if (normalCheck < 0) continue; //Prüfe dass es in der "dir"-Seite eingedrungen ist

                Vector2D p1ToP2 = p2OnEdge - p1OnEdge;
                if (p1ToRi * p1ToP2 < 0) continue; //Prüfe dass es vor der linken Kante von dir liegt
                Vector2D p2ToRi = r.Vertex[i] - p2OnEdge;
                if (p2ToRi * (-p1ToP2) < 0) continue; //Prüfe dass es vor der echten Kante von dir liegt

                //find the longest distance with certain edge
                //dir is -n direction, so the distance should be positive      
                if (normalCheck > 0 && normalCheck >= tmpSupport.SupportPointDist)
                {
                    if (normalCheck > tmpSupport.SupportPointDist) tmpSupport.SupportPoints.Clear(); //Wenn zwei Punkte den gleichen Max-Abstand haben dann nimm beide. Wenn nicht dann nimm nur den mit größten Abstand
                    tmpSupport.SupportPoints.Add(r.Vertex[i]);
                    tmpSupport.SupportPointDist = normalCheck;
                }
            }

            return tmpSupport;
        }

        //Beim Würfel-Auf-Tisch-Fall werden hier zwei Punkte zurück gegeben. Sonst einer
        private static CollisionInfo[] FindAxisLeastPenetration(ICollidableRectangle r1, ICollidableRectangle r2)
        {
            float bestDistance = 999999;
            int bestIndex = -1;
            Vector2D[] supportPoints = null;

            for (int i = 0; i < r1.FaceNormal.Length; i++)
            {
                // Retrieve a face normal from A
                var n = r1.FaceNormal[i];

                // use -n as direction and the vectex on edge i as point on edge

                // find the support on B
                // the point has longest distance with edge i 
                var tmpSupport = FindSupportPoint(r2, -n, r1.Vertex[i], r1.Vertex[(i+1)%4]);

                //SAT says if one side from r1 has no support-Point on r2, then there is no collision
                if (tmpSupport.SupportPoints == null) return null;

                //get the shortest support point depth
                if (tmpSupport.SupportPointDist < bestDistance)
                {
                    bestDistance = tmpSupport.SupportPointDist;
                    bestIndex = i;
                    supportPoints = tmpSupport.SupportPoints.ToArray();
                }
            }

            //all four directions have support point. That means at least one point from r2 lies inside from r1
            return supportPoints.Select(x => new CollisionInfo(x + r1.FaceNormal[bestIndex] * bestDistance, r1.FaceNormal[bestIndex], bestDistance)).ToArray();
        }

        public static CollisionInfo[] RectangleRectangle(ICollidableRectangle r1, ICollidableRectangle r2)
        {
            List<CollisionInfo> contacts = new List<CollisionInfo>();

            var collisionInfoR1 = FindAxisLeastPenetration(r1, r2);
            if (collisionInfoR1 != null)
                contacts.AddRange(collisionInfoR1.Select(x => new CollisionInfo(x.Start - x.Normal * x.Depth, x.Normal, x.Depth)));

            var collisionInfoR2 = FindAxisLeastPenetration(r2, r1);
            if (collisionInfoR2 != null)
                contacts.AddRange(collisionInfoR2.Select(x => new CollisionInfo(x.Start, -x.Normal, x.Depth)));

            if (contacts.Any() == false) return null;

            //Beim Würfel-Stapelbeispiel erhalte ich doppelte einträge welche hier hier rausfiltern will
            var withoutDoubles = contacts
                .GroupBy(x => x.Start.ToString() + x.Depth.ToString())
                .Select(x => x.First())
                .ToArray();

            return withoutDoubles;            
        }
    }
}
