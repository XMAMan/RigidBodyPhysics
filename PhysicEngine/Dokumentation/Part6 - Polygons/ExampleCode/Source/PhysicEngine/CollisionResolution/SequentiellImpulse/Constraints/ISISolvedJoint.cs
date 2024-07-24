namespace PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints
{
    internal interface ISISolvedJoint
    {
        List<IConstraint> BuildConstraints(ConstraintConstructorData data);
    }
}
