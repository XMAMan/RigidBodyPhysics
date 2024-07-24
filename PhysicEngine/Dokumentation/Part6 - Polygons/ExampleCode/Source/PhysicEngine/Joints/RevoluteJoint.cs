using PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints;
using PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints.BasisConstraints;
using PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints.Revolute;
using PhysicEngine.ExportData.Joints;
using PhysicEngine.MathHelper;
using PhysicEngine.MaxForceTracking;
using PhysicEngine.RigidBody;

namespace PhysicEngine.Joints
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
        public IPublicJoint.AngularMotor Motor { get; set; }
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
        public bool BreakWhenMaxForceIsReached { get; }
        public float MaxForceToBreak { get; }
        public float CurrentForce { get => AccumulatedPointToPointImpulse.Length(); } //Diese Kraft wurde im letzen TimeStep auf das Gelenk angwendet (Entspricht dem PointToPoint-AccumuletedImpulse oder dem DistanceImpluse)
        #endregion

        public RevoluteJoint(RevoluteJointExportData data, List<IRigidBody> bodies)
        {
            this.Body1 = this.B1 = bodies[data.BodyIndex1];
            this.Body2 = this.B2 = bodies[data.BodyIndex2];
            this.r1 = data.R1;
            this.r2 = data.R2;
            this.CollideConnected = data.CollideConnected;
            this.LimitIsEnabled = data.LimitIsEnabled;
            this.LowerAngle = data.LowerAngle;
            this.UpperAngle = data.UpperAngle;
            this.Motor = data.Motor;
            this.MotorSpeed = data.MotorSpeed;
            this.MaxMotorTorque = data.MaxMotorTorque;
            this.Soft = new SoftConstraintData(data.SoftData, B1, B2);
            this.BreakWhenMaxForceIsReached = data.BreakWhenMaxForceIsReached;
            this.MaxForceToBreak = data.MaxForceToBreak;

            UpdateAnchorPoints(); //Aktualisiere Anchor1/Anchor2

            Vec2D r1 = (this.Body1.Center - this.Anchor1).Normalize();
            Vec2D r2 = (this.Body2.Center - this.Anchor2).Normalize();
            float angle = Vec2D.Angle360(r1, r2); //Winkel von r2 im Bezug zu r1


            this.AngularDifferenceOnStart = B2.Angle - B1.Angle;
            this.DiffToMinOnStart = (float)((angle - this.LowerAngle) * Math.PI / 180); //LowerAngle=Winkel im Bezug zu r1
            
            if (this.UpperAngle > angle && this.LowerAngle > this.UpperAngle)
                this.DiffToMinOnStart += (float)Math.PI * 2; //Testcase: RevoluteJoint/SnapArm. Verhindere dasss DiffToMinOnStart als Negativ interpretiert wird obwohl R2 im Min-Max-Bereich liegt

            float min = this.LowerAngle;
            float max = this.UpperAngle;
            if (this.LimitIsEnabled == false)
            {
                min = 0;
                max = 360;
            }
            this.MinMaxDifference = max - min;
            if (this.MinMaxDifference < 0)
            {
                this.MinMaxDifference += 360; //Sorge dafür, dass min<max gilt
            }

            this.MinMaxDifference = (float)(MinMaxDifference * Math.PI / 180);

            UpdateAnchorPoints(); //Aktualisiere CurrentPosition

            this.MotorPosition = Math.Min(1, Math.Max(0, this.CurrentPosition)); //Soll-Startwert = Istwert zum Start
        }

        public void UpdateAnchorPoints()
        {
            this.Anchor1 = MathHelp.GetWorldPointFromLocalDirection(this.B1, this.r1);
            this.Anchor2 = MathHelp.GetWorldPointFromLocalDirection(this.B2, this.r2);

            float a = B2.Angle - B1.Angle - this.AngularDifferenceOnStart + this.DiffToMinOnStart;
            this.CurrentPosition = a / this.MinMaxDifference; //Wenn r2 im Min-Max-Bereich liegt, dann steht hier 0..1
        }

        public IExportJoint GetExportData(List<IRigidBody> bodies)
        {
            return new RevoluteJointExportData()
            {
                BodyIndex1 = bodies.IndexOf(B1),
                BodyIndex2 = bodies.IndexOf(B2),
                R1 = this.r1,
                R2 = this.r2,
                CollideConnected = this.CollideConnected,
                LimitIsEnabled = this.LimitIsEnabled,
                LowerAngle = this.LowerAngle,
                UpperAngle = this.UpperAngle,
                Motor = this.Motor,
                MotorSpeed = this.MotorSpeed,
                MaxMotorTorque = this.MaxMotorTorque,
                SoftData = this.Soft.GetExportData(),
                BreakWhenMaxForceIsReached = this.BreakWhenMaxForceIsReached,
                MaxForceToBreak = this.MaxForceToBreak,
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

            if (this.Motor == IPublicJoint.AngularMotor.Disabled)
                this.AccumulatedAngularMotorImpulse = 0;
            else
                list.Add(new AngularMotor(data, this));
            return list;
        }
    }
}
