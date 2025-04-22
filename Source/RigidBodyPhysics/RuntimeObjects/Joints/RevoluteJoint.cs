using RigidBodyPhysics.CollisionResolution.SequentiellImpulse.Constraints.BasisConstraints;
using RigidBodyPhysics.CollisionResolution.SequentiellImpulse.Constraints.Revolute;
using RigidBodyPhysics.CollisionResolution.SequentiellImpulse.Constraints;
using RigidBodyPhysics.ExportData.Joints;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.MaxForceTracking;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace RigidBodyPhysics.RuntimeObjects.Joints
{
    internal class RevoluteJoint : IJoint, IPublicRevoluteJoint, IPointToPointJoint, IMinMaxAngularJoint, IAngularMotorJoint, IBreakableJoint
    {
        private Vec2D r1; //lokaler Richtungsvektor von B1.Center nach Anchor1
        private Vec2D r2;

        public IPublicRigidBody Body1 { get; }
        public IPublicRigidBody Body2 { get; }
        public IRigidBody B1 { get; }
        public IRigidBody B2 { get; }
        public Vec2D Anchor1 { get; private set; } //Angabe in Weltkoordinaten
        public Vec2D Anchor2 { get; private set; }
        public bool CollideConnected { get; }

        public bool LimitIsEnabled { get; set; }
        public float LowerAngle { get; set; } //0..360
        public float UpperAngle { get; set; } //0..360

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
        public float MotorPosition { get; set; } //0..1
        public float MaxMotorTorque { get; set; }

        public SoftConstraintData Soft { get; } //Vom Nutzer vorgegebene Softness-Parameter

        public float CurrentPosition { get; private set; } //0..1


        public float AngularDifferenceOnStart { get; private set; }
        public float DiffToMinOnStart { get; private set; }
        public float MinMaxDifference { get; private set; }


        public float AccumulatedMinMaxAngularImpulse { get; set; } = 0;
        public float AccumulatedAngularMotorImpulse { get; set; } = 0;
        public Vec2D AccumulatedPointToPointImpulse { get; set; } = new Vec2D(0, 0);

        #region IBreakableJoint
        public bool IsBroken { get; set; } = false;
        public bool BreakWhenMaxForceIsReached { get; }
        public float MaxForceToBreak { get; }
        public float CurrentForce { get => AccumulatedPointToPointImpulse.Length(); } //Diese Kraft wurde im letzen TimeStep auf das Gelenk angwendet (Entspricht dem PointToPoint-AccumuletedImpulse oder dem DistanceImpluse)
        #endregion

        public RevoluteJoint(RevoluteJointExportData data, List<IRigidBody> bodies)
        {
            Body1 = B1 = bodies[data.BodyIndex1];
            Body2 = B2 = bodies[data.BodyIndex2];
            this.r1 = data.R1;
            this.r2 = data.R2;
            CollideConnected = data.CollideConnected;
            LimitIsEnabled = data.LimitIsEnabled;
            LowerAngle = data.LowerAngle;
            UpperAngle = data.UpperAngle;
            Motor = data.Motor;
            MotorSpeed = data.MotorSpeed;
            MaxMotorTorque = data.MaxMotorTorque;
            Soft = new SoftConstraintData(data.SoftData, B1, B2);
            BreakWhenMaxForceIsReached = data.BreakWhenMaxForceIsReached;
            MaxForceToBreak = data.MaxForceToBreak;

            UpdateAnchorPoints(); //Aktualisiere Anchor1/Anchor2

            AngularDifferenceOnStart = B2.Angle - B1.Angle;
            MinMaxDifference = GetMinMaxDifference();
            DiffToMinOnStart = GetDiffToMinOnStart();

            UpdateAnchorPoints(); //Aktualisiere CurrentPosition

            MotorPosition = Math.Min(1, Math.Max(0, CurrentPosition)); //Soll-Startwert = Istwert zum Start
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
                R1 = r1,
                R2 = r2,
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
    }
}
