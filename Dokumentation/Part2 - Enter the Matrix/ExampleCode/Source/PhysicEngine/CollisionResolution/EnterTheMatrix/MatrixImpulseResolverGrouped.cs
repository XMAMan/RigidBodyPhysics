using PhysicEngine.CollisionDetection;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.EnterTheMatrix
{
    //Gruppiere Collisionspunkte, wo B1 und B2 auf das gleiche Objekt zeigen (Würfel ohne Drehung gegen Wand/Tisch)
    internal class MatrixImpulseResolverGrouped : IImpulseResolver
    {
        public void Resolve(List<IRigidBody> bodies, RigidBodyCollision[] collisions, float dt, SolverSettings settings)
        {
            if (collisions.Length == 0) return;

            var collisionGroups = collisions.GroupBy(x => bodies.IndexOf(x.B1) + "_" + bodies.IndexOf(x.B2));

            foreach (var group in  collisionGroups)
            {
                var subBodies = group.SelectMany(x => new List<IRigidBody> { x.B1, x.B2 }).Distinct().ToList();

                var solve = new EquationOfMotionData(subBodies, group.ToArray(), dt, settings);
                solve.SetVelocityValues(subBodies);

                string testOutput = solve.GetOutput();
            }
            
        }
    }
    
}
