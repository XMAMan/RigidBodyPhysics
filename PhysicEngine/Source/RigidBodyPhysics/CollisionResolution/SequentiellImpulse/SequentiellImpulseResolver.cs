namespace RigidBodyPhysics.CollisionResolution.SequentiellImpulse
{
    //Berechnet die Kräfte, die nötig sind, um Kollisionen zu vermeiden und die Gelenkkräfte
    //Mit dieser Kraft wird dann die Geschwindigkeit der Körper korrigiert
    internal class SequentiellImpulseResolver : IImpulseResolver
    {
        private CollisionPointsCache pointCache = new CollisionPointsCache();

        public void Resolve(SolverInputData data)
        {
            //Übernehme vom vorherigen TimeStep die Collisionpoints um auf diese Weise den Accumulierten Impuls als Startwert für den nächsten TimeStep zu nehmen (WarmStart)
            var collisions = this.pointCache.Update(data.Collisions);

            ResolverHelper.MoveBodiesWithConstraint(data.Bodies, data.Joints, data.Thrusters, data.Motors, data.AxialFrictions, collisions, data.MouseData, data.Dt, data.Settings);
        }
    }
}
