using PhysicGlobal;
using RigidBodyPhysics.CollisionResolution.SequentiellImpulse.Constraints;
using RigidBodyPhysics.CollisionResolution.SequentiellImpulse.Constraints.BasisConstraints;
using RigidBodyPhysics.CollisionResolution.SequentiellImpulse.Constraints.Prismatic;
using RigidBodyPhysics.ExportData.Joints;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.MaxForceTracking;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RigidBodyPhysics.RuntimeObjects.Joints
{
    internal class PrismaticJoint : IJoint, IPublicPrismaticJoint, IPointToLineJoint, IMinMaxTranslationJoint, ITranslationMotorJoint, IFixAngularJoint, IBreakableJoint
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
        public float MinTranslation { get; init; }
        public float MaxTranslation { get; init; }
        public IPublicJoint.TranslationMotor Motor { get; set; }
        public float MotorSpeed { get; set; }
        public float MotorPosition { get; set; } 
        public float MaxMotorForce { get; set; }

        public float MotorPixelPosition { get; private set; }
        public float CurrentPosition { get; private set; } //0..1

        public SoftConstraintData Soft { get; init; } //Vom Nutzer vorgegebene Softness-Parameter

        public float AccumulatedPointToLineImpulse { get; set; } = 0;

        public float AccumulatedAngularImpulse { get; set; } = 0; //AngularPrismaticConstraint und PointToLineAndAngularPrismaticConstraint
        public float AccumulatedMinMaxImpulse { get; set; } = 0;
        public float AccumulatedTranslationMotorImpulse { get; set; } = 0;        

        #region IBreakableJoint
        public bool IsBroken { get; set; } = false;
        public bool BreakWhenMaxForceIsReached { get; init; }
        public float MaxForceToBreak { get; init; }
        public float CurrentForce { get => AccumulatedPointToLineImpulse; } //Diese Kraft wurde im letzen TimeStep auf das Gelenk angwendet (Entspricht dem PointToPoint-AccumuletedImpulse oder dem DistanceImpluse)
        #endregion

        public float R1Length { get; init; } //Abstand von Ankerpunkt2 projetziert auf r1 zu Center1
        public Vec2D B1ToA2 { get; private set; } //d=Anchor2 - B1.Center
        public Vec2D R1Dir { get; private set; } //(Anchor1 - B1.Center).Normalize();
        public float AngularDifferenceOnStart { get; init; }
        private float minMaxRange { get; init; } = 1;

        public PrismaticJoint(PrismaticJointExportData data, List<IRigidBody> bodies)
        {
            //Hier wird allen init-Variablen ein Wert zugewiesen
            Body1 = B1 = bodies[data.BodyIndex1];
            Body2 = B2 = bodies[data.BodyIndex2];
            r1 = data.R1;
            r2 = data.R2;
            CollideConnected = data.CollideConnected;
            LimitIsEnabled = data.LimitIsEnabled;
            MinTranslation = data.MinTranslation;
            MaxTranslation = data.MaxTranslation;
            Soft = new SoftConstraintData(data.SoftData, B1, B2);
            BreakWhenMaxForceIsReached = data.BreakWhenMaxForceIsReached;
            MaxForceToBreak = data.MaxForceToBreak;
            R1Length = r1.Length();
            AngularDifferenceOnStart = B2.Angle - B1.Angle;
            if (LimitIsEnabled) minMaxRange = MaxTranslation - MinTranslation;

            //weise den restlichen Variablen ein Wert zu
            UpdateAnchorPoints();   //Das muss zuerst kommen da hier CurrentPosition ein Wert zugewiesen wird
            LoadExportData(data);   //Hier wird CurrentPosition dann gelesen

            MotorPixelPosition = (MotorPosition * minMaxRange + MinTranslation) * R1Length; //Soll-Pixelwert = Istwert zum Start
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
            return new PrismaticJointExportData()
            {
                BodyIndex1 = bodies.IndexOf(B1),
                BodyIndex2 = bodies.IndexOf(B2),
                R1 = new Vec2D(r1),
                R2 = new Vec2D(r2),
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
                MotorPosition = MotorPosition,
                IsBroken = IsBroken,
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

        public void SetAllAccumulatedImpulsesToZero()
        {
            this.AccumulatedPointToLineImpulse = 0;
            this.AccumulatedAngularImpulse = 0;
            this.AccumulatedMinMaxImpulse = 0;
            this.AccumulatedTranslationMotorImpulse = 0;
        }

        public void LoadExportData(IExportJoint joint)
        {
            var data = (PrismaticJointExportData)joint;

            this.Motor = data.Motor;
            this.MotorSpeed = data.MotorSpeed;
            this.MaxMotorForce = data.MaxMotorForce;
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
