using PhysicEngine.MathHelper;

namespace PhysicEngine.CollisionDetection.NearPhase.Polygon
{
    internal class PolygonPolygonCollision
    {
        private static CollisionInfo[] GetAllPointsFromPoly2WhichAreInsideInPoly1(IConvexPolygon poly1, IConvexPolygon poly2, bool swap)
        {
            List<int> p2Indizes = new List<int>(); //Liste all der Poly2-Punkte, welcher innerhalb von Poly1 liegen
            for (int i = 0; i < poly2.Vertex.Length; i++)
                p2Indizes.Add(i);

            float maxFaceDistance = float.MinValue;
            int minFaceIndex = -1;

            for (int i = 0; i < poly1.Vertex.Length; i++)
            {
                Vec2D faceNormal = poly1.FaceNormal[i];
                Vec2D faceP1 = poly1.Vertex[i];

                List<int> p2PointsNotInPoly1 = new List<int>();
                float minDistance = float.MaxValue; //Wie weit ist der entfernteste Punkt von (Poly1-Kante i) weg?
                foreach (int j in p2Indizes)
                {
                    float faceDistance = (poly2.Vertex[j] - faceP1) * faceNormal; //faceDistance ist negativ, wenn der Poly2-Punkt im Poly1 ist

                    if (faceDistance > 0)           //Punkt j liegt außerhalb der Poly1-Kante i?
                    {                               
                        p2PointsNotInPoly1.Add(j);  //Wenn ja, dann entferne ihn aus p2Indizes. 
                    }
                    else
                    {
                        if (faceDistance < minDistance) //Liegt Punkt j am Meisten innerhalb der aktuellen Poly1-Seite i?
                        {
                            minDistance = faceDistance;
                        }
                    }
                }
                foreach (int j in p2PointsNotInPoly1)
                    p2Indizes.Remove(j);

                if (p2Indizes.Any() == false) return new CollisionInfo[0]; //Kein Punkt von Poly2 liegt in Poly1

                //Bei welche Face-Seite von Poly1 dringen die Punkte am wenigsten ein?
                //Nimm nur dann eine Polygonkante als potenzielle Wegstoßkante, wenn sie eine OutsideEdge ist
                bool face1IsInnerFace = poly1 is IConvexSubPolygon && (poly1 as IConvexSubPolygon).IsOutsideEdge[i] == false;
                if (face1IsInnerFace == false && minDistance > maxFaceDistance)
                {
                    maxFaceDistance = minDistance;
                    minFaceIndex = i;
                }
            }

            if (minFaceIndex == -1) return new CollisionInfo[0]; //Kein Normale gefunden(kann passieren, wenn is InsideEdges sind)

            //Die Punkte von Poly2, welcher innerhalb von Poly1 liegen haben die kleinste Eindringtiefe bei Poly1-Seite minFaceIndex
            Vec2D face1Normal = poly1.FaceNormal[minFaceIndex]; //In diese Richtung werden all die gefundenen Poly2-Punkte gedrückt
            Vec2D face1P1 = poly1.Vertex[minFaceIndex];

            List<CollisionInfo> collisions = new List<CollisionInfo>();
            if (swap == false) //Normale zeigt von Poly1 so, dass Poly2 in diese Richtung gedrückt werden soll
            {
                foreach (int i in p2Indizes)
                {
                    Vec2D poly2Point = poly2.Vertex[i];
                    float depth = (face1P1 - poly2Point) * face1Normal; //Positive Number
                    collisions.Add(new CollisionInfo(poly2Point, face1Normal, depth, (byte)(poly1.Vertex.Length + minFaceIndex), (byte)i));
                }
            }
            else //Normale zeigt von Poly2 nach Poly1. Also muss sie hier noch getauscht werden
            {
                foreach (int i in p2Indizes)
                {
                    Vec2D poly2Point = poly2.Vertex[i];
                    float depth = (face1P1 - poly2Point) * face1Normal; //Positive Number
                    collisions.Add(new CollisionInfo(poly2Point + face1Normal * depth, -face1Normal, depth, (byte)(poly1.Vertex.Length + minFaceIndex), (byte)i));
                }
            }

            return collisions.ToArray();
        }

        internal static CollisionInfo[] PolygonPolygon(ICollidable polygon1, ICollidable polygon2)
        {
            IConvexPolygon poly1 = (IConvexPolygon)polygon1;
            IConvexPolygon poly2 = (IConvexPolygon)polygon2;

            List<CollisionInfo> contacts = new List<CollisionInfo>();

            //Es ist möglich, dass Poly2-Punkte in Poly1 sich aufhalten aber in Poly2 befindet sich kein Poly1-Punkt
            //Deswegen ist es kein Abbruchgrund, wenn p2Points oder p1Points keine Punkte enthält ist.
            var p2Points = GetAllPointsFromPoly2WhichAreInsideInPoly1(poly1, poly2, false);
            contacts.AddRange(p2Points);

            var p1Points = GetAllPointsFromPoly2WhichAreInsideInPoly1(poly2, poly1, true);
            contacts.AddRange(p1Points);

            return contacts.ToArray();
        }
    }
}
