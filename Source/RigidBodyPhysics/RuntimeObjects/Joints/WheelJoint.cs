using RigidBodyPhysics.CollisionResolution.SequentiellImpulse.Constraints.BasisConstraints;
using RigidBodyPhysics.CollisionResolution.SequentiellImpulse.Constraints;
using RigidBodyPhysics.ExportData.Joints;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.MaxForceTracking;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace RigidBodyPhysics.RuntimeObjects.Joints
{
    internal class WheelJoint : IJoint, IPublicWheelJoint, IPointToLineJoint, IMinMaxTranslationJoint, ITranslationMotorJoint, IBreakableJoint
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
        public float MinTranslation { get; } = 0;
        public float MaxTranslation { get; } = 1;
        public IPublicJoint.TranslationMotor Motor { get; set; }
        public float MotorSpeed { get; set; }
        public float MotorPosition { get; set; }
        public float MaxMotorForce { get; set; }

        public float MotorPixelPosition { get; private set; }
        public float CurrentPosition { get; private set; } //0..1

        public SoftConstraintData Soft { get; } //Vom Nutzer vorgegebene Softness-Parameter

        public float AccumulatedPointToLineImpulse { get; set; } = 0;
        public float AccumulatedMinMaxImpulse { get; set; } = 0;
        public float AccumulatedTranslationMotorImpulse { get; set; } = 0; //Für MotorWheelConstraint

        #region IBreakableJoint
        public bool IsBroken { get; set; } = false;
        public bool BreakWhenMaxForceIsReached { get; }
        public float MaxForceToBreak { get; }
        public float CurrentForce { get => AccumulatedPointToLineImpulse; } //Diese Kraft wurde im letzen TimeStep auf das Gelenk angwendet (Entspricht dem PointToPoint-AccumuletedImpulse oder dem DistanceImpluse)
        #endregion

        public float R1Length { get; } //Abstand von Ankerpunkt2 projetziert auf r1 zu Center1
        public Vec2D B1ToA2 { get; private set; } //d=Anchor2 - B1.Center
        public Vec2D R1Dir { get; private set; } //(Anchor1 - B1.Center).Normalize();
        private float minMaxRange = 1;

        public WheelJoint(WheelJointExportData data, List<IRigidBody> bodies)
        {
            Body1 = B1 = bodies[data.BodyIndex1];
            Body2 = B2 = bodies[data.BodyIndex2];
            r1 = data.R1;
            r2 = data.R2;
            CollideConnected = data.CollideConnected;
            LimitIsEnabled = data.LimitIsEnabled;
            MinTranslation = data.MinTranslation;
            MaxTranslation = data.MaxTranslation;
            Motor = data.Motor;
            MotorSpeed = data.MotorSpeed;
            MaxMotorForce = data.MaxMotorForce;
            Soft = new SoftConstraintData(data.SoftData, B1, B2);
            BreakWhenMaxForceIsReached = data.BreakWhenMaxForceIsReached;
            MaxForceToBreak = data.MaxForceToBreak;

            R1Length = r1.Length();

            if (LimitIsEnabled) minMaxRange = MaxTranslation - MinTranslation;

            UpdateAnchorPoints();

            MotorPosition = Math.Min(1, Math.Max(0, CurrentPosition)); //Soll-Startwert = Istwert zum Start
            MotorPixelPosition = (MotorPosition * minMaxRange + MinTranslation) * R1Length;
        }

        public void UpdateAnchorPoints()
        {
            Anchor1 = MathHelp.GetWorldPointFromLocalDirection(B1, r1);
            Anchor2 = MathHelp.GetWorldPointFromLocalDirection(B2, r2);

            B1ToA2 = Anchor2 - B1.Center;
            R1Dir = (Anchor1 - B1.Center).Normalize();
            CurrentPosition = (R1Dir * B1ToA2 / R1Length - MinTranslation) / minMaxRange;

            MotorPixelPosition = (MotorPosition * minMaxRange + MinTranslation) * R1Length;
        }

        public IExportJoint GetExportData(List<IRigidBody> bodies)
        {
            return new WheelJointExportData()
            {
                BodyIndex1 = bodies.IndexOf(B1),
                BodyIndex2 = bodies.IndexOf(B2),
                R1 = r1,
                R2 = r2,
                CollideConnected = CollideConnected,
                LimitIsEnabled = LimitIsEnabled,
                MinTranslation = MinTranslation,
                MaxTranslation = MaxTranslation,
                Motor = Motor,
                MotorSpeed = MotorSpeed,
                MaxMotorForce = MaxMotorForce,
                SoftData = Soft.GetExportData(),
                BreakWhenMaxForceIsReached = BreakWhenMaxForceIsReached,
                MaxForceToBreak = MaxForceToBreak,
            };
        }

        public List<IConstraint> BuildConstraints(ConstraintConstructorData data)
        {
            List<IConstraint> list = new List<IConstraint>();
            list.Add(new PointToLine(data, this));

            if (LimitIsEnabled)
            {
                var c = new MinMaxTranslation(data, this);

                //Wenn currentPosition bereits im Bereich der Min-Max-Schranke liegt dann wende keinen MinMaxTranslation-Impuls an
                if (c.ImpulseMass > 0)
                {
                    list.Add(c);
                }
            }

            if (Motor == IPublicJoint.TranslationMotor.Disabled)
                AccumulatedTranslationMotorImpulse = 0;
            else
                list.Add(new TranslationMotor(data, this));
            return list;
        }
    }
}
