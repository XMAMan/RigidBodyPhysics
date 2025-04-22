using RigidBodyPhysics.CollisionResolution.SequentiellImpulse.Constraints;
using RigidBodyPhysics.CollisionResolution.SequentiellImpulse.Constraints.Distance;
using RigidBodyPhysics.ExportData.Joints;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.MaxForceTracking;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace RigidBodyPhysics.RuntimeObjects.Joints
{
    internal class DistanceJoint : IJoint, IPublicDistanceJoint, IBreakablePushPullJoint
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
        public bool IsBroken { get; set; } = false;
        public bool BreakWhenMaxForceIsReached { get; }
        public float MinForceToBreak { get; } //IBreakablePushPullJoint
        public float MaxForceToBreak { get; }
        public float CurrentForce { get => AccumulatedImpulse; } //Diese Kraft wurde im letzen TimeStep auf das Gelenk angwendet (Entspricht dem PointToPoint-AccumuletedImpulse oder dem DistanceImpluse)
        #endregion

        internal SoftConstraintData Soft; //Vom Nutzer vorgegebene Softness-Parameter

        public float AccumulatedImpulse { get; internal set; } = 0; //Für DistanceJointConstraint
        public float AccumulatedImpulseForMinMax { get; internal set; } = 0; //Für MinMaxDistanceConstraint

        public DistanceJoint(DistanceJointExportData data, List<IRigidBody> bodies)
        {
            Body1 = B1 = bodies[data.BodyIndex1];
            Body2 = B2 = bodies[data.BodyIndex2];
            r1 = data.R1;
            r2 = data.R2;
            CollideConnected = data.CollideConnected;
            LimitIsEnabled = data.LimitIsEnabled;
            JointIsRope = data.JointIsRope;
            MinLength = data.MinLength;
            MaxLength = data.MaxLength;
            BreakWhenMaxForceIsReached = data.BreakWhenMaxForceIsReached;
            MinForceToBreak = data.MinForceToBreak;
            MaxForceToBreak = data.MaxForceToBreak;

            Soft = new SoftConstraintData(data.SoftData, B1, B2);

            UpdateAnchorPoints();

            LengthPosition = CurrentPosition;
        }

        public void UpdateAnchorPoints()
        {
            Anchor1 = MathHelp.GetWorldPointFromLocalDirection(B1, r1);
            Anchor2 = MathHelp.GetWorldPointFromLocalDirection(B2, r2);

            CurrentPosition = (Anchor2 - Anchor1).Length();
        }

        public IExportJoint GetExportData(List<IRigidBody> bodies)
        {
            return new DistanceJointExportData()
            {
                BodyIndex1 = bodies.IndexOf(B1),
                BodyIndex2 = bodies.IndexOf(B2),
                R1 = r1,
                R2 = r2,
                CollideConnected = CollideConnected,
                LimitIsEnabled = LimitIsEnabled,
                JointIsRope = JointIsRope,
                MinLength = MinLength,
                MaxLength = MaxLength,
                SoftData = Soft.GetExportData(),
                BreakWhenMaxForceIsReached = BreakWhenMaxForceIsReached,
                MinForceToBreak = MinForceToBreak,
                MaxForceToBreak = MaxForceToBreak,
            };
        }

        public List<IConstraint> BuildConstraints(ConstraintConstructorData data)
        {
            List<IConstraint> list = new List<IConstraint>();
            if (JointIsRope)
            {
                list.Add(new MinMaxDistanceConstraint(data, this));
            }
            else
            {
                list.Add(new DistanceJointConstraint(data, this));
                if (LimitIsEnabled)
                    list.Add(new MinMaxDistanceConstraint(data, this));
            }

            return list;
        }
    }
}
