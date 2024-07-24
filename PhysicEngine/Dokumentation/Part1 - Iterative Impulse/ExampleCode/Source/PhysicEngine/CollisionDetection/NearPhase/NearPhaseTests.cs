using GraphicMinimal;

namespace PhysicEngine.CollisionDetection.NearPhase
{
    public static class NearPhaseTests
    {
        public static CollisionInfo CircleOther(ICollidableCircle c, ICollidable o)
        {
            if (o is ICollidableCircle)
                return CircleCircle(c, (ICollidableCircle)o);
            else
            {
                var collision = RectangleCircle((ICollidableRectangle)o, c);
                if (collision == null) return null;
                return collision.Swap();
            }
        }

        public static CollisionInfo[] RectangleOther(ICollidableRectangle r, ICollidable o)
        {
            if (o is ICollidableCircle)
            {
                var collision = RectangleCircle(r, (ICollidableCircle)o);
                if (collision == null) return null;
                return new CollisionInfo[] { collision };
            }                 
            else
                return RectangleRectangle(r, (ICollidableRectangle)o);
        }

        //Quelle: https://github.com/Apress/building-a-2d-physics-game-engine/blob/master/978-1-4842-2582-0_source%20code/Chapter5/Chapter5.1ACoolDemo/public_html/RigidBody/Circle_collision.js
        public static CollisionInfo CircleCircle(ICollidableCircle c1, ICollidableCircle c2)
        {
            Vector2D from1to2 = c2.Center - c1.Center;
            float rSum = c1.Radius + c2.Radius;
            float dist = from1to2.Length();
            
            if (dist != 0)
            {
                // overlapping but not same position
                Vector2D normal = from1to2 / dist;
                return new CollisionInfo(c1.Center + normal * c1.Radius, normal, rSum - dist);
            }
            else
            {
                //same position
                Vector2D normal = new Vector2D(1, 0);
                return new CollisionInfo(c1.Center + normal * c1.Radius, normal, rSum);
            }
        }

        //Quelle: https://github.com/Apress/building-a-2d-physics-game-engine/blob/master/978-1-4842-2582-0_source%20code/Chapter5/Chapter5.1ACoolDemo/public_html/RigidBody/Rectangle_collision.js
        public static CollisionInfo RectangleCircle(ICollidableRectangle r, ICollidableCircle c)
        {
            bool inside = true;
            float bestDistance = -99999;
            int nearestEdge = 0;

            for (int i=0;i<4;i++)
            {
                //find the nearest face for center of circle 
                float projection = (c.Center - r.Vertex[i]) * r.FaceNormal[i];
                if (projection > 0)
                {
                    //if the center of circle is outside of rectangle
                    bestDistance = projection;
                    nearestEdge = i;
                    inside = false;
                    break;
                }
                if (projection > bestDistance)
                {
                    bestDistance = projection;
                    nearestEdge = i;
                }
            }

            if (inside==false)
            {
                //the center of circle is outside of rectangle

                //v1 is from left vertex of face to center of circle 
                //v2 is from left vertex of face to right vertex of face
                Vector2D v1 = c.Center - r.Vertex[nearestEdge];
                Vector2D v2 = r.Vertex[(nearestEdge + 1) % 4] - r.Vertex[nearestEdge];

                float dot = v1 * v2;

                if (dot < 0)
                {
                    //the center of circle is in corner region of r.Vertex[nearestEdge]
                    float dis = v1.Length();

                    // compare the distance with radius to decide collision
                    if (dis > c.Radius)
                        return null;

                    Vector2D normal = v1.Normalize();
                    return new CollisionInfo(c.Center - normal * c.Radius, normal, c.Radius - dis); //Corner 1
                }else
                {
                    //the center of circle is in corner region of mVertex[nearestEdge+1]

                    //v1 is from right vertex of face to center of circle 
                    //v2 is from right vertex of face to left vertex of face
                    v1 = c.Center - r.Vertex[(nearestEdge+1) % 4];
                    v2 = -v2;
                    dot = v1 * v2;
                    if (dot < 0)
                    {
                        float dis = v1.Length();
                        //compare the distance with radium to decide collision
                        if (dis > c.Radius)
                            return null;
                        Vector2D normal = v1.Normalize();
                        return new CollisionInfo(c.Center - normal * c.Radius, normal, c.Radius - dis); //Corner 2
                    }else
                    {
                        //the center of circle is in face region of face[nearestEdge]
                        if (bestDistance < c.Radius)
                            return new CollisionInfo(c.Center - r.FaceNormal[nearestEdge] * c.Radius, r.FaceNormal[nearestEdge], c.Radius - bestDistance); //Face
                        else
                            return null;
                    }
                }
            }else
            {
                //the center of circle is inside of rectangle
                return new CollisionInfo(c.Center - r.FaceNormal[nearestEdge] * c.Radius, r.FaceNormal[nearestEdge], c.Radius - bestDistance);
            }
        }

        public static CollisionInfo[] RectangleRectangle(ICollidableRectangle r1, ICollidableRectangle r2)
        {
            //Variante 1 aus dem Buch building a 2d physics game engine
            //var collision = RectangleRectangleCollision1.RectangleRectangle(r1, r2);
            //if (collision == null) return null;
            //return new CollisionInfo[] { collision };

            //Variante 2 wie aus dem Buch building a 2d physics game engine aber so von mir modifziert, dass sie 2 Kontaktpunkte zurück gibt
            return RectangleRectangleCollision2.RectangleRectangle(r1, r2);

            //Variante 3 aus Box2D-Lite (Geht nicht da Originalcode viele Fehler enthält)
            //return RectangleRectangleCollision3.RectangleRectangle(r1, r2);
        }
    }
}
