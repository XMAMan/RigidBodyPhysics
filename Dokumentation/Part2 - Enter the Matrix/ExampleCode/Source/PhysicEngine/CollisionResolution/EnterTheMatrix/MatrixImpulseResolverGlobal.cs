using PhysicEngine.CollisionDetection;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.EnterTheMatrix
{
    //Umsetzung des Papers "Iterative Dynamics with Temporal Coherence - Erin Catto 2005"
    //Stelle Gleichungssystem über alle Kontaktpunkte auf und Löse dafür Lambda
    internal class MatrixImpulseResolverGlobal : IImpulseResolver
    {
        public void Resolve(List<IRigidBody> bodies, RigidBodyCollision[] collisions, float dt, SolverSettings settings)
        {
            if (collisions.Length == 0) return;
            
            var solve = new EquationOfMotionData(bodies, collisions, dt, settings);
            solve.SetVelocityValues(bodies);

            string testOutput = solve.GetOutput();           
        }
    }
}
