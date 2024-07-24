using RigidBodyPhysics.CollisionResolution.SequentiellImpulse.Constraints.Weld;
using RigidBodyPhysics.CollisionResolution.SequentiellImpulse.Constraints;
using RigidBodyPhysics.ExportData.Joints;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.MaxForceTracking;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace RigidBodyPhysics.RuntimeObjects.Joints
{
    internal class WeldJoint : IJoint, IPublicWeldJoint, IPointToPointJoint, IFixAngularJoint, IPointToPointAndFixAngularJoint, IBreakableJoint
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


        public SoftConstraintData Soft { get; } //Vom Nutzer vorgegebene Softness-Parameter

        public float Stiffness
        {
            get => Soft.Stiffness;
            set => Soft.Stiffness = value;
        }
        public float Damping
        {
            get => Soft.Damping;
            set => Soft.Damping = value;
        }

        public Vec2D AccumulatedPointToPointImpulse { get; set; } = new Vec2D(0, 0);
        public float AccumulatedAngularImpulse { get; set; } = 0;
        public float AngularDifferenceOnStart { get; private set; }

        #region IBreakableJoint
        public bool IsBroken { get; set; } = false;
        public bool BreakWhenMaxForceIsReached { get; }
        public float MaxForceToBreak { get; }
        public float CurrentForce { get => AccumulatedPointToPointImpulse.Length(); } //Diese Kraft wurde im letzen TimeStep auf das Gelenk angwendet (Entspricht dem PointToPoint-AccumuletedImpulse oder dem DistanceImpluse)
        #endregion

        public WeldJoint(WeldJointExportData data, List<IRigidBody> bodies)
        {
            Body1 = B1 = bodies[data.BodyIndex1];
            Body2 = B2 = bodies[data.BodyIndex2];
            r1 = data.R1;
            r2 = data.R2;
            CollideConnected = data.CollideConnected;
            Soft = new SoftConstraintData(data.SoftData, B1, B2);
            BreakWhenMaxForceIsReached = data.BreakWhenMaxForceIsReached;
            MaxForceToBreak = data.MaxForceToBreak;

            UpdateAnchorPoints();

            AngularDifferenceOnStart = B2.Angle - B1.Angle;
        }

        public void UpdateAnchorPoints()
        {
            Anchor1 = MathHelp.GetWorldPointFromLocalDirection(B1, r1);
            Anchor2 = MathHelp.GetWorldPointFromLocalDirection(B2, r2);
        }

        public IExportJoint GetExportData(List<IRigidBody> bodies)
        {
            return new WeldJointExportData()
            {
                BodyIndex1 = bodies.IndexOf(B1),
                BodyIndex2 = bodies.IndexOf(B2),
                R1 = r1,
                R2 = r2,
                CollideConnected = CollideConnected,
                SoftData = Soft.GetExportData(),
                BreakWhenMaxForceIsReached = BreakWhenMaxForceIsReached,
                MaxForceToBreak = MaxForceToBreak,
            };
        }

        public List<IConstraint> BuildConstraints(ConstraintConstructorData data)
        {
            List<IConstraint> list = new List<IConstraint>();

            //Möglichkeit 1: Zwei Einzelconstraints
            //list.Add(new PointToPoint(data, this));
            //list.Add(new FixAngular(data, this, true));

            //Möglichkeit 2: Ein Multiconstraint mit einer 3*3-Matrix (Effektiver)
            list.Add(new PointToPointAndFixAngular(data, this));
            return list;
        }
    }
}
