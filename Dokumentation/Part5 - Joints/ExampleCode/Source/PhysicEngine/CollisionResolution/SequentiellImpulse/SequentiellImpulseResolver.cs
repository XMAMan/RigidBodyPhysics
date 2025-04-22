namespace PhysicEngine.CollisionResolution.SequentiellImpulse
{
    //Berechnet die Kräfte, die nötig sind, um Kollisionen zu vermeiden und die Gelenkkräfte
    //Mit dieser Kraft wird dann die Geschwindigkeit der Körper korrigiert
    internal class SequentiellImpulseResolver : IImpulseResolver
    {
        private CollisionPointsCache pointCache = new CollisionPointsCache();

        public void Resolve(SolverInputData data)
        {
            if (data.Collisions.Length == 0 && data.Joints.Count == 0 && data.MouseData==null) return;
            var collisions = this.pointCache.Update(data.Collisions);

            ResolverHelper.MoveBodiesWithConstraint(data.Bodies, data.Joints, collisions, data.MouseData, data.Dt, data.Settings);
        }
    }
}
