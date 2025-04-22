using PhysicEngine.CollisionDetection;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.JRowAsObject
{
    //Da J und M viele Nullen enthält, versuche ich nun hier die Matrixmultiplikation zu optimieren
    internal class JRowAsObjectResolver : IImpulseResolver
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

            ResolverHelper.MoveBodiesWithConstraint(bodies, collisions, dt, settings);
        }        
    }
}
