using PhysicGlobal;
using RigidBodyPhysics.CollisionResolution.SequentiellImpulse.Constraints;
using RigidBodyPhysics.CollisionResolution.SequentiellImpulse.Constraints.BasisConstraints;
using RigidBodyPhysics.CollisionResolution.SequentiellImpulse.Constraints.Revolute;
using RigidBodyPhysics.ExportData.Joints;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.MaxForceTracking;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RigidBodyPhysics.RuntimeObjects.Joints
{
    internal class RevoluteJoint : IJoint, IPublicRevoluteJoint, IPointToPointJoint, IMinMaxAngularJoint, IAngularMotorJoint, IBreakableJoint
    {
        private Vec2D r1 { get; init; } //lokaler Richtungsvektor von B1.Center nach Anchor1
        private Vec2D r2 { get; init; }

        public IPublicRigidBody Body1 { get; init; }
        public IPublicRigidBody Body2 { get; init; }
        public IRigidBody B1 { get; init; }
        public IRigidBody B2 { get; init; }
        public Vec2D Anchor1 { get; private set; } //Angabe in Weltkoordinaten
        public Vec2D Anchor2 { get; private set; }
        public bool CollideConnected { get; init; }

        public bool LimitIsEnabled { get; init; }
        public float LowerAngle { get; init; } //0..360
        public float UpperAngle { get; init; } //0..360

        private IPublicJoint.AngularMotor motor = IPublicJoint.AngularMotor.Disabled;
        public IPublicJoint.AngularMotor Motor
        {
            get => motor;
            set
            {
                if (motor != value)
                {
                    motor = value;
                    MotorChanged?.Invoke(value);
                }
            }
        }
        public event Action<IPublicJoint.AngularMotor> MotorChanged;
        public float MotorSpeed { get; set; }
        public float MotorPosition { get; set; } //0..1 (Gelenksollwert)
        public float MaxMotorTorque { get; set; }

        public SoftConstraintData Soft { get; init; } //Vom Nutzer vorgegebene Softness-Parameter

        public float CurrentPosition { get; private set; } //0..1


        public float AngularDifferenceOnStart { get; init; }
        public float DiffToMinOnStart { get; init; }
        public float MinMaxDifference { get; init; }


        public float AccumulatedMinMaxAngularImpulse { get; set; } = 0;
        public float AccumulatedAngularMotorImpulse { get; set; } = 0;
        public Vec2D AccumulatedPointToPointImpulse { get; set; } = new Vec2D(0, 0);

        #region IBreakableJoint
        public bool IsBroken { get; set; } = false;
        public bool BreakWhenMaxForceIsReached { get; init; }
        public float MaxForceToBreak { get; init; }
        public float CurrentForce { get => AccumulatedPointToPointImpulse.Length(); } //Diese Kraft wurde im letzen TimeStep auf das Gelenk angwendet (Entspricht dem PointToPoint-AccumuletedImpulse oder dem DistanceImpluse)
        #endregion

        public RevoluteJoint(RevoluteJointExportData data, List<IRigidBody> bodies)
        {
            //Hier wird allen init-Variablen ein Wert zugewiesen
            Body1 = B1 = bodies[data.BodyIndex1];
            Body2 = B2 = bodies[data.BodyIndex2];
            r1 = data.R1;
            r2 = data.R2;
            CollideConnected = data.CollideConnected;
            LimitIsEnabled = data.LimitIsEnabled;
            LowerAngle = data.LowerAngle;
            UpperAngle = data.UpperAngle;
            Soft = new SoftConstraintData(data.SoftData, B1, B2);
            BreakWhenMaxForceIsReached = data.BreakWhenMaxForceIsReached;
            MaxForceToBreak = data.MaxForceToBreak;
            AngularDifferenceOnStart = B2.Angle - B1.Angle;

            //weise den restlichen Variablen ein Wert zu
            LoadExportData(data);   //Hier wird CurrentPosition dann gelesen

            UpdateAnchorPoints(); //Aktualisiere Anchor1/Anchor2

            MinMaxDifference = GetMinMaxDifference();
            DiffToMinOnStart = GetDiffToMinOnStart(); //Hier wird Anchor1/Anchor2 gelesen

            UpdateAnchorPoints(); //Aktualisiere CurrentPosition

            if (float.IsNaN(data.MotorPosition))
            {
                this.MotorPosition = Math.Min(1, Math.Max(0, CurrentPosition)); //Soll-Startwert = Istwert zum Start
            }
            else
            {
                this.MotorPosition = data.MotorPosition;
            }
        }

        private float GetMinMaxDifference()
        {
            float min = LowerAngle;
            float max = UpperAngle;
            if (LimitIsEnabled == false)
            {
                min = 0;
                max = 360;
            }
            float minMaxDifference = max - min;
            if (minMaxDifference < 0)
            {
                minMaxDifference += 360; //Sorge dafür, dass min<max gilt
            }

            minMaxDifference = (float)(minMaxDifference * Math.PI / 180);

            return minMaxDifference;
        }

        private float GetDiffToMinOnStart()
        {
            if (LimitIsEnabled == false)
                return 0;

            Vec2D r1 = (Body1.Center - Anchor1).Normalize();
            Vec2D r2 = (Body2.Center - Anchor2).Normalize();
            float angle = Vec2D.Angle360(r1, r2); //Winkel von r2 im Bezug zu r1

            float diffToMinOnStart = (float)((angle - LowerAngle) * Math.PI / 180); //LowerAngle=Winkel im Bezug zu r1

            if (UpperAngle > angle && LowerAngle > UpperAngle)
                diffToMinOnStart += (float)Math.PI * 2; //Testcase: RevoluteJoint/SnapArm. Verhindere dasss DiffToMinOnStart als Negativ interpretiert wird obwohl R2 im Min-Max-Bereich liegt

            return diffToMinOnStart;
        }

        public void UpdateAnchorPoints()
        {
            Anchor1 = MathHelp.GetWorldPointFromLocalDirection(B1, r1);
            Anchor2 = MathHelp.GetWorldPointFromLocalDirection(B2, r2);

            float a = B2.Angle - B1.Angle - AngularDifferenceOnStart + DiffToMinOnStart;
            CurrentPosition = a / MinMaxDifference; //Wenn r2 im Min-Max-Bereich liegt, dann steht hier 0..1
        }

        public IExportJoint GetExportData(List<IRigidBody> bodies)
        {
            return new RevoluteJointExportData()
            {
                BodyIndex1 = bodies.IndexOf(B1),
                BodyIndex2 = bodies.IndexOf(B2),
                R1 = new Vec2D(r1),
                R2 = new Vec2D(r2),
                CollideConnected = CollideConnected,
                LimitIsEnabled = LimitIsEnabled,
                LowerAngle = LowerAngle,
                UpperAngle = UpperAngle,
                Motor = Motor,
                MotorSpeed = MotorSpeed,
                MaxMotorTorque = MaxMotorTorque,
                SoftData = Soft.GetExportData(),
                BreakWhenMaxForceIsReached = BreakWhenMaxForceIsReached,
                MaxForceToBreak = MaxForceToBreak,
                MotorPosition = MotorPosition,
                IsBroken = IsBroken,
            };
        }

        public List<IConstraint> BuildConstraints(ConstraintConstructorData data)
        {
            List<IConstraint> list = new List<IConstraint>();
            list.Add(new PointToPoint(data, this));
            if (LimitIsEnabled)
            {
                var c = new MinMaxAngular(data, this);

                //Wenn currentPosition bereits im Bereich der Min-Max-Schranke liegt dann wende keinen MinMaxAngular-Impuls an
                if (c.ImpulseMass > 0)
                {
                    list.Add(c);
                }
            }
                
            if (Motor == IPublicJoint.AngularMotor.Disabled)
                AccumulatedAngularMotorImpulse = 0;
            else
                list.Add(new AngularMotor(data, this));
            return list;
        }

        public void SetAllAccumulatedImpulsesToZero()
        {
            this.AccumulatedMinMaxAngularImpulse = 0;
            this.AccumulatedAngularMotorImpulse = 0;
            this.AccumulatedPointToPointImpulse = new Vec2D(0, 0);
        }

        public void LoadExportData(IExportJoint joint)
        {
            var data = (RevoluteJointExportData)joint;
            
            this.Motor = data.Motor;
            this.MotorSpeed = data.MotorSpeed;
            this.MaxMotorTorque = data.MaxMotorTorque;
            this.IsBroken = data.IsBroken;

            if (float.IsNaN(data.MotorPosition))
            {
                this.MotorPosition = Math.Min(1, Math.Max(0, CurrentPosition)); //Soll-Startwert = Istwert zum Start
            }
            else
            {
                this.MotorPosition = data.MotorPosition;
            }
        }
    }
}
