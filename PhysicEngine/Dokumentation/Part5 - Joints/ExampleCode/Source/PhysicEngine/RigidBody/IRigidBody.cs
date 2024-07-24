using PhysicEngine.CollisionDetection.BroadPhase;
using PhysicEngine.CollisionDetection.NearPhase;
using PhysicEngine.ExportData.RigidBody;
using PhysicEngine.MathHelper;

namespace PhysicEngine.RigidBody
{
    internal interface IRigidBody : IBoundingCircle, ICollidable, IForceable, IMoveable, IExportableBody, IClickable, IPublicRigidBody
    {
        //State for a single timestep
        Vec2D Center { get; } //Position of the Center of gravity
        float Angle { get; } //Oriantation around the Z-Aches with rotationpoint=Center [0..2PI]
        Vec2D Velocity { get; set; } //Velocity from the Center-Point
        float AngularVelocity { get; set; }


        //Timeindependend properties
        float InverseMass { get; } //1 / Mass
        float InverseInertia { get; }
        float Restituion { get; } //Bouncing-Factor. How much energy is lost in Collision-Normal-Direction; 0=Mehlsack; 1=Springball
        float Friction { get; }   //Friction in Collision-Tangent-Direction; 0=Eiswürfel; 1=Klebstoff
    }

    //Applyed external forces
    internal interface IForceable
    {
        Vec2D Force { get; set; }//External Forces. Must be set bevore every TimeStep.Is set to 0 after TimeStep
        float Torque { get; set; }
    }

    internal interface IMoveable
    {
        void MoveCenter(Vec2D v);
        void Rotate(float angle);
        Matrix2x2 RotateToWorld { get; } //Rotiert ein lokalen Richtungsvektor in Weltkoordinaten
    }

    internal interface IExportableBody
    {
        IExportRigidBody GetExportData();
    }

    internal interface IClickable
    {
        bool IsPointInside(Vec2D position);
    }
}
