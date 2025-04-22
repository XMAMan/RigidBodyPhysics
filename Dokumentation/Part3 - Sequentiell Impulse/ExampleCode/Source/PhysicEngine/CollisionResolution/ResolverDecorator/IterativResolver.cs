using PhysicEngine.CollisionDetection;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.ResolverDecorator
{
    //Wie in Part1 wird hier jeder Kontaktpunkt einzeln gelößt
    internal class IterativResolver : IImpulseResolver
    {
        private IImpulseResolver decoree;
        public IterativResolver(IImpulseResolver decoree)
        {
            this.decoree = decoree;
        }

        public float[] GetLastAppliedImpulsePerConstraint()
        {
            throw new NotImplementedException();
        }

        public void Resolve(List<IRigidBody> bodies, RigidBodyCollision[] collisions, float dt, SolverSettings settings)
        {
            if (collisions.Length == 0) return;

            foreach (var col in collisions)
            {
                var subBodies = new List<IRigidBody> { col.B1, col.B2 };
                decoree.Resolve(subBodies, new RigidBodyCollision[] { col }, dt, settings);
            }
        }
    }
}
