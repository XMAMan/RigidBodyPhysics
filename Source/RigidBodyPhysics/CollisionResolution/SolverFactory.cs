using RigidBodyPhysics.CollisionResolution.ResolverDecorator;
using RigidBodyPhysics.CollisionResolution.SequentiellImpulse;
using static RigidBodyPhysics.PhysicScene;

namespace RigidBodyPhysics.CollisionResolution
{
    internal class SolverFactory
    {
        internal static IImpulseResolver CreateSolver(SolverType solver)
        {
            switch (solver)
            {
                case SolverType.Global:
                    return new SequentiellImpulseResolver();

                case SolverType.Grouped:
                    return new GroupedImpulseResolver(new SequentiellImpulseResolver());
            }

            throw new ArgumentException("Unknown solver: " + solver);
        }
    }
}
