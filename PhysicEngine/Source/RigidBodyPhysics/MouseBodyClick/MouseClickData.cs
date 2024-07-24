using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace RigidBodyPhysics.MouseBodyClick
{
    //Wird beim Mouse-Down-Event von der PhysicSecne erzeugt
    public class MouseClickData
    {
        public IPublicRigidBody RigidBody;
        public Vec2D LocalAnchorDirection;       //Angabe im Lokal-Space
        public Vec2D Position; //Position des Mausklicks in Weltkoordinaten
    }
}
