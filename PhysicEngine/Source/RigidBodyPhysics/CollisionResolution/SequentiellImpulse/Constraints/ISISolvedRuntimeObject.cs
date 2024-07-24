namespace RigidBodyPhysics.CollisionResolution.SequentiellImpulse.Constraints
{
    internal interface ISISolvedRuntimeObject
    {
        List<IConstraint> BuildConstraints(ConstraintConstructorData data);
    }
}
