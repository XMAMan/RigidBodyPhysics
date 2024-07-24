using RigidBodyPhysics.CollisionDetection;
using RigidBodyPhysics.CollisionResolution;
using RigidBodyPhysics.ExportData;
using RigidBodyPhysics.ExportData.Joints;
using RigidBodyPhysics.ExportData.RigidBody;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.MaxForceTracking;
using RigidBodyPhysics.MouseBodyClick;
using RigidBodyPhysics.RuntimeObjects.AxialFriction;
using RigidBodyPhysics.RuntimeObjects.Joints;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using RigidBodyPhysics.RuntimeObjects.RotaryMotor;
using RigidBodyPhysics.RuntimeObjects.Thruster;

namespace RigidBodyPhysics
{
    internal class PhysicSceneConstructorData
    {
        public IRigidBody[] Bodies { get; set; }
        public IJoint[] Joints { get; set; }
        public IThruster[] Thrusters { get; set; }
        public IRotaryMotor[] Motors { get; set; }
        public IAxialFriction[] AxialFrictions { get; set; }
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
        private List<IAxialFriction> axialFrictions = new List<IAxialFriction>();
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

        public delegate void BodyWasDeleted(PhysicScene sender, IPublicRigidBody body);
        public delegate void JointWasDeleted(PhysicScene sender, IPublicJoint joint);
        public delegate void ThrusterWasDeleted(PhysicScene sender, IPublicThruster thruster);
        public delegate void RotaryMotorWasDeleted(PhysicScene sender, IPublicRotaryMotor motor);
        public delegate void AxialFrictionWasDeleted(PhysicScene sender, IPublicAxialFriction axialFriction);
        public delegate void CollisonOccuredHandler(PhysicScene sender, PublicRigidBodyCollision[] collisions);

        public BodyWasDeleted BodyWasDeletedHandler;
        public JointWasDeleted JointWasDeletedHandler;
        public ThrusterWasDeleted ThrusterWasDeletedHandler;
        public RotaryMotorWasDeleted RotaryMotorWasDeleteddHandler;
        public AxialFrictionWasDeleted AxialFrictionWasDeletedHandler;

        public CollisonOccuredHandler CollisonOccured;

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

        public IPublicAxialFriction[] GetAllAxialFrictions()
        {
            return this.axialFrictions.ToArray();
        }

        public PhysicScenePublicData GetAllPublicData()
        {
            return new PhysicScenePublicData(this.GetAllBodys(), this.GetAllJoints(), this.GetAllThrusters(), this.GetAllRotaryMotors(), this.GetAllAxialFrictions());
        }

        public PhysicScenePublicData AddPhysicScene(PhysicSceneExportData data)
        {
            if (data.CollisionMatrix == null) data.CollisionMatrix = this.collisionManager.CollisionMatrix;
            var scene = new PhysicScene(data);
            foreach (var body in scene.bodies)
            {
                AddRigidBody(body);
            }
            foreach (var joint in scene.joints)
            {
                AddJoint(joint);
            }
            this.thrusters.AddRange(scene.thrusters);
            this.rotaryMotors.AddRange(scene.rotaryMotors);
            this.axialFrictions.AddRange(scene.axialFrictions);
            SetVelocityFromFixBodiesToZero();

            return new PhysicScenePublicData(scene.GetAllBodys(), scene.GetAllJoints(), scene.GetAllThrusters(), scene.GetAllRotaryMotors(), scene.GetAllAxialFrictions());
        }

        public IPublicRigidBody AddRigidBody(IExportRigidBody exportBody)
        {
            var body = ExportHelper.BodyFromExportData(exportBody);
            AddRigidBody(body);
            return body;
        }

        public IPublicJoint AddJoint(IExportJoint exportJoint)
        {
            var joint = ExportHelper.JointFromExportData(exportJoint, this.bodies);
            this.joints.Add(joint);
            return joint;
        }

        //Wird benötigt, wenn man die R1/R2-Property von ein ExportJoint bei AddJoint festlegen will
        public static Vec2D GetLocalDirectionFromWorldPoint(IPublicRigidBody body, Vec2D worldPosition)
        {
            return MathHelp.GetLocalDirectionFromWorldPoint((IRigidBody)body, worldPosition);
        }

        internal void AddRigidBody(IRigidBody body)
        {
            SetVelocityToZeroIfBodyIsFix(body);
            this.bodies.Add(body);
            this.collisionManager.AddRigidBody(body);
            this.maxForceTracker.TryToAddBody(body);
        }

        internal void AddJoint(IJoint joint)
        {
            this.joints.Add(joint);
            this.maxForceTracker.TryToAddJoint(joint);
        }

        public void RemoveRigidBody(IPublicRigidBody body)
        {
            RemoveRigidBody((IRigidBody)body);
        }

        public void RemoveJoint(IPublicJoint joint)
        {
            RemoveJoint((IJoint)joint);
        }

        public void RemoveThruster(IPublicThruster thruster)
        {
            RemoveThruster((IThruster)thruster);
        }

        public void RemoveRotaryMotor(IPublicRotaryMotor motor)
        {
            RemoveRotaryMotor((IRotaryMotor)motor);
        }

        internal void RemoveRigidBody(IRigidBody body)
        {
            if (this.bodies.Contains(body) == false) return;

            this.bodies.Remove(body);
            this.collisionManager.RemoveRigidBody(body);

            var jointsToRemove = this.joints.Where(x => x.B1 == body || x.B2 == body).ToList();
            foreach (var item in jointsToRemove)
            {
                RemoveJoint(item);
            }

            var thrustersToRemove = this.thrusters.Where(x => x.B1 == body).ToList();
            foreach (var item in thrustersToRemove)
            {
                RemoveThruster(item);
            }

            var motorsToRemove = this.rotaryMotors.Where(x => x.B1 == body).ToList();
            foreach (var item in motorsToRemove)
            {
                RemoveRotaryMotor(item);
            }

            var axialFrictionsToRemove = this.axialFrictions.Where(x => x.B1 == body).ToList();
            foreach (var item in axialFrictionsToRemove)
            {
                RemoveAxialFriction(item);
            }

            if (body is IBreakableBody)
            {
                this.maxForceTracker.RemoveBody((IBreakableBody)body);
            }

            this.BodyWasDeletedHandler?.Invoke(this, body);
        }

        internal void RemoveJoint(IJoint joint)
        {
            this.joints.Remove(joint);

            //Korrigiere die ShouldCollide-Property bei den beiden Körpern und die ShouldCollide-Liste im CollisionManager
            joint.B1.CollideExcludeList.Remove(joint.B2);
            joint.B2.CollideExcludeList.Remove(joint.B1);

            this.collisionManager.UpdateAfterJointWasRemoved();

            if (joint is IBreakableJoint)
            {
                this.maxForceTracker.RemoveJoint((IBreakableJoint)joint);

                ((IBreakableJoint)joint).IsBroken = true;
            }

            

            this.JointWasDeletedHandler?.Invoke(this, joint);
        }

        internal void RemoveThruster(IThruster thruster)
        {
            this.thrusters.Remove(thruster);
            this.ThrusterWasDeletedHandler?.Invoke(this, thruster);
        }

        internal void RemoveRotaryMotor(IRotaryMotor motor)
        {
            this.rotaryMotors.Remove(motor);
            this.RotaryMotorWasDeleteddHandler?.Invoke(this, motor);
        }

        internal void RemoveAxialFriction(IAxialFriction axialFriction)
        {
            this.axialFrictions.Remove(axialFriction);
            this.AxialFrictionWasDeletedHandler?.Invoke(this, axialFriction);
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
            if (data.AxialFrictions != null && data.AxialFrictions.Length != this.axialFrictions.Count) throw new ArgumentException("AxialFriction-Count must be equal");

            for (int i = 0; i < this.bodies.Count; i++)
            {
                this.bodies[i].MoveCenter(data.Bodies[i].Center - this.bodies[i].Center);
                this.bodies[i].Velocity = data.Bodies[i].Velocity;
                this.bodies[i].Rotate(data.Bodies[i].AngleInDegree / 180 * (float)Math.PI - this.bodies[i].Angle);
                this.bodies[i].AngularVelocity = data.Bodies[i].AngularVelocity;
            }

            for (int i = 0; i < this.thrusters.Count; i++)
            {
                var publicThruster = (IPublicThruster)this.thrusters[i];
                publicThruster.IsEnabled = data.Thrusters[i].IsEnabled;
                publicThruster.ForceLength = data.Thrusters[i].ForceLength;
            }

            for (int i = 0; i < this.rotaryMotors.Count; i++)
            {
                var publicMotor = (IPublicRotaryMotor)this.rotaryMotors[i];
                publicMotor.IsEnabled = data.Motors[i].IsEnabled;
                publicMotor.BrakeIsEnabled = data.Motors[i].BrakeIsEnabled;
            }

            for (int i=0;i<this.axialFrictions.Count;i++)
            {
                var publicAxialFriction = (IPublicAxialFriction)this.axialFrictions[i];
                publicAxialFriction.Friction = data.AxialFrictions[i].Friction;
            }

            SetVelocityFromFixBodiesToZero();

        }

        //Hier werden die ganzen IPublic-Objekte neu angelegt -> Wird im PhysicSceneSimulator benutzt, damit man ein
        //neues Level einladen kann ohne dass das HasGravity-Flag und andere Propertys ihren Wert verlieren.
        public void Reload(PhysicSceneExportData data) 
        {
            Reload(ExportHelper.FromExportData(data));
        }
        private void Reload(PhysicSceneConstructorData data)
        {
            this.bodies = data.Bodies.ToList();
            this.joints = data.Joints != null ? data.Joints.ToList() : new List<IJoint>();
            this.thrusters = data.Thrusters != null ? data.Thrusters.ToList() : new List<IThruster>();
            this.rotaryMotors = data.Motors != null ? data.Motors.ToList() : new List<IRotaryMotor>();
            this.axialFrictions = data.AxialFrictions != null ? data.AxialFrictions.ToList() : new List<IAxialFriction>();

            if (data.CollisionMatrix == null)
                data.CollisionMatrix = new bool[1, 1] { { true } };

            this.collisionManager = new CollisionManager(this.bodies, data.CollisionMatrix);

            this.maxForceTracker = new MaxForceTracker(
                this.bodies.Where(x => x is IBreakableBody).Cast<IBreakableBody>().ToList(),
                this.joints.Where(x => x is IBreakableJoint).Cast<IBreakableJoint>().ToList(),
                RemoveRigidBody,
                RemoveJoint
                );

            SetVelocityFromFixBodiesToZero();
        }

        //Wenn von außen Bodies reingegeben werden, die einerseits nicht bewegbar sein sollen aber die anderseits eine Start-Geschwindigkeit
        //haben, dann muss die Geschwindigkeit auf 0 gesetzt werden, damit sie wirklich fix sind
        private void SetVelocityFromFixBodiesToZero()
        {
            foreach (var body in this.bodies)
            {
                SetVelocityToZeroIfBodyIsFix(body);
            }
        }

        private void SetVelocityToZeroIfBodyIsFix(IRigidBody body)
        {
            if (body.IsNotMoveable)
            {
                body.Velocity = new Vec2D(0, 0);
                body.AngularVelocity = 0;
            }
        }

        //Ermittelt von allen Körper die aktuellen Kollisionspunkte (Wird für die Testausgabe benötigt)
        public IRigidBodyCollision[] GetCollisions()
        {
            return this.collisionManager.GetAllCollisions();
        }

        public IRigidBodyCollision[] GetCollisionPointsWithScene(IExportRigidBody exportBody)
        {
            var body = ExportHelper.BodyFromExportData(exportBody);
            return this.collisionManager.GetCollisionsWithExternObject(body);
        }

        public void TimeStep(float dt)
        {
            //1. Weise der externen Kraft einen Wert zu (Der Nutzer darf vor jeden TimeStep-Aufruf auch selber Kraftwerte setzen)
            if (this.HasGravity)
                AddGravityForceForAllBodies();

            //2. Ermittle alle Kollisionspunkte
            var collisionsFromThisTimeStep = this.collisionManager.GetAllCollisions();
            if (collisionsFromThisTimeStep.Any())
                this.CollisonOccured?.Invoke(this, collisionsFromThisTimeStep.Select(x => new PublicRigidBodyCollision(x)).ToArray());

            //3. Constraint-Kraft aufgrund der aktuellen Geschwindigkeit berechnen und zusammen mit der externen Kraft anwenden
            this.impulseResolver.Resolve(new SolverInputData(this.bodies, this.joints, this.thrusters, this.rotaryMotors, this.axialFrictions, collisionsFromThisTimeStep, this.mouseData, dt, this.settings));

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

            foreach (var axialFriction in this.axialFrictions)
            {
                axialFriction.UpdateAnchorPoints();
            }
        }

        //Rufe diese Funktion am Anfang nach dem Laden der Scene auf um somit ruhig liegende Resting-Kontaktpunkte zu erhalten
        //Objekte die sich stark überlappen werden so weit auseinander geschoben, dass sie nur noch um AllowedPenetration sich überlappen
        //Ist das Kollisionsabstand kleiner als AllowedPenetration dann wird kein CollisionImpulse ausgelößt
        public void PushBodysApart()
        {
            PositionalCorrection.CreateCalmRestingContacts(this.collisionManager, this.AllowedPenetration);
        }

        //Gibt von allen Objekten aus der Scene die ExportDaten zurück
        public PhysicSceneExportData GetExportData()
        {
            return ExportHelper.ToExportData(new PhysicSceneConstructorData()
            {
                Bodies = this.bodies.ToArray(),
                Joints = this.joints.ToArray(),
                Thrusters = this.thrusters.ToArray(),
                Motors = this.rotaryMotors.ToArray(),
                AxialFrictions = this.axialFrictions.ToArray(),
                CollisionMatrix = this.collisionManager.CollisionMatrix
            });
        }

        //Gibt von einer Teilmenge der Scene die ExportDaten zurück
        public PhysicSceneExportData GetExportData(PhysicScenePublicData physicObjects)
        {
            var bodieSubList = physicObjects.Bodies.Cast<IRigidBody>().ToList();

            return new PhysicSceneExportData()
            {
                Bodies = physicObjects.Bodies.Select(x => x.GetExportData()).ToArray(),
                Joints = physicObjects.Joints.Cast<IJoint>().Select(x => x.GetExportData(bodieSubList)).ToArray(),
                Thrusters = physicObjects.Thrusters.Cast<IThruster>().Select(x => x.GetExportData(bodieSubList)).ToArray(),
                Motors =  physicObjects.Motors.Cast<IRotaryMotor>().Select(x => x.GetExportData(bodieSubList)).ToArray(),
                AxialFrictions = physicObjects.AxialFrictions.Cast<IAxialFriction>().Select(x => x.GetExportData(bodieSubList)).ToArray(),
                CollisionMatrix = this.collisionManager.CollisionMatrix
            };
        }

        #region IClickableBodyList
        //Körper per Maus anclicken
        public MouseClickData TryToGetBodyWithMouseClick(Vec2D mousePosition)
        {
            foreach (var body in this.bodies)
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
