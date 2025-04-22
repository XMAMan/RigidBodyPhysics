using GraphicMinimal;
using PhysicEngine.CollisionDetection;
using PhysicEngine.CollisionResolution;
using PhysicEngine.CollisionResolution.EnterTheMatrix;
using PhysicEngine.RigidBody;
using System;
using System.Reflection;

namespace PhysicEngine
{
    //Enthält eine Menge von RigidBody-Objekten
    public class PhysicScene
    {
        private List<IRigidBody> bodies = new List<IRigidBody>();

        private SolverSettings settings= new SolverSettings();
        private IImpulseResolver impulseResolver = new MatrixImpulseResolverGlobal();
        //private IImpulseResolver impulseResolver = new MatrixImpulseResolverGrouped();

        //Diese Funktionen sind übergangsweise drin bis ich automatisch Kontaktpunkte gruppieren kann
        public void UseGlobalSolver() => this.impulseResolver = new MatrixImpulseResolverGlobal();
        public void UseGroupSolver() => this.impulseResolver = new MatrixImpulseResolverGrouped();

        public bool DoPositionalCorrection { get => this.settings.DoPositionalCorrection; set => this.settings.DoPositionalCorrection = value; }
        public float PositionalCorrectionRate { get => this.settings.PositionalCorrectionRate; set => this.settings.PositionalCorrectionRate = value; }
        public float AllowedPenetration { get => this.settings.AllowedPenetration; set => this.settings.AllowedPenetration = value; }
        public int IterationCount { get => this.settings.IterationCount; set => this.settings.IterationCount = value; }
        public float Gravity { get => this.settings.Gravity; set => this.settings.Gravity = value; }

        public bool HasGravity = true;

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

        //Ermittelt von allen Körper die aktuellen Kollisionspunkte (Wird für die Testausgabe benötigt)
        public RigidBodyCollision[] GetCollisions()
        {
            return CollisionHelper.GetAllCollisions(this.bodies);
        }

        //Zum Test, um zu sehen wie schnell PGS konvergiert
        //dt wird benötigt, da die NormalConstraint damit sehen kann, ob es ein RestingContact oder CollidingContact ist
        //Als Input sollte man eine Scene nehmen wo viele Restingcontact sind
        //Die Schwerkraft bringt den Unruhe ins System welche durch Constraint-Kräfte ausgelichen werden muss
        public Matrix[] GetProjectedGaussSeidelSteps(float dt, int iterations)
        {
            var collisions = CollisionHelper.GetAllCollisions(this.bodies);
            AddGravityForceForAllBodies(); //Der Schwerkraft bringt Unruhe ins System
            var solve = new EquationOfMotionData(this.bodies, collisions, dt, this.settings); //Bestimme die resultierende Constraint-Kraft
            return solve.GetProjectedGaussSeidelSteps(iterations); //Gebe den Lambda-Vektor von jeden PGS-Schritt zurück
        }

        public void TimeStep(float dt)
        {
            //1. Weise der externen Kraft einen Wert zu (Der Nutzer darf vor jeden TimeStep-Aufruf auch selber Kraftwerte setzen)
            if (this.HasGravity)
                AddGravityForceForAllBodies();

            //2. Ermittle alle Kollisionspunkte
            var collisionsFromThisTimeStep = CollisionHelper.GetAllCollisions(this.bodies);
            if (collisionsFromThisTimeStep.Any())
                this.CollisonOccured?.Invoke(this, collisionsFromThisTimeStep);

            //3. Constraint-Kraft aufgrund der aktuellen Geschwindigkeit berechnen und zusammen mit der externen Kraft anwenden
            if (collisionsFromThisTimeStep.Any()) //Abfrage: Gibt es Constraints?
                this.impulseResolver.Resolve(this.bodies, collisionsFromThisTimeStep, dt, this.settings); //Wende Constraint-Kraft + Externe Kraft an
            else
                ApplyExternalForces(dt); //Körper ohne Beschränkung bewegen (Wende nur die externe Kraft an)

            //4. Geschwindigkeit verändert die Position
            MoveBodys(dt);
        }

        //Der Nutzer kann pro TimeStep von außen dem Body eine externe Kraft zuweisen. Die Gravity kommt dann hier noch mit als externe Kraft hinzu.
        private void AddGravityForceForAllBodies()
        {
            foreach (var body in this.bodies)
            {
                if (body.InverseMass == 0)
                    continue;

                body.Force += new Vector2D(0, this.Gravity) / body.InverseMass;
            }
        }

        //Die externe Kraft taucht sowohl in der Bewegungsgleichung auf, welche der Constraint-Solver verwendet als
        //auch hier, wenn es für ein Body keine Constraints gibt. Ich darf die Kraft nur hier oder im Solver anwenden.
        private void ApplyExternalForces(float dt)
        {
            foreach (var body in this.bodies)
            {
                if (body.InverseMass == 0)
                    continue;

                //Bei Anwendung des Matrix-Solvers wird die externe Kraft angewendet.
                //Gibt es aber keine Kontaktpunkte, dann muss ich die Schwerkraft hier selber anwenden da der Matrix-Solver
                //nur dann ein neuen V2-Wert ausrechen kann, wenn es mindestens ein Kontaktpunkt gibt. Sonst hat er keine
                //Constraints und somit auch keine J-Matrix womit er bei Schritt 3 den 'jTransposed * lambda'-Term nicht
                //ausrechen kann, welcher nötig ist, um V2 zu bestimmen.

                body.Velocity.X += body.InverseMass * body.Force.X * dt;
                body.Velocity.Y += body.InverseMass * body.Force.Y * dt;
                body.AngularVelocity += body.InverseInertia * body.Torque * dt;
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

        //Rufe diese Funktion am Anfang nach dem Laden der Scene auf um somit ruhig liegende Resting-Kontaktpunkte zu erhalten
        //Objekte die sich stark überlappen werden so weit auseinander geschoben, dass sie nur noch um AllowedPenetration sich überlappen
        //Ist das Kollisionsabstand kleiner als AllowedPenetration dann wird kein CollisionImpulse ausgelößt
        public void PushBodysApart()
        {
            PositionalCorrection.CreateCalmRestingContacts(this.bodies, this.AllowedPenetration);
        }        
    }
}