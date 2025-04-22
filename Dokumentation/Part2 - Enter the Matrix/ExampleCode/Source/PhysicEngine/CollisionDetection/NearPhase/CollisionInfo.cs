using GraphicMinimal;

namespace PhysicEngine.CollisionDetection.NearPhase
{
    //Quelle: https://github.com/Apress/building-a-2d-physics-game-engine/blob/master/978-1-4842-2582-0_source%20code/Chapter5/Chapter5.1ACoolDemo/public_html/Lib/CollisionInfo.js
    //Idee diese Klasse um StartEdgeIndex und EndEdgeIndex zu erweitern: Box2D-Light Contact.FeaturePair
    public class CollisionInfo
    {
        public Vector2D Start;  //Collisionpoint from RigidBody1
        public Vector2D End;    //Collisionpoint from RigidBody2
        public Vector2D Normal; //Normal from Start-Point
        public float Depth;     //Distanz between Start and End
  
        public CollisionInfo(Vector2D start, Vector2D normal, float depth)
            : this(start, start + normal * depth, normal, depth)
        {            
        }

        protected CollisionInfo(Vector2D start, Vector2D end, Vector2D normal, float depth)
        {
            this.Start = start;
            this.End = end;
            this.Normal = normal;
            this.Depth = depth;
        }

        public CollisionInfo Swap()
        {
            return new CollisionInfo(End, Start, -Normal, Depth);
        }
    }
}
