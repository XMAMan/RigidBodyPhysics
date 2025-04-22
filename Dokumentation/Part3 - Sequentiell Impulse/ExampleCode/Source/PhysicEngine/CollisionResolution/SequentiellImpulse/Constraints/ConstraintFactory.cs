namespace PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints
{
    internal class ConstraintFactory
    {
        public IConstraint[] CreateConstraints(ConstraintConstructorData data, CollisionPointWithImpulse[] collisions)
        {
            List<IConstraint> constraints = new List<IConstraint>();

            constraints.AddRange(collisions.Select(x => new NormalConstraint(data, x)));
            constraints.AddRange(collisions.Select(x => new FrictionConstraint(data, x)));

            return constraints.ToArray();
        }
    }
}
