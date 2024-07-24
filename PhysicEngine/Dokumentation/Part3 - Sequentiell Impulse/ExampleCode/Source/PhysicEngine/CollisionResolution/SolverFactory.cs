using PhysicEngine.CollisionResolution.EnterTheMatrix;
using PhysicEngine.CollisionResolution.IterativeImpulse;
using PhysicEngine.CollisionResolution.JRowAsObject;
using PhysicEngine.CollisionResolution.ResolverDecorator;
using PhysicEngine.CollisionResolution.SequentiellImpulse;
using static PhysicEngine.PhysicScene;

namespace PhysicEngine.CollisionResolution
{
    internal class SolverFactory
    {
        public static IImpulseResolver CreateGlobalSolver(SolverType solver)
        {
            switch (solver)
            {
                case SolverType.Matrix:
                    return new MatrixImpulseResolver();

                case SolverType.JRowAsObject:
                    return new JRowAsObjectResolver();

                case SolverType.SequentiellImpulse:
                    return new SequentiellImpulseResolver(SequentiellImpulseResolver.Variation.Original);

                case SolverType.SI_Easy0:
                    return new SequentiellImpulseResolver(SequentiellImpulseResolver.Variation.Easy0);

                case SolverType.SI_Easy1:
                    return new SequentiellImpulseResolver(SequentiellImpulseResolver.Variation.Easy1);

                case SolverType.SI_Easy2:
                    return new SequentiellImpulseResolver(SequentiellImpulseResolver.Variation.Easy2);

                case SolverType.SI_Easy3:
                    return new SequentiellImpulseResolver(SequentiellImpulseResolver.Variation.Easy3);

                case SolverType.SI_Easy4:
                    return new SequentiellImpulseResolver(SequentiellImpulseResolver.Variation.Easy4);

                case SolverType.SI_Easy5:
                    return new SequentiellImpulseResolver(SequentiellImpulseResolver.Variation.Easy5);

                case SolverType.SI_Easy6:
                    return new SequentiellImpulseResolver(SequentiellImpulseResolver.Variation.Easy6);

                case SolverType.AutoGroup:
                    return new GroupWithSpecialLogicSolver(new MatrixImpulseResolver());

                case SolverType.Box2DLite:
                    return new SequentiellImpulseBox2DLite.SequentiellImpulseResolver();

                case SolverType.IterativeImpulse:
                    return new IterativeImpulseResolver();
            }

            throw new ArgumentException("Unknown solver: " + solver);
        }

        public static IImpulseResolver CreateGroupSolver(SolverType solver)
        {
            switch (solver)
            {
                case SolverType.Matrix:
                    return new GroupedImpulseResolver(new MatrixImpulseResolver());

                case SolverType.JRowAsObject:
                    return new GroupedImpulseResolver(new JRowAsObjectResolver());

                case SolverType.SequentiellImpulse:
                    return new GroupedImpulseResolver(new SequentiellImpulseResolver(SequentiellImpulseResolver.Variation.Original));

                case SolverType.SI_Easy0:
                    return new GroupedImpulseResolver(new SequentiellImpulseResolver(SequentiellImpulseResolver.Variation.Easy0));

                case SolverType.SI_Easy1:
                    return new GroupedImpulseResolver(new SequentiellImpulseResolver(SequentiellImpulseResolver.Variation.Easy1));

                case SolverType.SI_Easy2:
                    return new GroupedImpulseResolver(new SequentiellImpulseResolver(SequentiellImpulseResolver.Variation.Easy2));

                case SolverType.SI_Easy3:
                    return new GroupedImpulseResolver(new SequentiellImpulseResolver(SequentiellImpulseResolver.Variation.Easy3));

                case SolverType.SI_Easy4:
                    return new GroupedImpulseResolver(new SequentiellImpulseResolver(SequentiellImpulseResolver.Variation.Easy4));

                case SolverType.SI_Easy5:
                    return new GroupedImpulseResolver(new SequentiellImpulseResolver(SequentiellImpulseResolver.Variation.Easy5));

                case SolverType.SI_Easy6:
                    return new GroupedImpulseResolver(new SequentiellImpulseResolver(SequentiellImpulseResolver.Variation.Easy6));

                case SolverType.AutoGroup:
                    return new GroupWithSpecialLogicSolver(new MatrixImpulseResolver());

                case SolverType.Box2DLite:
                    return new GroupedImpulseResolver(new SequentiellImpulseBox2DLite.SequentiellImpulseResolver());

                case SolverType.IterativeImpulse:
                    return new GroupedImpulseResolver(new IterativeImpulseResolver());
            }

            throw new ArgumentException("Unknown solver: " + solver);
        }
    }
}
