using RigidBodyPhysics.MathHelper;

namespace RigidBodyPhysics.CollisionDetection.NearPhase.Polygon
{
    internal class ConvexPolygonOtherCollision
    {
        //Quelle: https://github.com/tutsplus/ImpulseEngine/blob/master/Collision.cpp#L68 -> Ich habe es noch etwas optimiert
        internal static CollisionInfo[] PolygonCircle(ICollidable polygon, ICollidable circle)
        {
            IConvexSubPolygon p = (IConvexSubPolygon)polygon;
            ICollidableCircle c = (ICollidableCircle)circle;

            float maxSeparation = float.MinValue;
            int faceIndex = 0;
            for (int i = 0; i < p.Vertex.Length; i++)
            {
                float s = (c.Center - p.Vertex[i]) * p.FaceNormal[i];

                if (s > c.Radius)
                    return new CollisionInfo[0]; //There is a Separating axis so there can not be a Collision

                if (s > maxSeparation)
                {
                    maxSeparation = s;
                    faceIndex = i;
                }
            }

            // Grab face's vertices
            Vec2D v1 = p.Vertex[faceIndex];
            Vec2D v2 = p.Vertex[(faceIndex + 1) % p.Vertex.Length];

            // Check to see if center is within polygon
            if (maxSeparation < 0)
            {
                return new CollisionInfo[] { new CollisionInfo(c.Center - p.FaceNormal[faceIndex] * c.Radius, p.FaceNormal[faceIndex], c.Radius - maxSeparation, (byte)faceIndex, 0) };
            }

            float dot1 = (c.Center - v1) * (v2 - v1);

            // Closest to v1
            if (dot1 <= 0)
            {
                float distSqr = (c.Center - v1).SquareLength();
                if (distSqr > c.Radius * c.Radius)
                    return new CollisionInfo[0];

                float normalLength = (float)Math.Sqrt(distSqr);
                Vec2D normal = (c.Center - v1) / normalLength;
                return new CollisionInfo[] { new CollisionInfo(c.Center - normal * c.Radius, normal, c.Radius - normalLength, (byte)faceIndex, 0) }; //Corner 1
            }

            float dot2 = (c.Center - v2) * (v1 - v2);

            // Closest to v2
            if (dot2 <= 0)
            {
                float distSqr = (c.Center - v2).SquareLength();
                if (distSqr > c.Radius * c.Radius)
                    return new CollisionInfo[0];

                float normalLength = (float)Math.Sqrt(distSqr);
                Vec2D normal = (c.Center - v2) / normalLength;
                return new CollisionInfo[] { new CollisionInfo(c.Center - normal * c.Radius, normal, c.Radius - normalLength, (byte)(faceIndex + 1), 0) }; //Corner 1
            }

            // Closest to face
            return new CollisionInfo[] { new CollisionInfo(c.Center - p.FaceNormal[faceIndex] * c.Radius, p.FaceNormal[faceIndex], c.Radius - maxSeparation, (byte)faceIndex, 0) }; //Face
        }

        internal static CollisionInfo[] PolygonRectangle(ICollidable polygon, ICollidable rectangle)
        {
            return PolygonPolygonCollision.PolygonPolygon(polygon, rectangle);
        }

        internal static CollisionInfo[] PolygonEdge(ICollidable polygon, ICollidable edge)
        {
            return EdgeOtherCollision.EdgeConvexPolygon(edge, polygon).Select(x => x.Swap()).ToArray();
        }

        internal static CollisionInfo[] PolygonPolygon(ICollidable poly1, ICollidable poly2)
        {
            return PolygonPolygonCollision.PolygonPolygon(poly1, poly2);
        }
    }
}
