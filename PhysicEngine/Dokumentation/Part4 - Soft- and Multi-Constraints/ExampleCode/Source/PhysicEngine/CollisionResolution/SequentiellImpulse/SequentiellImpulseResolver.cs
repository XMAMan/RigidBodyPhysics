using PhysicEngine.CollisionResolution.SequentiellImpulse.Examples;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse
{
    //Bei JRowAsObjectResolverGlobal wird eine A-Matrix erstellt womit geschaut wird, wie stark andere Objekte sich
    //Beschleunigen, wenn der Constraint angewendet wird. Stattdessen beschleunige ich die anderen Objekte beim PGS-
    //Step wirklich und beschleunige sie aber auch wieder zurück, wenn es zu viel war
    internal class SequentiellImpulseResolver : IImpulseResolver
    {
        private CollisionPointsCache pointCache = new CollisionPointsCache();

        public void Resolve(SolverInputData data)
        {
            if (data.Collisions.Length == 0 && data.Joints.Count == 0 && data.MouseData==null) return;
            var collisions = this.pointCache.Update(data.Collisions);

            switch (data.ResolverHelper)
            {
                case PhysicScene.Helper.Original:
                    ResolverHelper.MoveBodiesWithConstraint(data.Bodies, data.Joints, collisions, data.MouseData, data.Dt, data.Settings);
                    break;

                case PhysicScene.Helper.Helper1:
                    ResolverHelper1.MoveBodiesWithConstraint(data.Bodies, data.Joints, collisions, data.MouseData, data.Dt, data.Settings);
                    break;

                case PhysicScene.Helper.Helper2:
                    ResolverHelper2.MoveBodiesWithConstraint(data.Bodies, data.Joints, data.Dt, data.Settings, ResolverHelper2.Variation.Formula_2_13);
                    break;

                case PhysicScene.Helper.Helper3:
                    ResolverHelper2.MoveBodiesWithConstraint(data.Bodies, data.Joints, data.Dt, data.Settings, ResolverHelper2.Variation.Formula_2_17);
                    break;
            }
            
        }
    }
}
