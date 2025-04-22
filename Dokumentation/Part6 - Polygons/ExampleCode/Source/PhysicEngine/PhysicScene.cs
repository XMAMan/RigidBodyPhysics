using PhysicEngine.CollisionDetection;
using PhysicEngine.CollisionResolution;
using PhysicEngine.ExportData;
using PhysicEngine.Joints;
using PhysicEngine.MathHelper;
using PhysicEngine.MaxForceTracking;
using PhysicEngine.MouseBodyClick;
using PhysicEngine.RigidBody;
using PhysicEngine.RotaryMotor;
using PhysicEngine.Thruster;

namespace PhysicEngine
{
    internal class PhysicSceneConstructorData
    {
        public IRigidBody[] Bodies { get; set; }
        public IJoint[] Joints { get; set; }
        public IThruster[] Thrusters { get; set; }
        public IRotaryMotor[]Motors { get; set; }
        public bool[,] CollisionMatrix { get; set; }
    }

    //Enthält eine Menge von RigidBody-Objekten
    public class PhysicScene : IClickableBodyList
    {
        public enum SolverType { Global, Grouped }


        private List<IRigidBody> bodies = new List<IRigidBody>();
        private List<IJoint> joints = new List<IJoint>();
        private List<IThruster> thrusters = new List<IThruster>();
        private List<IRotaryMotor> rotaryMotors = new List<IRotaryMotor>();
        private MouseConstraintData mouseData = null; //Wenn der Nutzer ein Körper angeklickt hat, welchen er festhalten will
        private CollisionManager collisionManager = null; //Ermittelt die Kollisionspunkte
        private MaxForceTracker maxForceTracker;

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


        public delegate void JointWasDeleted(PhysicScene sender, IPublicJoint joint);
        public delegate void BodyWasDeleted(PhysicScene sender, IPublicRigidBody body);

        public JointWasDeleted JointWasDeletedHandler;
        public BodyWasDeleted BodyWasDeletedHandler;


        internal delegate void CollisonOccuredHandler(object sender, RigidBodyCollision[] collisions);
        internal CollisonOccuredHandler CollisonOccured;

        public PhysicScene() { }
        public PhysicScene(PhysicSceneExportData data)
        {
            Reload(data);
        }
        internal PhysicScene(PhysicSceneConstructorData data)
        {
            Reload(data);
        }

        public IPublicRigidBody[] GetAllBodys()
        {
            return this.bodies.ToArray();
        }

        public IPublicJoint[] GetAllJoints()
        {
            return this.joints.ToArray();
        }

        public IPublicThruster[] GetAllThrusters()
        {
            return this.thrusters.ToArray();
        }

        public IPublicRotaryMotor[] GetAllRotaryMotors()
        {
            return this.rotaryMotors.ToArray();
        }

        public void AddPhysicScene(PhysicSceneExportData data)
        {
            var scene = new PhysicScene(data);
            this.bodies.AddRange(scene.bodies);
            this.joints.AddRange(scene.joints);
            this.thrusters.AddRange(scene.thrusters);
            this.rotaryMotors.AddRange(scene.rotaryMotors);
            SetVelocityFromFixBodiesToZero();
        }

        public void RemoveRigidBody(IPublicRigidBody body)
        {
            RemoveRigidBody((IRigidBody)body);
        }

        internal void RemoveRigidBody(IRigidBody body)
        {
            this.bodies.Remove(body);
            this.collisionManager.RemoveRigidBody(body);

            var jointsToRemove = this.joints.Where(x => x.B1 == body || x.B2 == body).ToList();
            foreach (var item in jointsToRemove)
            {
                this.joints.Remove(item);
            }

            var thrustersToRemove = this.thrusters.Where(x => x.B1 == body).ToList();
            foreach (var item in thrustersToRemove)
            {
                this.thrusters.Remove(item);
            }

            var motorsToRemove = this.rotaryMotors.Where(x => x.B1 == body).ToList();
            foreach (var item in motorsToRemove)
            {
                this.rotaryMotors.Remove(item);
            }
        }

        internal void RemoveJoint(IJoint joint)
        {
            this.joints.Remove(joint);

            //Korrigiere die ShouldCollide-Property bei den beiden Körpern und die ShouldCollide-Liste im CollisionManager
            joint.B1.CollideExcludeList.Remove(joint.B2);
            joint.B2.CollideExcludeList.Remove(joint.B1);

            this.collisionManager.UpdateAfterJointWasRemoved();
        }

        internal IRigidBody GetBodyAtIndex(int index)
        {
            return this.bodies[index];
        }

        internal IJoint GetJointAtIndex(int index)
        {
            return this.joints[index];
        }

        internal T GetJointAtIndex<T>(int index) where T : IJoint
        {
            return (T)this.joints[index];
        }

        public void ResetPosition(PhysicSceneExportData data)//Hier bleiben die IPublic-Objekte erhalten
        {
            if (data.Bodies.Length != this.bodies.Count) throw new ArgumentException("Body-Count must be equal");
            if (data.Thrusters != null && data.Thrusters.Length != this.thrusters.Count) throw new ArgumentException("Thruster-Count must be equal");
            if (data.Motors != null && data.Motors.Length != this.rotaryMotors.Count) throw new ArgumentException("Motor-Count must be equal");

            for (int i=0;i<this.bodies.Count; i++)
            {
                this.bodies[i].MoveCenter(data.Bodies[i].Center - this.bodies[i].Center);
                this.bodies[i].Velocity = data.Bodies[i].Velocity;
                this.bodies[i].Rotate(data.Bodies[i].AngleInDegree / 180 * (float)Math.PI - this.bodies[i].Angle);
                this.bodies[i].AngularVelocity = data.Bodies[i].AngularVelocity;
            }

            for (int i = 0; i < this.thrusters.Count; i++)
            {
                (this.thrusters[i] as IPublicThruster).IsEnabled = data.Thrusters[i].IsEnabled;
            }

            for (int i=0;i<this.rotaryMotors.Count; i++)
            {
                (this.rotaryMotors[i] as IPublicRotaryMotor).IsEnabled = data.Motors[i].IsEnabled;
            }

            SetVelocityFromFixBodiesToZero();

        }
        public void Reload(PhysicSceneExportData data) //Hier werden die ganzen IPublic-Objekte neu angelegt
        {
            Reload(ExportHelper.FromExportData(data));
        }
        private void Reload(PhysicSceneConstructorData data)
        {
            this.bodies = data.Bodies.ToList();            
            this.joints = data.Joints != null ? data.Joints.ToList() : new List<IJoint>();
            this.thrusters = data.Thrusters != null ? data.Thrusters.ToList() : new List<IThruster>();
            this.rotaryMotors = data.Motors != null ? data.Motors.ToList() : new List<IRotaryMotor>();

            if (data.CollisionMatrix == null)
                data.CollisionMatrix = new bool[1, 1] { { true } };

            this.collisionManager = new CollisionManager(this.bodies, data.CollisionMatrix);

            this.maxForceTracker = new MaxForceTracker(
                this.bodies.Where(x=> x is IBreakableBody).Cast<IBreakableBody>().ToList(),
                this.joints.Where(x => x is IBreakableJoint).Cast<IBreakableJoint>().ToList(),
                (body)=>
                {
                    RemoveRigidBody(body);
                    this.BodyWasDeletedHandler?.Invoke(this, body);
                },
                (joint)=>
                {
                    RemoveJoint(joint);                    
                    this.JointWasDeletedHandler?.Invoke(this, joint);
                }
                );

            SetVelocityFromFixBodiesToZero();
        }

        //Wenn von AUßen Bodies reingegeben werden, die einerseits nicht bewegbar sein sollen aber die anderseits eine Start-Geschwindigkeit
        //haben, dann muss die Geschwindigkeit auf 0 gesetzt werden, damit sie wirklich fix sind
        private void SetVelocityFromFixBodiesToZero()
        {
            foreach (var body in this.bodies)
            {
                if (body.IsNotMoveable)
                {
                    body.Velocity = new Vec2D(0, 0);
                    body.AngularVelocity = 0;
                }
                    
            }
        }

        //Ermittelt von allen Körper die aktuellen Kollisionspunkte (Wird für die Testausgabe benötigt)
        public IRigidBodyCollision[] GetCollisions()
        {
            return this.collisionManager.GetAllCollisions();
        }

        public void TimeStep(float dt)
        {
            //1. Weise der externen Kraft einen Wert zu (Der Nutzer darf vor jeden TimeStep-Aufruf auch selber Kraftwerte setzen)
            if (this.HasGravity)
                AddGravityForceForAllBodies();

            //2. Ermittle alle Kollisionspunkte
            var collisionsFromThisTimeStep = this.collisionManager.GetAllCollisions();
            if (collisionsFromThisTimeStep.Any())
                this.CollisonOccured?.Invoke(this, collisionsFromThisTimeStep);

            //3. Constraint-Kraft aufgrund der aktuellen Geschwindigkeit berechnen und zusammen mit der externen Kraft anwenden
            this.impulseResolver.Resolve(new SolverInputData(this.bodies, this.joints, this.thrusters, this.rotaryMotors, collisionsFromThisTimeStep, this.mouseData, dt, this.settings)); 

            //4. Geschwindigkeit verändert die Position
            MoveBodysAndSetForceToZero(dt);

            //5. Prüfen wo die Kräfte so groß waren, dass Stäbe oder Gelenke brechen
            this.maxForceTracker.CheckForces();
        }

        //Der Nutzer kann pro TimeStep von außen dem Body eine externe Kraft zuweisen. Die Gravity kommt dann hier noch mit als externe Kraft hinzu.
        private void AddGravityForceForAllBodies()
        {
            foreach (var body in this.bodies)
            {
                if (body.InverseMass == 0)
                    continue;

                body.Force += new Vec2D(0, this.Gravity) / body.InverseMass;
            }
        }

        private void MoveBodysAndSetForceToZero(float dt)
        {
            //Quelle 1: https://github.com/Apress/building-a-2d-physics-game-engine/blob/master/978-1-4842-2582-0_source%20code/Chapter5/Chapter5.1ACoolDemo/public_html/RigidBody/RigidShape.js Zeile 90+93
            //Quelle 2: Box2D-Lite World.cpp Zeile 130-134
            foreach (var body in this.bodies)
            {
                body.MoveCenter(dt * body.Velocity);
                body.Rotate(dt * body.AngularVelocity);

                //Resette die externe Kraft
                body.Force = new Vec2D(0, 0);
                body.Torque = 0;
            }

            foreach (var joint in this.joints)
            {
                joint.UpdateAnchorPoints();
            }

            foreach (var thruster in this.thrusters)
            {
                thruster.UpdateAnchorPoints();
            }
        }

        //Rufe diese Funktion am Anfang nach dem Laden der Scene auf um somit ruhig liegende Resting-Kontaktpunkte zu erhalten
        //Objekte die sich stark überlappen werden so weit auseinander geschoben, dass sie nur noch um AllowedPenetration sich überlappen
        //Ist das Kollisionsabstand kleiner als AllowedPenetration dann wird kein CollisionImpulse ausgelößt
        public void PushBodysApart()
        {
            PositionalCorrection.CreateCalmRestingContacts(this.collisionManager, this.AllowedPenetration);
        }        

        public PhysicSceneExportData GetExportData()
        {
            return ExportHelper.ToExportData(new PhysicSceneConstructorData()
            {
                Bodies = this.bodies.ToArray(),
                Joints = this.joints.ToArray(),
                Thrusters = this.thrusters.ToArray(),
                Motors = this.rotaryMotors.ToArray(),
                CollisionMatrix = this.collisionManager.CollisionMatrix
            });
        }

        #region IClickableBodyList
        //Körper per Maus anclicken
        public MouseClickData TryToGetBodyWithMouseClick(Vec2D mousePosition)
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
            if (mouseClick != null && (mouseClick.RigidBody as IRigidBody).InverseMass != 0)
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
        public void UpdateMousePosition(Vec2D mousePosition)
        {
            if (this.mouseData != null)
            {
                this.mouseData.UpdateMousePosition(mousePosition);
            }
        }
        #endregion
    }
}