using PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints;
using PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints.Distance;
using PhysicEngine.ExportData.Joints;
using PhysicEngine.MathHelper;
using PhysicEngine.MaxForceTracking;
using PhysicEngine.RigidBody;

namespace PhysicEngine.Joints
{
    internal class DistanceJoint : IJoint, IPublicDistanceJoint, IBreakableJoint
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

        public float LengthPosition { get; set; }//Sollwertlänge in Pixeln
        public bool LimitIsEnabled { get; }
        public bool JointIsRope { get; }
        public float MinLength { get; } //Minimale Länge in Pixeln
        public float MaxLength { get; } //Maximale Länge in Pixeln
        public float CurrentPosition { get; private set; } //Istwertlänge in Pixeln

        #region IBreakableJoint
        public bool BreakWhenMaxForceIsReached { get; }
        public float MaxForceToBreak { get; }
        public float CurrentForce { get => AccumulatedImpulse; } //Diese Kraft wurde im letzen TimeStep auf das Gelenk angwendet (Entspricht dem PointToPoint-AccumuletedImpulse oder dem DistanceImpluse)
        #endregion

        internal SoftConstraintData Soft; //Vom Nutzer vorgegebene Softness-Parameter

        public float AccumulatedImpulse { get; internal set; } = 0; //Für DistanceJointConstraint
        public float AccumulatedImpulseForMinMax { get; internal set; } = 0; //Für MinMaxDistanceConstraint

        public DistanceJoint(DistanceJointExportData data, List<IRigidBody> bodies)
        {
            this.Body1 = this.B1 = bodies[data.BodyIndex1];
            this.Body2 = this.B2 = bodies[data.BodyIndex2];
            this.r1 = data.R1;
            this.r2 = data.R2;
            this.CollideConnected = data.CollideConnected;
            this.LimitIsEnabled = data.LimitIsEnabled;
            this.JointIsRope = data.JointIsRope;
            this.MinLength = data.MinLength;
            this.MaxLength = data.MaxLength;
            this.BreakWhenMaxForceIsReached = data.BreakWhenMaxForceIsReached;
            this.MaxForceToBreak = data.MaxForceToBreak;

            this.Soft = new SoftConstraintData(data.SoftData, B1, B2);

            UpdateAnchorPoints();

            this.LengthPosition = this.CurrentPosition;
        }

        public void UpdateAnchorPoints()
        {
            this.Anchor1 = MathHelp.GetWorldPointFromLocalDirection(this.B1, this.r1);
            this.Anchor2 = MathHelp.GetWorldPointFromLocalDirection(this.B2, this.r2);

            this.CurrentPosition = (this.Anchor2 - this.Anchor1).Length();
        }

        public IExportJoint GetExportData(List<IRigidBody> bodies)
        {
            return new DistanceJointExportData()
            {
                BodyIndex1 = bodies.IndexOf(B1),
                BodyIndex2 = bodies.IndexOf(B2),
                R1 = this.r1,
                R2 = this.r2,
                CollideConnected = this.CollideConnected,
                LimitIsEnabled = this.LimitIsEnabled,
                JointIsRope = this.JointIsRope,
                MinLength = this.MinLength,
                MaxLength = this.MaxLength,
                SoftData = this.Soft.GetExportData(),
                BreakWhenMaxForceIsReached = this.BreakWhenMaxForceIsReached,
                MaxForceToBreak = this.MaxForceToBreak,
            };
        }

        public List<IConstraint> BuildConstraints(ConstraintConstructorData data)
        {
            List<IConstraint> list = new List<IConstraint>();
            if (this.JointIsRope)
            {
                list.Add(new MinMaxDistanceConstraint(data, this));
            }else
            {
                list.Add(new DistanceJointConstraint(data, this));
                if (this.LimitIsEnabled)
                    list.Add(new MinMaxDistanceConstraint(data, this));
            }
            
            return list;
        }
    }
}
