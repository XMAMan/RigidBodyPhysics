using LevelEditorGlobal;

namespace LevelEditorControl.LevelItems.PhysicItem
{
    //Hiermit bekommt ein anklickbarer RigidBody noch eine CollisionCategory
    internal class MouseClickableWithCollision : MouseClickableDecorator, ICollidable
    {
        private ICollidable decoree;

        public MouseClickableWithCollision(IMouseClickable decoree, RotatedRectangle rotatedRectangle)
            : base(decoree, rotatedRectangle)
        {
            this.decoree = (ICollidable)decoree;
        }

        public int CollisionCategory { get => this.decoree.CollisionCategory; set => this.decoree.CollisionCategory = value; } //ICollidable
    }
}
