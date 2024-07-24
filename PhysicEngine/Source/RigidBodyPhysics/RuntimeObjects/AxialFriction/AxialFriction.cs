using RigidBodyPhysics.CollisionResolution.SequentiellImpulse.Constraints;
using RigidBodyPhysics.ExportData.AxialFriction;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using RigidBodyPhysics.CollisionResolution.SequentiellImpulse.Constraints.AxialFriction;

namespace RigidBodyPhysics.RuntimeObjects.AxialFriction
{
    //Reibung, die nur in Richtung der Axe wirkt
    internal class AxialFriction : IAxialFriction
    {
        private Vec2D r1; //lokaler Richtungsvektor von B1.Center nach Anchor
        private Vec2D forceDirection; //Lokaler Richtungsvektor

        #region IPublicAxialFriction
        public IPublicRigidBody Body { get; private set; }
        public Vec2D Anchor { get; private set; }
        public Vec2D ForceDirection { get; private set; } //Richtungsvektor im Globalspace
        public float Friction { get; set; }
        #endregion

        #region IAxialFriction
        public IRigidBody B1 { get; private set; }
        public Vec2D R1 { get; private set; } //Angabe in Weltkoordinaten (Hebelarm vom Center zum Anchor-Punkt)      
        public float AccumulatedFrictionImpulse { get; set; } = 0; //Aufsummierte Impulse von der Reibung
        public void UpdateAnchorPoints() //Muss aufgerufen werden, wenn sich die Position der Bodys geändert hat
        {
            Anchor = MathHelp.GetWorldPointFromLocalDirection(B1, r1);
            R1 = Anchor - B1.Center;
            ForceDirection = MathHelp.GetWorldDirectionFromLocalDirection(B1, forceDirection);
        }
        #endregion

        public AxialFriction(AxialFrictionExportData data, List<IRigidBody> bodies)
        {
            Body = B1 = bodies[data.BodyIndex];
            r1 = data.R1;
            forceDirection = data.ForceDirection;
            Friction = data.Friction;

            UpdateAnchorPoints();
        }

        #region IExportableAxialFriction
        public IExportAxialFriction GetExportData(List<IRigidBody> bodies)
        {
            return new AxialFrictionExportData()
            {
                BodyIndex = bodies.IndexOf(B1),
                R1 = r1,
                ForceDirection = forceDirection,
                Friction = Friction
            };
        }
        #endregion

        public List<IConstraint> BuildConstraints(ConstraintConstructorData data)
        {
            List<IConstraint> list = new List<IConstraint>();

            if (this.Friction != 0)
            {
                list.Add(new AxialFrictionConstraint(data, this));
            }           

            return list;
        }
    }
}
