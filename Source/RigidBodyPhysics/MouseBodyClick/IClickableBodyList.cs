using RigidBodyPhysics.MathHelper;

namespace RigidBodyPhysics.MouseBodyClick
{
    internal interface IClickableBodyList
    {
        MouseClickData TryToGetBodyWithMouseClick(Vec2D mousePosition);
        void SetMouseConstraint(MouseClickData mouseClick, MouseConstraintUserData userData); //Maus wird geklickt und hält ein Körper fest        
        void ClearMouseConstraint(); //Maus wird losgelassen
        void UpdateMousePosition(Vec2D mousePosition);
    }
}
