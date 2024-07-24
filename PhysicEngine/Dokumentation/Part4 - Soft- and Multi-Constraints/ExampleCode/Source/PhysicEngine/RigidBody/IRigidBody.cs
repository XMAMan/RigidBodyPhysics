using GraphicMinimal;
using PhysicEngine.CollisionDetection.BroadPhase;
using PhysicEngine.CollisionDetection.NearPhase;
using PhysicEngine.ExportData.RigidBody;

namespace PhysicEngine.RigidBody
{
    public interface IRigidBody : IBoundingCircle, ICollidable, IForceable, IMoveable, IExportableBody, IClickable
    {
        //State for a single timestep
        Vector2D Center { get; } //Position of the Center of gravity
        float Angle { get; } //Oriantation around the Z-Aches with rotationpoint=Center [0..2PI]
        Vector2D Velocity { get; set; } //Velocity from the Center-Point
        float AngularVelocity { get; set; }


        //Timeindependend properties
        float InverseMass { get; } //1 / Mass
        float InverseInertia { get; }
        float Restituion { get; } //Bouncing-Factor. How much energy is lost in Collision-Normal-Direction; 0=Mehlsack; 1=Springball
        float Friction { get; }   //Friction in Collision-Tangent-Direction; 0=Eiswürfel; 1=Klebstoff
    }

    //Applyed external forces
    public interface IForceable
    {
        Vector2D Force { get; set; }//External Forces. Must be set bevore every TimeStep.Is set to 0 after TimeStep
        float Torque { get; set; }
    }

    public interface IMoveable
    {
        void MoveCenter(Vector2D v);
        void Rotate(float angle);
    }

    public interface IExportableBody
    {
        IExportRigidBody GetExportData();
    }

    public interface IClickable
    {
        bool IsPointInside(Vector2D position);
    }
}
