using PhysicEngine.MathHelper;

namespace PhysicEngine.CollisionDetection.NearPhase
{
    //Das ist ähnlich wie die Rechteck-Kollision-Funktion aus dem "building-a-2d-physics-game-engine"-Buch 
    //aber ich habe das so geändert, dass nun mehrere Kontaktpunkte zurück gegeben werden können.
    //Ein Kontaktpunkt kann immer nur auf der Ecke von ein Rechteck liegen
    internal static class RectangleRectangleCollision
    {
        struct SupportStruct
        {
            internal SupportStruct(float supportPointDist)
            {
                this.MaxDistance = supportPointDist;
            }

            internal List<byte> SupportPoints = new List<byte>(); //Hier steht der Index auf ein Eckpunkt von ein Rechteck
            internal float MaxDistance = 0;
        }

        //Gib all die Punkte aus Rechteck r zurück, welche hinter der Kante p1OnEdge-p2OnEdge liegen
        private static SupportStruct FindSupportPoint(ICollidableRectangle r, Vec2D dir, Vec2D p1OnEdge, Vec2D p2OnEdge)
        {
            var tmpSupport = new SupportStruct(-9999999);

            //check each vector of other object
            for (byte i = 0; i < r.Vertex.Length; i++)
            {
                //the longest project length
                Vec2D p1ToRi = r.Vertex[i] - p1OnEdge;
                float normalCheck = p1ToRi * dir;

                if (normalCheck < 0) continue; //Prüfe dass es in der "dir"-Seite eingedrungen ist

                Vec2D p1ToP2 = p2OnEdge - p1OnEdge;
                if (p1ToRi * p1ToP2 < 0) continue; //Prüfe dass es vor der linken Kante von dir liegt
                Vec2D p2ToRi = r.Vertex[i] - p2OnEdge;
                if (p2ToRi * (-p1ToP2) < 0) continue; //Prüfe dass es vor der echten Kante von dir liegt

                if (normalCheck > tmpSupport.MaxDistance) tmpSupport.MaxDistance = normalCheck;
                tmpSupport.SupportPoints.Add(i);
            }

            return tmpSupport;
        }

        //Gibt all die Punkte von r2 zurück, welche in r1 liegen. Dabei wird die r1-Facenormale gewählt, wo die Punktwolke aus r2 am wenigstens eingedrungen ist
        private static CollisionInfo[] FindAxisLeastPenetration(ICollidableRectangle r1, ICollidableRectangle r2)
        {
            float bestDistance = 999999;
            int r1Face = -1;
            byte[] supportPoints = null;

            for (int i = 0; i < r1.FaceNormal.Length; i++)
            {
                // Retrieve a face normal from A
                var n = r1.FaceNormal[i];

                // use -n as direction and the vectex on edge i as point on edge

                // find the support on B
                // the point has longest distance with edge i 
                var tmpSupport = FindSupportPoint(r2, -n, r1.Vertex[i], r1.Vertex[(i + 1) % 4]);

                //SAT says if one side from r1 has no support-Point on r2, then there is no collision
                if (tmpSupport.SupportPoints == null) return null;

                //get the shortest support point depth
                if (tmpSupport.MaxDistance < bestDistance)
                {
                    bestDistance = tmpSupport.MaxDistance;
                    r1Face = i;
                    supportPoints = tmpSupport.SupportPoints.ToArray();
                }
            }

            //all four directions have support point. That means at least one point from r2 lies inside from r1
            return supportPoints.Select(r2Edge => new CollisionInfo(r2.Vertex[r2Edge] + r1.FaceNormal[r1Face] * bestDistance, r1.FaceNormal[r1Face], bestDistance, (byte)(r1Face + 4), r2Edge)).ToArray();
        }

        internal static CollisionInfo[] RectangleRectangle(ICollidable rectangle1, ICollidable rectangle2)
        {
            //Funktioniert auch außer beim Würfelstapel wo die Würfel exakt Ecke über Ecke stehen. Dort werden dann auch
            //Kollisionsnormalen erzeugt, die zur Seite zeigen.Deswegen fliegt der Stapel dann um.
            //Der Grund ist, weil der Eckpunkt von den einen Würfel dringt viel weniger in die Left/Right-Seite vom anderen Würfel
            //ein als aufgrund der Schwerkraft in die Top/Bottom-Seite. Die Schwerkraft läßt ein ruhigen Würfel um 0.8 Pixel nach
            //unten Bewegen. Wenn ich faceDistance -= 1; schreibe, dann wird die kleine Eindringtiefe in X-Richtung nicht mehr beachtet
            //und die Kollisionsnormalen zeigen dann alle nach oben. Somit steht der Stapel dann still. Würde ich aber die Schwerkraft
            //ändern oder die Time-Step-Weite, dann müsste ich faceDistance nicht nur um 1 Pixel verringern sondern um Gravity*DeltaT.
            //Da das aber alles wie rumgetweake wirkt, nutze ich einfach weiterhin diese Klasse hier für die Rectangle-Rectangle-Kollision.
            //return PolygonPolygonCollision.PolygonPolygon(rectangle1, rectangle2); 

            ICollidableRectangle r1 = (ICollidableRectangle)rectangle1;
            ICollidableRectangle r2 = (ICollidableRectangle)rectangle2;

            List<CollisionInfo> contacts = new List<CollisionInfo>();

            var collisionInfoR1 = FindAxisLeastPenetration(r1, r2);
            if (collisionInfoR1 != null)
                contacts.AddRange(collisionInfoR1.Select(x => new CollisionInfo(x.Start - x.Normal * x.Depth, x.Normal, x.Depth, x.StartEdgeIndex, x.EndEdgeIndex)));

            var collisionInfoR2 = FindAxisLeastPenetration(r2, r1);
            if (collisionInfoR2 != null)
                contacts.AddRange(collisionInfoR2.Select(x => new CollisionInfo(x.Start, -x.Normal, x.Depth, x.EndEdgeIndex, x.StartEdgeIndex)));

            if (contacts.Any() == false) return new CollisionInfo[0];

            //Beim Würfel-Stapelbeispiel erhalte ich doppelte einträge welche ich hier rausfiltere
            var withoutDoubles = contacts
                .GroupBy(x => x.Start.ToString() + x.Depth.ToString())
                .Select(x => x.First())
                .ToArray();

            return withoutDoubles;
        }
    }
}
