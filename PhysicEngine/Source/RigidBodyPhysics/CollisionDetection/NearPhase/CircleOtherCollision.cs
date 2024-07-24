using RigidBodyPhysics.CollisionDetection.NearPhase.Polygon;
using RigidBodyPhysics.MathHelper;

namespace RigidBodyPhysics.CollisionDetection.NearPhase
{
    //Schnittpunkt zwischen Kreis und Kreis/Rechteck
    internal static class CircleOtherCollision
    {
        //Quelle: https://github.com/Apress/building-a-2d-physics-game-engine/blob/master/978-1-4842-2582-0_source%20code/Chapter5/Chapter5.1ACoolDemo/public_html/RigidBody/Circle_collision.js
        internal static CollisionInfo[] CircleCircle(ICollidable circle1, ICollidable circle2)
        {
            ICollidableCircle c1 = (ICollidableCircle)circle1;
            ICollidableCircle c2 = (ICollidableCircle)circle2;

            Vec2D from1to2 = c2.Center - c1.Center;
            float rSum = c1.Radius + c2.Radius;
            float dist = from1to2.Length();

            if (dist != 0)
            {
                // Vec2D but not same position
                Vec2D normal = from1to2 / dist;
                return new CollisionInfo[] { new CollisionInfo(c1.Center + normal * c1.Radius, normal, rSum - dist, 0, 0) };
            }
            else
            {
                //same position
                Vec2D normal = new Vec2D(1, 0);
                return new CollisionInfo[] { new CollisionInfo(c1.Center + normal * c1.Radius, normal, rSum, 0, 0) };
            }
        }

        internal static CollisionInfo[] CircleRectangle(ICollidable circle, ICollidable rectangle)
        {
            var collision = RectangleOtherCollision.RectangleCircle((ICollidableRectangle)rectangle, (ICollidableCircle)circle);
            return collision.Select(x => x.Swap()).ToArray();
        }

        internal static CollisionInfo[] CircleEdge(ICollidable circle, ICollidable edge)
        {
            var collision = EdgeOtherCollision.EdgeCircle((ICollideablePolygonEdge)edge, (ICollidableCircle)circle);
            return collision.Select(x => x.Swap()).ToArray();
        }

        internal static CollisionInfo[] CircleConvexPolygon(ICollidable circle, ICollidable polygon)
        {
            var collision = ConvexPolygonOtherCollision.PolygonCircle((IConvexSubPolygon)polygon, (ICollidableCircle)circle);
            return collision.Select(x => x.Swap()).ToArray();
        }
    }
}
