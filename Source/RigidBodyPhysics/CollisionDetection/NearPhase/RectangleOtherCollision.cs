using RigidBodyPhysics.CollisionDetection.NearPhase.Polygon;
using RigidBodyPhysics.MathHelper;

namespace RigidBodyPhysics.CollisionDetection.NearPhase
{
    internal static class RectangleOtherCollision
    {
        //Quelle: https://github.com/Apress/building-a-2d-physics-game-engine/blob/master/978-1-4842-2582-0_source%20code/Chapter5/Chapter5.1ACoolDemo/public_html/RigidBody/Rectangle_collision.js
        internal static CollisionInfo[] RectangleCircle(ICollidable rectangle, ICollidable circle)
        {
            ICollidableRectangle r = (ICollidableRectangle)rectangle;
            ICollidableCircle c = (ICollidableCircle)circle;

            bool inside = true;
            float bestDistance = -99999;
            int nearestEdge = 0;

            for (int i = 0; i < 4; i++)
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

            if (inside == false)
            {
                //the center of circle is outside of rectangle

                //v1 is from left vertex of face to center of circle 
                //v2 is from left vertex of face to right vertex of face
                Vec2D v1 = c.Center - r.Vertex[nearestEdge];
                Vec2D v2 = r.Vertex[(nearestEdge + 1) % 4] - r.Vertex[nearestEdge];

                float dot = v1 * v2;

                if (dot < 0)
                {
                    //the center of circle is in corner region of r.Vertex[nearestEdge]
                    float dis = v1.Length();

                    // compare the distance with radius to decide collision
                    if (dis > c.Radius)
                        return new CollisionInfo[0];

                    Vec2D normal = v1.Normalize();
                    return new CollisionInfo[] { new CollisionInfo(c.Center - normal * c.Radius, normal, c.Radius - dis, 0, 0) }; //Corner 1
                }
                else
                {
                    //the center of circle is in corner region of mVertex[nearestEdge+1]

                    //v1 is from right vertex of face to center of circle 
                    //v2 is from right vertex of face to left vertex of face
                    v1 = c.Center - r.Vertex[(nearestEdge + 1) % 4];
                    v2 = -v2;
                    dot = v1 * v2;
                    if (dot < 0)
                    {
                        float dis = v1.Length();
                        //compare the distance with radium to decide collision
                        if (dis > c.Radius)
                            return new CollisionInfo[0];
                        Vec2D normal = v1 / dis;
                        return new CollisionInfo[] { new CollisionInfo(c.Center - normal * c.Radius, normal, c.Radius - dis, 0, 0) }; //Corner 2
                    }
                    else
                    {
                        //the center of circle is in face region of face[nearestEdge]
                        if (bestDistance < c.Radius)
                            return new CollisionInfo[] { new CollisionInfo(c.Center - r.FaceNormal[nearestEdge] * c.Radius, r.FaceNormal[nearestEdge], c.Radius - bestDistance, 0, 0) }; //Face
                        else
                            return new CollisionInfo[0];
                    }
                }
            }
            else
            {
                //the center of circle is inside of rectangle
                return new CollisionInfo[] { new CollisionInfo(c.Center - r.FaceNormal[nearestEdge] * c.Radius, r.FaceNormal[nearestEdge], c.Radius - bestDistance, 0, 0) };
            }
        }

        internal static CollisionInfo[] RectangleEdge(ICollidable rectangle, ICollidable edge)
        {
            var collision = EdgeOtherCollision.EdgeConvexPolygon((ICollideablePolygonEdge)edge, (ICollidableRectangle)rectangle);
            return collision.Select(x => x.Swap()).ToArray();
        }

        internal static CollisionInfo[] RectangleConvexPolygon(ICollidable rectangle, ICollidable polygon)
        {
            var collision = ConvexPolygonOtherCollision.PolygonRectangle((IConvexSubPolygon)polygon, (ICollidableRectangle)rectangle);
            return collision.Select(x => x.Swap()).ToArray();
        }
    }
}
