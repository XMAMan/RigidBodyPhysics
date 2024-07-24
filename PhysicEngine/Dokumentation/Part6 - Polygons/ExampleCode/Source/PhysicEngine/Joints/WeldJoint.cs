using PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints;
using PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints.Weld;
using PhysicEngine.ExportData.Joints;
using PhysicEngine.MathHelper;
using PhysicEngine.MaxForceTracking;
using PhysicEngine.RigidBody;

namespace PhysicEngine.Joints
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
            get => this.Soft.Stiffness; 
            set => this.Soft.Stiffness = value;
        }
        public float Damping 
        {
            get => this.Soft.Damping;
            set => this.Soft.Damping = value;
        }

        public Vec2D AccumulatedPointToPointImpulse { get; set; } = new Vec2D(0, 0);
        public float AccumulatedAngularImpulse { get; set; } = 0;
        public float AngularDifferenceOnStart { get; private set; }

        #region IBreakableJoint
        public bool BreakWhenMaxForceIsReached { get; }
        public float MaxForceToBreak { get; }
        public float CurrentForce { get => AccumulatedPointToPointImpulse.Length(); } //Diese Kraft wurde im letzen TimeStep auf das Gelenk angwendet (Entspricht dem PointToPoint-AccumuletedImpulse oder dem DistanceImpluse)
        #endregion

        public WeldJoint(WeldJointExportData data, List<IRigidBody> bodies)
        {
            this.Body1 = this.B1 = bodies[data.BodyIndex1];
            this.Body2 = this.B2 = bodies[data.BodyIndex2];
            this.r1 = data.R1;
            this.r2 = data.R2;
            this.CollideConnected = data.CollideConnected;
            this.Soft = new SoftConstraintData(data.SoftData, B1, B2);
            this.BreakWhenMaxForceIsReached = data.BreakWhenMaxForceIsReached;
            this.MaxForceToBreak = data.MaxForceToBreak;

            UpdateAnchorPoints();

            this.AngularDifferenceOnStart = this.B2.Angle - this.B1.Angle;
        }

        public void UpdateAnchorPoints()
        {
            this.Anchor1 = MathHelp.GetWorldPointFromLocalDirection(this.B1, this.r1);
            this.Anchor2 = MathHelp.GetWorldPointFromLocalDirection(this.B2, this.r2);
        }

        public IExportJoint GetExportData(List<IRigidBody> bodies)
        {
            return new WeldJointExportData()
            {
                BodyIndex1 = bodies.IndexOf(B1),
                BodyIndex2 = bodies.IndexOf(B2),
                R1 = this.r1,
                R2 = this.r2,
                CollideConnected = this.CollideConnected,
                SoftData = this.Soft.GetExportData(),
                BreakWhenMaxForceIsReached = this.BreakWhenMaxForceIsReached,
                MaxForceToBreak = this.MaxForceToBreak,
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
