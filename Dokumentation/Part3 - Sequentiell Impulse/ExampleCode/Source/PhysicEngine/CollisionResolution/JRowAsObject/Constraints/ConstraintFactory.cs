namespace PhysicEngine.CollisionResolution.JRowAsObject.Constraints
{
    internal class ConstraintFactory
    {
        public IConstraint[] CreateConstraints(ConstraintConstructorData data, CollisionPointWithLambda[] collisions)
        {
            List<IConstraint> constraints = new List<IConstraint>();
            constraints.AddRange(collisions.Select(x => new NormalConstraint(data, x)));
            constraints.AddRange(collisions.Select(x => new FrictionConstraint(data, x)));

            return constraints.ToArray();
        }
    }
}
