using PhysicEngine.CollisionDetection;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.SequentiellImpulseBox2DLite
{
    internal class SequentiellImpulseResolver : IImpulseResolver
    {
        private bool accumulateImpulse = true; //Wenn dieser Schalter auf false steht, bedeutet dass, das SI Kontaktpunkte auseinander
        //drückt, was aber so nie aufs richtige Ergebnis. Der Sinn von diesen Schalter hier ist zu zeigen, dass man mit negativen
        //Impulswerten arbeiten muss und lediglich die Summe geclampt werden muss

        private CollisionPointsCache<CollisionWithImpulse> pointCache
            = new CollisionPointsCache<CollisionWithImpulse>((c) => new CollisionWithImpulse(c), (oldP, newP) => newP.TakeDataFromOtherPoint(oldP));

        public float[] GetLastAppliedImpulsePerConstraint()
        {
            throw new NotImplementedException();
        }

        public void Resolve(List<IRigidBody> bodies, RigidBodyCollision[] collisions1, float dt, SolverSettings settings)
        {
            if (collisions1.Length == 0) return;
            var collisions = this.pointCache.Update(collisions1);

            float inv_dt = dt > 0.0f ? 1.0f / dt : 0.0f;

            //Berechne Hilftsvariablen für den Impuls, die nur einmal berechnet werden müssen            
            foreach (var collision in collisions)
                collision.PreStep(dt, inv_dt, settings);

            //Wende die externe Kraft für alle Körper an
            foreach (var body in bodies)
            {
                body.Velocity.X += body.InverseMass * body.Force.X * dt;
                body.Velocity.Y += body.InverseMass * body.Force.Y * dt;
                body.AngularVelocity += body.InverseInertia * body.Torque * dt;
            }

            if (settings.DoWarmStart)
            {
                foreach (var collision in collisions)
                {
                    collision.DoWarmStartImpulse();
                }
            }

            //Wende die Impulse so lange an bis die Kontaktpunkte voneinander wegfliegen
            for (int i = 0; i < settings.IterationCount; i++)
            {
                foreach (var collision in collisions)
                {
                    collision.ApplyImpulse(accumulateImpulse);
                }
            }
        }
    }
}
