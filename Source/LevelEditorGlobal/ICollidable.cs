namespace LevelEditorGlobal
{
    //Hiermit kann der Editor von Objekten die CollisionCategory festlegen
    public interface ICollidable : IMouseClickable
    {
        int CollisionCategory { get; set; } //Die CollisionMatrix legt fest, welche CollisionCategory mit welcher anderen CollisionCategory kollidiert
    }

    public interface ICollidableContainer
    {
        ICollidable[] Collidables { get; }
    }
}
