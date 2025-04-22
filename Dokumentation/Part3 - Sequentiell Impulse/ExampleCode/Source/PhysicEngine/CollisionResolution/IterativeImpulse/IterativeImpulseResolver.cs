using GraphicMinimal;
using PhysicEngine.CollisionDetection;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.IterativeImpulse
{
    //Iteratives Impuls-Verfahren ohne Schwerkraft und Reibung. Diese Klasse dient zum Vergleich mit PhysicEngine.CollisionResolution.SequentiellImpulse.Examples.ResolverHelper1
    internal class IterativeImpulseResolver : IImpulseResolver
    {
        public void Resolve(List<IRigidBody> bodies, RigidBodyCollision[] collisions, float dt, SolverSettings settings)
        {
            ApplyImpulsesUntilAllCollisionsAreResolved(collisions, settings.IterationCount);
        }

        public static void ApplyImpulsesUntilAllCollisionsAreResolved(RigidBodyCollision[] collisions, int maxIterationCount)
        {
            for (int i = 0; i < maxIterationCount; i++)
                foreach (var collision in collisions)
                    ApplyImpulse(collision);
        }
        private static void ApplyImpulse(RigidBodyCollision collision)
        {
            var B1 = collision.B1;
            var B2 = collision.B2;
            Vector2D n = collision.Normal;
            Vector2D start = collision.Start * (B2.InverseMass / (B1.InverseMass + B2.InverseMass));
            Vector2D end = collision.End * (B1.InverseMass / (B1.InverseMass + B2.InverseMass));
            Vector2D p = start + end;
            Vector2D r1 = p - B1.Center;
            Vector2D r2 = p - B2.Center;
            Vector2D v1 = B1.Velocity + new Vector2D(-B1.AngularVelocity * r1.Y, B1.AngularVelocity * r1.X);
            Vector2D v2 = B2.Velocity + new Vector2D(-B2.AngularVelocity * r2.Y, B2.AngularVelocity * r2.X);
            Vector2D relativeVelocity = v2 - v1;
            float velocityInNormal = relativeVelocity * n;
            if (velocityInNormal >= 0) return; //if objects moving apart ignore
            float restituion = Math.Min(B1.Restituion, B2.Restituion);
            float R1crossN = Vector2D.ZValueFromCross(r1, n);
            float R2crossN = Vector2D.ZValueFromCross(r2, n);

            float jN = -restituion * velocityInNormal - velocityInNormal;
            jN = jN / (B1.InverseMass + B2.InverseMass +
                R1crossN * R1crossN * B1.InverseInertia +
                R2crossN * R2crossN * B2.InverseInertia);

            Vector2D impulse = n * jN;

            B1.Velocity -= impulse * B1.InverseMass;
            B2.Velocity += impulse * B2.InverseMass;

            B1.AngularVelocity -= R1crossN * jN * B1.InverseInertia;
            B2.AngularVelocity += R2crossN * jN * B2.InverseInertia;
        }

        public float[] GetLastAppliedImpulsePerConstraint()
        {
            throw new NotImplementedException();
        }
    }
}
