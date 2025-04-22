using PhysicEngine.CollisionDetection;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.EnterTheMatrix
{
    //Wie in Part1 wird hier jeder Kontaktpunkt einzeln gelößt
    internal class MatrixImpulseResolverIterativ : IImpulseResolver
    {
        public void Resolve(List<IRigidBody> bodies, RigidBodyCollision[] collisions, float dt, SolverSettings settings)
        {
            if (collisions.Length == 0) return;

            foreach (var col in collisions)
            {
                var subBodies = new List<IRigidBody> { col.B1, col.B2 };
                var solve = new EquationOfMotionData(subBodies, new RigidBodyCollision[] { col }, dt, settings);
                solve.SetVelocityValues(subBodies);

                string testOutput = solve.GetOutput();
            }
        }
    }
}
