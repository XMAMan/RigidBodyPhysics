﻿using RigidBodyPhysics.CollisionDetection.BroadPhase;
using RigidBodyPhysics.CollisionDetection.NearPhase;
using RigidBodyPhysics.ExportData.RigidBody;
using RigidBodyPhysics.MathHelper;

namespace RigidBodyPhysics.RuntimeObjects.RigidBody
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

    //Applyed external forces -> Über dieses Interface sage ich dem Körper, welche Kräfte auf sein Zentrum im nächsten TimeStep wirken sollen
    public interface IForceable
    {
        Vec2D Force { get; set; }//External Forces. Must be set bevore every TimeStep.Is set to 0 after TimeStep
        float Torque { get; set; }
    }

    //Über dieses Interface sehe ich, welche Constraint+Gravity-Kräfte auf den Körper beim letzten TimeStep gewirkt haben
    //Für die Berechung der Zug-Druck-Kräfte auf ein Stab wird hiermit getrackt, welche Impulse auf ein Körper wirken
    internal interface IBeamForceTracker
    {
        void ResetTrackForce();
        void AddTrackForce(Vec2D forcePosition, Vec2D forceDirection); //Eine Kraft greift am Punkt forcePosition in Richtung forceDirection an
        float GetPushPullForce(); // Größer 0 bedeutet der Stab wird zusammen gedrückt; Kleiner 0 heißt, er wird in die Länge gezogen
    }

    internal interface IMoveable
    {
        void MoveCenter(Vec2D v);
        void Rotate(float angle);
        Matrix2x2 RotateToWorld { get; } //Rotiert ein lokalen Richtungsvektor in Weltkoordinaten
    }

    public interface IExportableBody
    {
        IExportRigidBody GetExportData();
    }

    internal interface IClickable
    {
        bool IsPointInside(Vec2D position);
    }
}