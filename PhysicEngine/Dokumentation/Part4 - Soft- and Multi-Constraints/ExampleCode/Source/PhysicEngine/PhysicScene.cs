using GraphicMinimal;
using PhysicEngine.CollisionDetection;
using PhysicEngine.CollisionResolution;
using PhysicEngine.ExportData;
using PhysicEngine.Joints;
using PhysicEngine.MathHelper;
using PhysicEngine.MouseBodyClick;
using PhysicEngine.RigidBody;

namespace PhysicEngine
{
    public class PhysicSceneConstructorData
    {
        public IRigidBody[] Bodies { get; set; }
        public IJoint[] Joints { get; set; }
    }

    //Enthält eine Menge von RigidBody-Objekten
    public class PhysicScene : IClickableBodyList
    {
        public enum SolverType { Global, Grouped }

        //Hier sind verschiedene ResolverHelper-Ausprägungen, welche in der Dokumentation als Beispiele genutzt werden
        public enum Helper 
        { 
            Original, //Nutze ResolverHelper, welcher DistanceJointConstraints erzeugt und über Formel (2.17) den Impuls berechnet
            Helper1,  //Nutze ResolverHelper1, welcher keine DistanceJointConstraint nutzt und über die externe Kraft die Feder simuliert
            Helper2,  //Nutze ResolverHelper2, welcher über Formel (2.13) den Impuls berechnet
            Helper3,  //Nutze ResolverHelper2, welcher über Formel (2.17) den Impuls berechnet
        }

        public Helper ResolverHelper { get; set; } = Helper.Original;


        private List<IRigidBody> bodies = new List<IRigidBody>();
        private List<IJoint> joints = new List<IJoint>();
        private MouseConstraintData mouseData = null; //Wenn der Nutzer ein Körper angeklickt hat, welchen er festhalten will

        private SolverSettings settings = new SolverSettings();
        private IImpulseResolver impulseResolver = SolverFactory.CreateSolver(SolverType.Global);

        //Über diese Property kann ich aus der Testanwendung den Solvertyp ändern
        private SolverType solver = SolverType.Global;
        public SolverType Solver
        {
            get
            {
                return this.solver;
            }
            set
            {
                this.solver = value;
                this.impulseResolver = SolverFactory.CreateSolver(this.solver);
            }
        }

        public bool DoPositionalCorrection { get => this.settings.DoPositionalCorrection; set => this.settings.DoPositionalCorrection = value; }
        public float PositionalCorrectionRate { get => this.settings.PositionalCorrectionRate; set => this.settings.PositionalCorrectionRate = value; }
        public float AllowedPenetration { get => this.settings.AllowedPenetration; set => this.settings.AllowedPenetration = value; }
        public int IterationCount { get => this.settings.IterationCount; set => this.settings.IterationCount = value; }
        public bool DoWarmStart { get => this.settings.DoWarmStart; set => this.settings.DoWarmStart = value; }
        public float Gravity { get => this.settings.Gravity; set => this.settings.Gravity = value; }

        public bool HasGravity = true;

        public delegate void CollisonOccuredHandler(object sender, RigidBodyCollision[] collisions);
        public CollisonOccuredHandler CollisonOccured;

        public PhysicScene() { }
        public PhysicScene(PhysicSceneConstructorData data)
        {
            Reload(data);
        }

        public void AddRigidBody(IRigidBody body)
        {
            this.bodies.Add(body);
        }
        public void RemoveRigidBody(IRigidBody body)
        {
            this.bodies.Remove(body);

            var jointsToRemove = this.joints.Where(x => x.B1 == body || x.B2 == body).ToList();
            foreach (var item in jointsToRemove)
            {
                this.joints.Remove(item);
            }
        }

        public IRigidBody GetBodyAtIndex(int index)
        {
            return this.bodies[index];
        }

        public IJoint GetJointAtIndex(int index)
        {
            return this.joints[index];
        }

        public T GetJointAtIndex<T>(int index) where T : IJoint
        {
            return (T)this.joints[index];
        }

        public void Reload(PhysicSceneConstructorData data)
        {
            this.bodies = data.Bodies.ToList();
            this.joints = data.Joints != null ? data.Joints.ToList() : new List<IJoint>();
        }

        //Ermittelt von allen Körper die aktuellen Kollisionspunkte (Wird für die Testausgabe benötigt)
        public RigidBodyCollision[] GetCollisions()
        {
            return CollisionHelper.GetAllCollisions(this.bodies);
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

            //So kann man eine unendliche Federschwingung simulieren
            //var oldV = this.bodies.Select(x => x.Velocity).ToArray();
            //var oldA = this.bodies.Select(x => x.AngularVelocity).ToArray();

            //3. Constraint-Kraft aufgrund der aktuellen Geschwindigkeit berechnen und zusammen mit der externen Kraft anwenden
            if (collisionsFromThisTimeStep.Any() || this.joints.Any() || this.mouseData != null) //Abfrage: Gibt es Constraints?
                this.impulseResolver.Resolve(new SolverInputData(this.bodies, this.joints, collisionsFromThisTimeStep, this.mouseData, dt, this.settings, this.ResolverHelper)); //Wende Constraint-Kraft + Externe Kraft an
            else
                ApplyExternalForces(dt); //Körper ohne Beschränkung bewegen (Wende nur die externe Kraft an)

            //4. Geschwindigkeit verändert die Position
            MoveBodysAndSetForceToZero(dt);

            /*
            //So kann man eine unendliche Federschwingung simulieren
            for (int i=0;i<bodies.Count;i++)
            {
                var body = bodies[i];
                body.MoveCenter(dt * oldV[i]);
                body.Rotate(dt * oldA[i]);

                //Resette die externe Kraft
                body.Force = new Vector2D(0, 0);
                body.Torque = 0;
            }

            foreach (var joint in this.joints)
            {
                joint.UpdateAnchorPoints();
            }*/
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
        private void MoveBodysAndSetForceToZero(float dt)
        {
            foreach (var body in this.bodies)
            {
                body.MoveCenter(dt * body.Velocity);
                body.Rotate(dt * body.AngularVelocity);

                //Resette die externe Kraft
                body.Force = new Vector2D(0, 0);
                body.Torque = 0;
            }

            foreach (var joint in this.joints)
            {
                joint.UpdateAnchorPoints();
            }
        }

        //Rufe diese Funktion am Anfang nach dem Laden der Scene auf um somit ruhig liegende Resting-Kontaktpunkte zu erhalten
        //Objekte die sich stark überlappen werden so weit auseinander geschoben, dass sie nur noch um AllowedPenetration sich überlappen
        //Ist das Kollisionsabstand kleiner als AllowedPenetration dann wird kein CollisionImpulse ausgelößt
        public void PushBodysApart()
        {
            PositionalCorrection.CreateCalmRestingContacts(this.bodies, this.AllowedPenetration);
        }        

        public PhysicSceneExportData GetExportData()
        {
            return ExportHelper.ToExportData(new PhysicSceneConstructorData()
            {
                Bodies = this.bodies.ToArray(),
                Joints = this.joints.ToArray(),
            });
        }

        #region IClickableBodyList
        //Körper per Maus anclicken
        public MouseClickData TryToGetBodyWithMouseClick(Vector2D mousePosition)
        {
            foreach(var body in this.bodies)
            {
                if (body.IsPointInside(mousePosition))
                {
                    return new MouseClickData()
                    {
                        RigidBody = body,
                        LocalAnchorDirection = MathHelp.GetLocalDirectionFromWorldPoint(body, mousePosition),
                        Position = mousePosition
                    };
                }
            }

            return null;
        }

        //Maus wird geklickt und hält ein Körper fest. Sie zieht an ein Ankerpunkt mit einer Kraft von maxForce=0..Float.MaxValue
        public void SetMouseConstraint(MouseClickData mouseClick, MouseConstraintUserData userData)
        {
            //Nur wenn die Masse nicht unendlich ist kann der Körper mit der Maus verschoben werden 
            if (mouseClick?.RigidBody?.InverseMass != 0)
            {
                this.mouseData = new MouseConstraintData(mouseClick, userData);
            }            
        }

        //Maus wird losgelassen
        public void ClearMouseConstraint()
        {
            this.mouseData = null;
        }

        //Maus wird bewegt
        public void UpdateMousePosition(Vector2D mousePosition)
        {
            if (this.mouseData != null)
            {
                this.mouseData.UpdateMousePosition(mousePosition);
            }
        }
        #endregion
    }
}