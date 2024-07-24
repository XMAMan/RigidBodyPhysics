using PhysicEngine.CollisionDetection;
using PhysicEngine.Joints;
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

        public void Resolve(SolverInputData data)
        {
            if (data.Collisions.Length == 0) return;

            var collisionGroups = data.Collisions.GroupBy(x => data.Bodies.IndexOf(x.B1) + "_" + data.Bodies.IndexOf(x.B2));

            foreach (var group in collisionGroups)
            {
                var subBodies = group.SelectMany(x => new List<IRigidBody> { x.B1, x.B2 }).Distinct().ToList();
                var points = group.ToArray();

                decoree.Resolve(new SolverInputData(data) { Bodies = subBodies, Collisions = points });
            }
        }
    }
}
