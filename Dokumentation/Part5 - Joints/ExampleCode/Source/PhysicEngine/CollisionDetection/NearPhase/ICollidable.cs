namespace PhysicEngine.CollisionDetection.NearPhase
{
    internal interface ICollidable
    {
        CollisionInfo[] CollideWith(ICollidable collidable);
        List<ICollidable> CollideExcludeList { get; } //Mit diesen Objekten soll dieses Objekt nicht kollidieren (Wird für IJoint.CollideConnected benötigt)
    }
}
