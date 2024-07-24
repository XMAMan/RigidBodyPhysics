using GraphicMinimal;
using PhysicEngine.CollisionDetection;
using PhysicEngine.CollisionResolution;
using PhysicEngine.RigidBody;
using System;
using System.Reflection;

namespace PhysicEngine
{
    //Enthält eine Menge von RigidBody-Objekten
    public class PhysicScene
    {
        private List<IRigidBody> bodies = new List<IRigidBody>();

        public bool DoPositionalCorrection = false; //Soll nach jeden TimeStep die Kollision dadurch aufgelößt werden indem die Position laut Kollisionsnormale versetzt wird?
        public float PositionalCorrectionRate = 1; //0 = Keine Korrektur; 1 = Nach ein TimeStep ist die Kollision weg
        public int MaxIterionsForImpulseResolution = 15; //So oft wird über alle Kontaktpunkte ein Impuls angewendet mit dem Ziel, dass die Kontaktpunkte sich voneinander wegbewegen

        public delegate void CollisonOccuredHandler(object sender, RigidBodyCollision[] collisions);
        public CollisonOccuredHandler CollisonOccured;

        public PhysicScene() { }
        public PhysicScene(List<IRigidBody> bodies)
        {
            this.bodies = bodies;
        }

        public void AddRigidBody(IRigidBody body)
        {
            this.bodies.Add(body);
        }

        public void Reload(List<IRigidBody> bodies)
        {
            this.bodies = bodies;
        }

        public void TimeStep(float dt)
        {
            //1. Ermittle alle Kollisionspunkte / Update vorhandene Punkte
            //2. Alle RigidBodys erfahren externe Kräfte
            //3. Aus den aufsummierten Kräften wird ein Vorschlagswert für die neue Geschwindigkeit errechnet
            //4. Es wird geschaut ob es Kollisionen/Constraint-Verletzungen gab. Wenn ja dann wird der Geschwindigkeitsvorschlagwert angepasst
            //5. Geschwindigkeit verändert die Position
            //6. [Optional] Position so korrigieren, dass es keine Kollision mehr gibt

            var collisions = CollisionHelper.GetAllCollisions(this.bodies);
            if (collisions.Any())
                this.CollisonOccured?.Invoke(this, collisions);

            IterativeImpulse.ApplyImpulsesUntilAllCollisionsAreResolved(collisions, this.MaxIterionsForImpulseResolution);
            MoveBodys(dt);

            if (this.DoPositionalCorrection)
                PositionalCorrection.DoCorrection(collisions, this.PositionalCorrectionRate);
        }

        //Habe ich von box2d-lite aus der World.cpp Zeile 96/97 aber wird hier aktuell nicht verwendet
        private void ApplyExternalForces(float dt)
        {
            foreach (var body in this.bodies)
            {
                if (body.InverseMass == 0)
                    continue;

                Vector2D accelerationFromExternalForces = body.Force * body.InverseMass;
                Vector2D accelerationFromGravity = new Vector2D(0, -9.81f);
                body.Velocity += dt * (accelerationFromGravity + accelerationFromExternalForces);

                body.AngularVelocity += dt * body.InverseInertia * body.Torque;
            }
        }

        //Quelle 1: https://github.com/Apress/building-a-2d-physics-game-engine/blob/master/978-1-4842-2582-0_source%20code/Chapter5/Chapter5.1ACoolDemo/public_html/RigidBody/RigidShape.js Zeile 90+93
        //Quelle 2: Box2D-Lite World.cpp Zeile 130-134
        private void MoveBodys(float dt)
        {
            foreach (var body in this.bodies)
            {
                body.MoveCenter(dt * body.Velocity);
                body.Rotate(dt * body.AngularVelocity);

                body.Force = new Vector2D(0, 0);
                body.Torque = 0;
            }
        }
    }
}