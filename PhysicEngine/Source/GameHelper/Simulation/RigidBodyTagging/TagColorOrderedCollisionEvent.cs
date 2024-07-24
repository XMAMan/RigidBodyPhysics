using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace GameHelper.Simulation.RigidBodyTagging
{
    //Gibt in TagColor-sortierter Reihenfolge die Kollision zweier RigidBodys zurück
    public class TagColorOrderedCollisionEvent
    {
        public IPublicRigidBody Body1 { get; }
        public IPublicRigidBody Body2 { get; }
        public byte Color1 { get; }
        public byte Color2 { get; }

        public TagColorOrderedCollisionEvent(IPublicRigidBody body1, IPublicRigidBody body2, byte color1, byte color2)
        {
            if (color1 <= color2)
            {
                Body1 = body1;
                Body2 = body2;
                Color1 = color1;
                Color2 = color2;
            }
            else
            {
                Body1 = body2;
                Body2 = body1;
                Color1 = color2;
                Color2 = color1;
            }
        }
    }
}
