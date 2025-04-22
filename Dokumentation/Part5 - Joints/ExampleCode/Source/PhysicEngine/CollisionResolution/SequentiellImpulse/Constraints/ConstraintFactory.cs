using PhysicEngine.MouseBodyClick;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints
{
    internal class ConstraintFactory
    {
        internal IConstraint[] CreateConstraints(ConstraintConstructorData data, CollisionPointWithImpulse[] collisions)
        {
            List<IConstraint> constraints = new List<IConstraint>();

            constraints.AddRange(collisions.Select(x => new NormalConstraint(data, x)));
            constraints.AddRange(collisions.Select(x => new FrictionConstraint(data, x)));
            
            constraints.AddRange(data.Joints.SelectMany(x => x.BuildConstraints(data)));

            if (data.MouseData != null)
            {
                constraints.Add(new MouseConstraint(data, data.MouseData));
            }

            return constraints.ToArray();
        }
    }
}
