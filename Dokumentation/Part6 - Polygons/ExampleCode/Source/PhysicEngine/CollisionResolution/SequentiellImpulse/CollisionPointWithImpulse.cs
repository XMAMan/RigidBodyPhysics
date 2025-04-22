using PhysicEngine.CollisionDetection;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse
{
    //Speichert den Normal- und Friction-Impulse-Wert für ein Kollisionspunkt
    internal class CollisionPointWithImpulse : RigidBodyCollision
    {
        internal float NormalImpulse = 0;
        internal float FrictionImpulse = 0;

        internal CollisionPointWithImpulse(RigidBodyCollision c)
            : base(c)
        {
        }

        internal void TakeDataFromOtherPoint(CollisionPointWithImpulse c)
        {
            this.NormalImpulse = c.NormalImpulse;
            this.FrictionImpulse = c.FrictionImpulse;
        }
    }
}
