using PhysicEngine.CollisionDetection;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.ResolverDecorator
{
    //Gruppiert Kollisionspunkte laut gemeinsamen Objekten für B1 und B2 (Sinnvoll für NoGravity-Newtonscraddle-Tests)
    internal class GroupedImpulseResolver : IImpulseResolver
    {
        private IImpulseResolver decoree;
        public GroupedImpulseResolver(IImpulseResolver decoree)
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

            var collisionGroups = collisions.GroupBy(x => bodies.IndexOf(x.B1) + "_" + bodies.IndexOf(x.B2));

            foreach (var group in collisionGroups)
            {
                var subBodies = group.SelectMany(x => new List<IRigidBody> { x.B1, x.B2 }).Distinct().ToList();
                var points = group.ToArray();

                decoree.Resolve(subBodies, points, dt, settings);
            }
        }
    }
}
