using PhysicEngine.MathHelper;

namespace PhysicEngine.RigidBody
{
    public interface IPublicRigidBody
    {
        Vec2D Center { get; }
        float Angle { get; }
        Vec2D Velocity { get; }
        float AngularVelocity { get; }
    }

    public interface IPublicRigidRectangle : IPublicRigidBody
    {
        Vec2D[] Vertex { get; }
    }

    public interface IPublicRigidCircle : IPublicRigidBody
    {        
        float Radius { get; }        
    }
}
