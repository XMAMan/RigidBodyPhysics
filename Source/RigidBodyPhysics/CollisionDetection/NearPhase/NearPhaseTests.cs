using RigidBodyPhysics.CollisionDetection.NearPhase.Polygon;

namespace RigidBodyPhysics.CollisionDetection.NearPhase
{
    static class NearPhaseTests
    {
        delegate CollisionInfo[] CollideWith(ICollidable c1, ICollidable c2);

        private static readonly CollideWith[,] CollideMatrix = new CollideWith[,]
        {
            //              Circle                                      Rectangle                                       Edge                                    Polygon
            /*Circle*/    { CircleOtherCollision.CircleCircle,          CircleOtherCollision.CircleRectangle,           CircleOtherCollision.CircleEdge,        CircleOtherCollision.CircleConvexPolygon},
            /*Rectangle*/ { RectangleOtherCollision.RectangleCircle,    RectangleRectangleCollision.RectangleRectangle, RectangleOtherCollision.RectangleEdge,  RectangleOtherCollision.RectangleConvexPolygon },
            /*Edge*/      { EdgeOtherCollision.EdgeCircle,              EdgeOtherCollision.EdgeConvexPolygon,           EdgeOtherCollision.EdgeEdge,            EdgeOtherCollision.EdgeConvexPolygon},
            /*Polygon*/   { ConvexPolygonOtherCollision.PolygonCircle,  ConvexPolygonOtherCollision.PolygonRectangle,   ConvexPolygonOtherCollision.PolygonEdge,ConvexPolygonOtherCollision.PolygonPolygon}
        };

        internal static CollisionInfo[] Collide(ICollidable c1, ICollidable c2)
        {
            var func = CollideMatrix[(int)c1.TypeId, (int)c2.TypeId];
            return func(c1, c2);
        }
    }
}
