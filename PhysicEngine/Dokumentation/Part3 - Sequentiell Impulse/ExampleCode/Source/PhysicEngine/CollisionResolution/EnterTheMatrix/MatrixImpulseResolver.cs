using PhysicEngine.CollisionDetection;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.EnterTheMatrix
{
    //Umsetzung des Papers "Iterative Dynamics with Temporal Coherence - Erin Catto 2005"
    //Stelle Gleichungssystem über alle Kontaktpunkte auf und Löse dafür Lambda
    internal class MatrixImpulseResolver : IImpulseResolver
    {
		private CollisionPointsCache<CollisionPointWithLambda> pointCache
			= new CollisionPointsCache<CollisionPointWithLambda>((c) => new CollisionPointWithLambda(c), (oldP, newP) => newP.TakeDataFromOtherPoint(oldP));

        public float[] GetLastAppliedImpulsePerConstraint()
        {
            throw new NotImplementedException();
        }

        public void Resolve(List<IRigidBody> bodies, RigidBodyCollision[] collisions1, float dt, SolverSettings settings)
        {
            if (collisions1.Length == 0) return;
			var collisions = this.pointCache.Update(collisions1);

            var solve = new EquationOfMotionData(bodies, collisions, dt, settings);
            solve.SetVelocityValues(bodies);

            //string testOutput = solve.GetOutput();           
        }
    }
}
