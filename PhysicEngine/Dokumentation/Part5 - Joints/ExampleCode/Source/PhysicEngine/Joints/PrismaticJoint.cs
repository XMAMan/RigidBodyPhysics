using PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints;
using PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints.BasisConstraints;
using PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints.Prismatic;
using PhysicEngine.ExportData.Joints;
using PhysicEngine.MathHelper;
using PhysicEngine.RigidBody;

namespace PhysicEngine.Joints
{
    internal class PrismaticJoint : IJoint, IPublicPrismaticJoint, IPointToLineJoint, IMinMaxTranslationJoint, ITranslationMotorJoint, IFixAngularJoint
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
        public float MinTranslation { get; }
        public float MaxTranslation { get; }
        public IPublicJoint.TranslationMotor Motor { get; set; }
        public float MotorSpeed { get; set; }
        public float MotorPosition { get; set; }
        public float MaxMotorForce { get; set; }

        public float MotorPixelPosition { get; private set; }
        public float CurrentPosition { get; private set; } //0..1

        public SoftConstraintData Soft { get; } //Vom Nutzer vorgegebene Softness-Parameter

        public float AccumulatedPointToLineImpulse { get; set; } = 0;

        public float AccumulatedAngularImpulse { get; set; } = 0; //AngularPrismaticConstraint und PointToLineAndAngularPrismaticConstraint
        public float AccumulatedMinMaxImpulse { get; set; } = 0;
        public float AccumulatedTranslationMotorImpulse { get; set; } = 0;

        public float DistanceOnStart { get; } //Abstand von Ankerpunkt2 projetziert auf r1 zu Center1
        public Vec2D B1ToA2 { get; private set; } //d=Anchor2 - B1.Center
        public Vec2D R1Dir { get; private set; } //(Anchor1 - B1.Center).Normalize();
        public float AngularDifferenceOnStart { get; }
        private float minMaxRange = 1;

        public PrismaticJoint(PrismaticJointExportData data, List<IRigidBody> bodies)
        {
            this.Body1 = this.B1 = bodies[data.BodyIndex1];
            this.Body2 = this.B2 = bodies[data.BodyIndex2];
            this.r1 = data.R1;
            this.r2 = data.R2;
            this.CollideConnected = data.CollideConnected;
            this.LimitIsEnabled = data.LimitIsEnabled;
            this.MinTranslation = data.MinTranslation;
            this.MaxTranslation = data.MaxTranslation;
            this.Motor = data.Motor;
            this.MotorSpeed = data.MotorSpeed;
            this.MotorPosition = data.MotorPosition;
            this.MaxMotorForce = data.MaxMotorForce;
            this.Soft = new SoftConstraintData(data.SoftData, B1, B2);

            Vec2D d = MathHelp.GetWorldPointFromLocalDirection(this.B2, this.r2) - this.B1.Center;
            this.DistanceOnStart = MathHelp.GetWorldDirectionFromLocalDirection(this.B1, this.r1).Normalize() * d;

            this.AngularDifferenceOnStart = this.B2.Angle - this.B1.Angle;

            if (this.LimitIsEnabled) this.minMaxRange = MaxTranslation - MinTranslation;

            UpdateAnchorPoints();

            this.MotorPosition = Math.Min(1, Math.Max(0, this.CurrentPosition)); //Soll-Startwert = Istwert zum Start
        }

        public void UpdateAnchorPoints()
        {
            this.Anchor1 = MathHelp.GetWorldPointFromLocalDirection(this.B1, this.r1);
            this.Anchor2 = MathHelp.GetWorldPointFromLocalDirection(this.B2, this.r2);

            this.B1ToA2 = this.Anchor2 - this.B1.Center;
            this.R1Dir = (this.Anchor1 - this.B1.Center).Normalize();
            this.CurrentPosition = ((R1Dir * B1ToA2) / this.DistanceOnStart - this.MinTranslation) / minMaxRange;

            this.MotorPixelPosition = (this.MotorPosition * minMaxRange + this.MinTranslation) * this.DistanceOnStart;
        }

        public IExportJoint GetExportData(List<IRigidBody> bodies)
        {
            Vec2D r1Dir = (this.Anchor1 - this.B1.Center).Normalize();
            Vec2D d = this.Anchor2 - this.B1.Center;
            float newDistanceOnStart = r1Dir * d;
            float minMaxChange = this.DistanceOnStart / newDistanceOnStart;


            return new PrismaticJointExportData()
            {
                BodyIndex1 = bodies.IndexOf(B1),
                BodyIndex2 = bodies.IndexOf(B2),
                R1 = this.r1,
                R2 = this.r2,
                CollideConnected = this.CollideConnected,
                LimitIsEnabled = this.LimitIsEnabled,
                MinTranslation = this.MinTranslation * minMaxChange,
                MaxTranslation = this.MaxTranslation * minMaxChange,
                Motor = this.Motor,
                MotorSpeed = this.MotorSpeed,
                MotorPosition = this.CurrentPosition,
                MaxMotorForce = this.MaxMotorForce,
                SoftData = this.Soft.GetExportData()
            };
        }

        public List<IConstraint> BuildConstraints(ConstraintConstructorData data)
        {
            List<IConstraint> list = new List<IConstraint>();
            //Möglichkeit 1: PointToLine und Angular-Constraint als getrennte Klassen
            //list.Add(new PointToLine(data, this));
            //list.Add(new FixAngular(data, this, false));


            //Möglichkeit 2: PointToLine kommt in die erste J-Zeile und Angular in die zweite J-Zeile. Über
            //die inverse K-Matrix bekomme ich zwei Impulswerte: Linear-Impuls in t1-Richtung und AngularImpuls
            list.Add(new PointToLineAndFixAngular(data, this));

            if (this.LimitIsEnabled)
                list.Add(new MinMaxTranslation(data, this));
            if (this.Motor == IPublicPrismaticJoint.TranslationMotor.Disabled)
                this.AccumulatedTranslationMotorImpulse = 0;
            else
                list.Add(new TranslationMotor(data, this));
            return list;
        }
    }
}
