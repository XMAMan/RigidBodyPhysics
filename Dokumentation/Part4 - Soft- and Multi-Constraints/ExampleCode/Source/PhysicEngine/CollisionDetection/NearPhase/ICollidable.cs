namespace PhysicEngine.CollisionDetection.NearPhase
{
    public interface ICollidable
    {
        CollisionInfo[] CollideWith(ICollidable collidable);
    }
}
