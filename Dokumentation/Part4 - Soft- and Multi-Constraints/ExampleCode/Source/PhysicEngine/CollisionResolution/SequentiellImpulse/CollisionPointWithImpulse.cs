using PhysicEngine.CollisionDetection;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse
{
    //Speichert den Normal- und Friction-Impulse-Wert für ein Kollisionspunkt
    internal class CollisionPointWithImpulse : RigidBodyCollision
    {
        public float NormalImpulse = 0;
        public float FrictionImpulse = 0;

        public CollisionPointWithImpulse(RigidBodyCollision c)
            : base(c)
        {
        }

        public void TakeDataFromOtherPoint(CollisionPointWithImpulse c)
        {
            this.NormalImpulse = c.NormalImpulse;
            this.FrictionImpulse = c.FrictionImpulse;
        }
    }
}
