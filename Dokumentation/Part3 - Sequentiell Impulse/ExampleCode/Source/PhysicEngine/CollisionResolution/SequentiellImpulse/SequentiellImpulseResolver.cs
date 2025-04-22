using PhysicEngine.CollisionDetection;
using PhysicEngine.CollisionResolution.SequentiellImpulse.Examples;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse
{
    //Bei JRowAsObjectResolverGlobal wird eine A-Matrix erstellt womit geschaut wird, wie stark andere Objekte sich
    //Beschleunigen, wenn der Constraint angewendet wird. Stattdessen beschleunige ich die anderen Objekte beim PGS-
    //Step wirklich und beschleunige sie aber auch wieder zurück, wenn es zu viel war
    internal class SequentiellImpulseResolver : IImpulseResolver
    {
        private CollisionPointsCache<CollisionPointWithImpulse> pointCache
            = new CollisionPointsCache<CollisionPointWithImpulse>((c) => new CollisionPointWithImpulse(c), (oldP, newP) => newP.TakeDataFromOtherPoint(oldP));

        private Variation variation;

        public SequentiellImpulseResolver(Variation variation) 
        {
            this.variation = variation;
        }


        public enum Variation
        {
            Original,
            Easy0, //Ergibt sich aus der Herleitung über das Bild, wo man mehrere Constraint-Ebenen als Linien hat
            Easy1, //Sieht aus wie Iterative Impulse: Nur mit vereinfachten Normalconstraint, ohne WarmStart/AccumulatedImpulse/ExterneKraft
            Easy2, //Wie Easy1 nur zusätzlich noch mit FrictionConstraint
            Easy3, //Wie Easy2 nur zusätzlich noch mit AccumulatedImpulse
            Easy4, //Wie Easy3 nur zusätzlich noch mit externer Kraft welche vor der Constraint-Erstellung angewendet wird
            Easy5, //Wie Easy3 nur zusätzlich noch mit externer Kraft welche nach der Constraint-Erstellung angewendet wird
            Easy6, //Wie Easy3 nur zusätzlich noch mit externer Kraft welche nach der Constraint-Kraftanwendung angewendet wird
        }

        private float[] lastAppliedImpulsePerConstraint = null;

        public void Resolve(List<IRigidBody> bodies, RigidBodyCollision[] collisions1, float dt, SolverSettings settings)
        {
            if (collisions1.Length == 0) return;
            var collisions = this.pointCache.Update(collisions1);

            switch (this.variation)
            {
                case Variation.Original:
                    ResolverHelper.MoveBodiesWithConstraint(bodies, collisions, dt, settings);
                    break;

                case Variation.Easy0:
                    ResolverHelper0.MoveBodiesWithConstraint(bodies, collisions, settings, dt);
                    break;

                case Variation.Easy1:
                    this.lastAppliedImpulsePerConstraint = ResolverHelper1.MoveBodiesWithConstraint(collisions, settings);
                    break;

                case Variation.Easy2:
                    this.lastAppliedImpulsePerConstraint = ResolverHelper2.MoveBodiesWithConstraint(bodies, collisions, dt, settings, false);
                    break;

                case Variation.Easy3:
                    this.lastAppliedImpulsePerConstraint = ResolverHelper2.MoveBodiesWithConstraint(bodies, collisions, dt, settings, true);
                    break;

                case Variation.Easy4:
                    this.lastAppliedImpulsePerConstraint = ResolverHelper3.MoveBodiesWithConstraint(bodies, collisions, dt, settings, ResolverHelper3.Variation.Variation1);
                    break;

                case Variation.Easy5:
                    this.lastAppliedImpulsePerConstraint = ResolverHelper3.MoveBodiesWithConstraint(bodies, collisions, dt, settings, ResolverHelper3.Variation.Variation2);
                    break;

                case Variation.Easy6:
                    this.lastAppliedImpulsePerConstraint = ResolverHelper3.MoveBodiesWithConstraint(bodies, collisions, dt, settings, ResolverHelper3.Variation.Variation3);
                    break;
            }     
        }

        public float[] GetLastAppliedImpulsePerConstraint()
        {
            return this.lastAppliedImpulsePerConstraint;
        }
    }
}
