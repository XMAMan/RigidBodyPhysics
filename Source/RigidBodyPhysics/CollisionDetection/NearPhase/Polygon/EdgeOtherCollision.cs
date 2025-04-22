using RigidBodyPhysics.MathHelper;

namespace RigidBodyPhysics.CollisionDetection.NearPhase.Polygon
{
    //All die Nearphase-Tests zwischen eine Linie (geht von P1 nach P2) und ein Kreis/Rechteck/Polygon/Polygonkante
    internal static class EdgeOtherCollision
    {
        internal static CollisionInfo[] EdgeCircle(ICollidable edge, ICollidable circle)
        {
            ICollideablePolygonEdge ed = (ICollideablePolygonEdge)edge;
            ICollidableCircle ci = (ICollidableCircle)circle;

            //Nur wenn das Kreis-Zentrum vor der Kante liegt wird ein Test gemacht
            Vec2D p1ToC = ci.Center - ed.P1;
            float normalDistance = p1ToC * ed.Normal;
            if (normalDistance > ci.Radius) return new CollisionInfo[0];
            if (normalDistance < 0) return new CollisionInfo[0]; //Keine Kollision mit der Linie, wenn das Kreiszentrum hinter der Linie liegt

            Vec2D p1 = ed.P1;
            Vec2D p2 = ed.P2;
            Vec2D m = ci.Center;
            float radius = ci.Radius;

            Vec2D V = p2 - p1;
            float a = p1.X - m.X, b = p1.Y - m.Y, c = 2 * a * V.X + 2 * b * V.Y, d = V.X * V.X + V.Y * V.Y, e = a * a + b * b - radius * radius;
            float f1 = (float)Math.Sqrt(c * c - 4 * d * e), f2 = 1.0f / (2 * d);
            float t1 = (-c + f1) * f2, t2 = (-c - f1) * f2;
            if (t1 >= 0 && t1 <= 1 || t2 >= 0 && t2 <= 1)
            {
                return new CollisionInfo[] { new CollisionInfo(ci.Center - ed.Normal * ci.Radius, ed.Normal, ci.Radius - normalDistance, 0, 0) };
            }

            return new CollisionInfo[0];
        }

        internal static CollisionInfo[] EdgeConvexPolygon(ICollidable collidableEdge, ICollidable poly)
        {
            ICollideablePolygonEdge edge = (ICollideablePolygonEdge)collidableEdge;
            IConvexPolygon polygon = (IConvexPolygon)poly;

            List<CollisionInfo> collisions = new List<CollisionInfo>();

            //Schritt 1: Prüfe, welche Eckpunkte vom Polygon hinter der Edge-Kante liegen.
            //Mache die Prüfung aber nur, wenn das Polygonentrum vor der Kante liegt. Grund: Das Polygon soll an spitzer EdgePolygonkante nicht 
            //von der gegenüberliegenden Kantenunterseite nach unten gezeogen werden
            Vec2D polygonCenter = polygon.Center;
            if (polygon is IConvexSubPolygon) polygonCenter = (polygon as IConvexSubPolygon).CenterFromParentPolygon;
            Vec2D p1ToC = polygonCenter - edge.P1;
            float normalDistance = p1ToC * edge.Normal;

            //Nur wenn das Polygon-Zentrum vor der Kante liegt wird ein Test gemacht
            if (normalDistance > 0)
            {
                //Prüfe für jeden Ecke vom Polygon, ob er hinter der Kante liegt
                for (int i = 0; i < polygon.Vertex.Length; i++)
                {
                    Vec2D corner = polygon.Vertex[i];
                    Vec2D p1ToCorner = corner - edge.P1;
                    float distanceToEdge = p1ToCorner * edge.Normal;
                    if (distanceToEdge < 0)
                    {
                        float tangentDistance = p1ToCorner * edge.P1ToP2Direction;
                        if ((tangentDistance > edge.Min && tangentDistance < edge.Max))
                        {
                            collisions.Add(new CollisionInfo(corner, edge.Normal, -distanceToEdge, 0, (byte)i));
                        }
                    }
                }
            }

            //Schritt 2: Prüfe per SAT, ob der Edge-P1-Punkt innerhalb des Polygon liegt.
            //Mach das aber nur, wenn er eine nach außen zeigende Spitze ist.
            if (edge.IsP1Convex)
            {
                bool p1IsInsideFromRectangle = true;
                int minFaceIndex = -1;
                float maxFaceDistance = float.MinValue;

                for (int i = 0; i < polygon.Vertex.Length; i++)
                {
                    Vec2D corner = polygon.Vertex[i];
                    Vec2D p1ToCorner = edge.P1 - corner;

                    //Prüfe für den P1-Edgepunkt, ob er innerhalb des Polygon liegt
                    float faceDistance = p1ToCorner * polygon.FaceNormal[i];
                    if (faceDistance > 0)
                    {
                        p1IsInsideFromRectangle = false;
                        break; //Abbruch da es kein Schnittpunkt geben kann
                    }
                    else
                    {
                        if (faceDistance > maxFaceDistance)
                        {
                            maxFaceDistance = faceDistance;
                            minFaceIndex = i;
                        }
                    }
                }

                if (p1IsInsideFromRectangle)
                {
                    collisions.Add(new CollisionInfo(edge.P1, -polygon.FaceNormal[minFaceIndex], -maxFaceDistance, (byte)minFaceIndex, 0));
                }
            }


            return collisions.ToArray();
        }

        //Prüfe, ob edge2.P1 hinter der edge1-Linie liegt. Mach das aber nur, wenn edge2.Center vor edge1 liegt
        internal static CollisionInfo[] EdgeEdge(ICollidable e1, ICollidable e2)
        {
            ICollideablePolygonEdge edge1 = (ICollideablePolygonEdge)e1;
            ICollideablePolygonEdge edge2 = (ICollideablePolygonEdge)e2;

            List<CollisionInfo> collisions = new List<CollisionInfo>();

            Vec2D p1ToC = edge2.Center - edge1.P1;
            float normalDistance = p1ToC * edge1.Normal;

            //Nur wenn das Polygon-Zentrum was zu edge2 gehört vor der edge1-Kante liegt wird ein Test gemacht
            if (normalDistance > 0)
            {
                Vec2D corner = edge2.P1;
                Vec2D p1ToCorner = corner - edge1.P1;
                float distanceToEdge = p1ToCorner * edge1.Normal;
                if (distanceToEdge < 0)
                {
                    float tangentDistance = p1ToCorner * edge1.P1ToP2Direction;
                    if (tangentDistance > 0 && tangentDistance < edge1.Length)
                    {
                        collisions.Add(new CollisionInfo(corner, edge1.Normal, -distanceToEdge, 0, 0));
                    }
                }
            }

            return collisions.ToArray();
        }
    }
}
