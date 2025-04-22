using PhysicEngine.MathHelper;

namespace PhysicEngine.CollisionDetection.NearPhase
{
    //Quelle: https://github.com/Apress/building-a-2d-physics-game-engine/blob/master/978-1-4842-2582-0_source%20code/Chapter5/Chapter5.1ACoolDemo/public_html/Lib/CollisionInfo.js
    //Idee diese Klasse um StartEdgeIndex und EndEdgeIndex zu erweitern: Box2D-Light Contact.FeaturePair
    class CollisionInfo
    {
        public Vec2D Start { get; }  //Collisionpoint from RigidBody1
        public Vec2D End { get; }    //Collisionpoint from RigidBody2
        public Vec2D Normal { get; } //Normal from Start-Point
        internal float Depth { get; }     //Distanz between Start and End

        internal readonly byte StartEdgeIndex; //Beim Rechteck: Index von der Ecke oder Face-Seite, wo der Kontaktpunkt liegt
        internal readonly byte EndEdgeIndex;   //Beim Kreis steht hier immer nur 0

        internal CollisionInfo(Vec2D start, Vec2D normal, float depth, byte startEdgeIndex, byte endEdgeIndex)
            : this(start, start + normal * depth, normal, depth, startEdgeIndex, endEdgeIndex)
        {            
        }

        protected CollisionInfo(Vec2D start, Vec2D end, Vec2D normal, float depth, byte startEdgeIndex, byte endEdgeIndex)
        {
            this.Start = start;
            this.End = end;
            this.Normal = normal;
            this.Depth = depth;
            this.StartEdgeIndex = startEdgeIndex;
            this.EndEdgeIndex = endEdgeIndex;
        }

        internal CollisionInfo Swap()
        {
            return new CollisionInfo(End, Start, -Normal, Depth, EndEdgeIndex, StartEdgeIndex);
        }
    }
}
