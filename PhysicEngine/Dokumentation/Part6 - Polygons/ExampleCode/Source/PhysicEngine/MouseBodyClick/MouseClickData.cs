using PhysicEngine.MathHelper;
using PhysicEngine.RigidBody;

namespace PhysicEngine.MouseBodyClick
{
    //Wird beim Mouse-Down-Event von der PhysicSecne erzeugt
    public class MouseClickData
    {
        public IPublicRigidBody RigidBody;
        public Vec2D LocalAnchorDirection;       //Angabe im Lokal-Space
        public Vec2D Position; //Position des Mausklicks in Weltkoordinaten
    }
}
