using PhysicEngine.CollisionResolution.ResolverDecorator;
using PhysicEngine.CollisionResolution.SequentiellImpulse;
using static PhysicEngine.PhysicScene;

namespace PhysicEngine.CollisionResolution
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
