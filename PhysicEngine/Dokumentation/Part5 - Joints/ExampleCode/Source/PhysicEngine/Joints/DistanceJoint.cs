using PhysicEngine.CollisionResolution.SequentiellImpulse;
using PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints;
using PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints.Distance;
using PhysicEngine.ExportData.Joints;
using PhysicEngine.MathHelper;
using PhysicEngine.RigidBody;
using static PhysicEngine.Joints.IPublicJoint;

namespace PhysicEngine.Joints
{
    internal class DistanceJoint : IJoint, IPublicDistanceJoint
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

        private float lengthPosition = 1;
        public float LengthPosition 
        { 
            get => lengthPosition;
            set 
            {
                lengthPosition = value;
                this.Length = (lengthPosition * minMaxRange + this.MinLength) * this.AnchorDistanceOnStart;
            }
        }

        internal float AnchorDistanceOnStart { get; }        
        internal float Length { get; set; } //Sollwertlänge in Pixeln
        public bool LimitIsEnabled { get; }
        public float MinLength { get; } //Minimale Länge in Prozent im Bezug zur Length-Property
        public float MaxLength { get; } //Maximale Länge in Prozent im Bezug zur Length-Property
        public float CurrentPosition { get; private set; } //Istwertlänge 0..1


        internal SoftConstraintData Soft; //Vom Nutzer vorgegebene Softness-Parameter

        internal float AccumulatedImpulse { get; set; } = 0; //Für DistanceJointConstraint
        internal float AccumulatedImpulseForMinMax { get; set; } = 0; //Für MinMaxDistanceConstraint

        private float minMaxRange = 1;

        public DistanceJoint(DistanceJointExportData data, List<IRigidBody> bodies)
        {
            this.Body1 = this.B1 = bodies[data.BodyIndex1];
            this.Body2 = this.B2 = bodies[data.BodyIndex2];
            this.r1 = data.R1;
            this.r2 = data.R2;
            this.CollideConnected = data.CollideConnected;
            this.LengthPosition = data.LengthPosition;
            this.LimitIsEnabled = data.LimitIsEnabled;
            this.MinLength = data.MinLength;
            this.MaxLength = data.MaxLength;


            this.Soft = new SoftConstraintData(data.SoftData, B1, B2);

            this.AnchorDistanceOnStart = (MathHelp.GetWorldPointFromLocalDirection(this.B2, this.r2) - MathHelp.GetWorldPointFromLocalDirection(this.B1, this.r1)).Length();
            this.Length = this.AnchorDistanceOnStart * data.LengthPosition;
            if (this.LimitIsEnabled) this.minMaxRange = Math.Abs(MaxLength - MinLength);

            UpdateAnchorPoints();

            this.LengthPosition = this.CurrentPosition;
        }

        public void UpdateAnchorPoints()
        {
            this.Anchor1 = MathHelp.GetWorldPointFromLocalDirection(this.B1, this.r1);
            this.Anchor2 = MathHelp.GetWorldPointFromLocalDirection(this.B2, this.r2);

            float currentLength = (this.Anchor2 - this.Anchor1).Length();
            this.CurrentPosition = (currentLength / this.AnchorDistanceOnStart - this.MinLength) / minMaxRange;
        }

        public IExportJoint GetExportData(List<IRigidBody> bodies)
        {
            float newDistanceOnStart = (Anchor2 - Anchor1).Length();
            float minMaxChange = this.AnchorDistanceOnStart / newDistanceOnStart;

            return new DistanceJointExportData()
            {
                BodyIndex1 = bodies.IndexOf(B1),
                BodyIndex2 = bodies.IndexOf(B2),
                R1 = this.r1,
                R2 = this.r2,
                CollideConnected = this.CollideConnected,
                LengthPosition = this.CurrentPosition,
                LimitIsEnabled = this.LimitIsEnabled,
                MinLength = this.MinLength * minMaxChange,
                MaxLength = this.MaxLength * minMaxChange,
                SoftData = this.Soft.GetExportData()
            };
        }

        public List<IConstraint> BuildConstraints(ConstraintConstructorData data)
        {
            List<IConstraint> list = new List<IConstraint>();
            list.Add(new DistanceJointConstraint(data, this));
            if (this.LimitIsEnabled)
                list.Add(new MinMaxDistanceConstraint(data, this));
            return list;
        }
    }
}
