using GraphicMinimal;

namespace PhysicEngine.CollisionDetection.NearPhase
{
    //Wenn es eine Kollision zwischen zwei Rechtecken gibt, dann gibt diese Funktion immmer nur 1 Kollisionspunkt zurück.
    //Sie beachtet nicht, dass es auch eine Kollision zwischen zwei Kanten geben kann sondern nur Kante-Ecke
    //Quelle: https://github.com/Apress/building-a-2d-physics-game-engine/blob/master/978-1-4842-2582-0_source%20code/Chapter5/Chapter5.1ACoolDemo/public_html/RigidBody/Rectangle_collision.js
    public static class RectangleRectangleCollision1
    {
        struct SupportStruct
        {
            public SupportStruct(float supportPointDist) 
            { 
                this.SupportPointDist = supportPointDist;
            }

            public Vector2D SupportPoint = null;
            public float SupportPointDist = 0;            
        }

        private static SupportStruct FindSupportPoint(ICollidableRectangle r, Vector2D dir, Vector2D ptOnEdge)
        {
            var tmpSupport = new SupportStruct(-9999999);

            //check each vector of other object
            for (int i=0; i<r.Vertex.Length; i++)
            {
                //the longest project length
                float projection = (r.Vertex[i] - ptOnEdge) * dir;

                //find the longest distance with certain edge
                //dir is -n direction, so the distance should be positive      
                if (projection > 0 && projection > tmpSupport.SupportPointDist)
                {
                    tmpSupport.SupportPoint = r.Vertex[i];
                    tmpSupport.SupportPointDist = projection;
                }
            }

            return tmpSupport;
        }

        private static CollisionInfo FindAxisLeastPenetration(ICollidableRectangle r1, ICollidableRectangle r2)
        {
            float bestDistance = 999999;
            int bestIndex = -1;
            Vector2D supportPoint = null;

            for (int i=0;i<r1.FaceNormal.Length;i++)
            {
                // Retrieve a face normal from A
                var n = r1.FaceNormal[i];

                // use -n as direction and the vectex on edge i as point on edge

                // find the support on B
                // the point has longest distance with edge i 
                var tmpSupport = FindSupportPoint(r2, -n, r1.Vertex[i]);

                //SAT says if one side from r1 has no support-Point on r2, then there is no collision
                if (tmpSupport.SupportPoint == null) return null; 

                //get the shortest support point depth
                if (tmpSupport.SupportPointDist < bestDistance)
                {
                    bestDistance = tmpSupport.SupportPointDist;
                    bestIndex = i;
                    supportPoint = tmpSupport.SupportPoint;
                }
            }

            //all four directions have support point. That means at least one point from r2 lies inside from r1
            return new CollisionInfo(supportPoint + r1.FaceNormal[bestIndex] * bestDistance, r1.FaceNormal[bestIndex], bestDistance);
        }

        public static CollisionInfo RectangleRectangle(ICollidableRectangle r1, ICollidableRectangle r2)
        {
            //find Axis of Separation for both rectangle
            var collisionInfoR1 = FindAxisLeastPenetration(r1 , r2);

            if (collisionInfoR1 != null)
            {
                var collisionInfoR2 = FindAxisLeastPenetration(r2 , r1);
                if (collisionInfoR2 != null)
                {
                    //if both of rectangles are overlapping, choose the shorter normal as the normal
                    if (collisionInfoR1.Depth < collisionInfoR2.Depth)
                        return new CollisionInfo(collisionInfoR1.Start - collisionInfoR1.Normal * collisionInfoR1.Depth, collisionInfoR1.Normal, collisionInfoR1.Depth);
                    else
                        return new CollisionInfo(collisionInfoR2.Start, -collisionInfoR2.Normal, collisionInfoR2.Depth);
                }
            }

            return null;
        }
    }
}
