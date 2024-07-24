using GraphicMinimal;
using PhysicEngine.CollisionDetection;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse.Examples
{
    //Das ist das Sequentielle Impuls-Verfahren ohne Impulsclamping/AccumulatedImpulse/PositionCorrection/Reibung 
    internal class ResolverHelper0
    {
        public static void MoveBodiesWithConstraint(List<IRigidBody> bodies, RigidBodyCollision[] collisions, SolverSettings settings, float dt)
        {
            var constraints = collisions.Select(x => new NormalConstraint0(x)).ToList();

            //1. Apply Gravity-Force
            foreach (var body in bodies)
            {
                body.Velocity.X += body.InverseMass * body.Force.X * dt;
                body.Velocity.Y += body.InverseMass * body.Force.Y * dt;
                body.AngularVelocity += body.InverseInertia * body.Torque * dt;
            }

            //2. Apply Constraint-Forces
            for (int i = 0; i < settings.IterationCount; i++)
            {
                foreach (var c in constraints)
                {
                    Vector2D v1 = c.B1.Velocity + new Vector2D(-c.B1.AngularVelocity * c.R1.Y, c.B1.AngularVelocity * c.R1.X);
                    Vector2D v2 = c.B2.Velocity + new Vector2D(-c.B2.AngularVelocity * c.R2.Y, c.B2.AngularVelocity * c.R2.X);
                    Vector2D relativeVelocity = v2 - v1;
                    float velocityInForceDirection = relativeVelocity * c.ForceDirection;
                    float impulse = c.ImpulseMass * (c.Bias - velocityInForceDirection);
                    ApplyImpulse(c, impulse);
                }
            }
        }

        //Impulse = m * deltaV -> Impulse/m = deltaV
        private static void ApplyImpulse(NormalConstraint0 c, float impulse)
        {
            Vector2D impulseVector = impulse * c.ForceDirection;
            c.B1.Velocity -= impulseVector * c.B1.InverseMass;
            c.B1.AngularVelocity -= Vector2D.ZValueFromCross(c.R1, impulseVector) * c.B1.InverseInertia;

            c.B2.Velocity += impulseVector * c.B2.InverseMass;
            c.B2.AngularVelocity += Vector2D.ZValueFromCross(c.R2, impulseVector) * c.B2.InverseInertia;
        }
    }

    internal class NormalConstraint0
    {
        public Vector2D ForceDirection { get; } //In diese Richtung wird B2 gedrückt (B1 wird entgegengesetzt gedrückt)
        public Vector2D R1 { get; } //Hebelarm vom B1.Center zum Kontaktpunkt
        public Vector2D R2 { get; } //Hebelarm vom B2.Center zum Kontaktpunkt
        public IRigidBody B1 { get; }
        public IRigidBody B2 { get; }
        public float Bias { get; }
        public float ImpulseMass { get; } //Umrechungsvektor vom Relative-Kontaktpunktgeschwindigkeitswert in ein Impuls

        public NormalConstraint0(RigidBodyCollision point)
        {
            this.B1 = point.B1;
            this.B2 = point.B2;

            var c = point;

            //Hebelarm bestimmen
            Vector2D start = c.Start * (c.B2.InverseMass / (c.B1.InverseMass + c.B2.InverseMass));
            Vector2D end = c.End * (c.B1.InverseMass / (c.B1.InverseMass + c.B2.InverseMass));
            Vector2D p = start + end;
            this.R1 = p - c.B1.Center;
            this.R2 = p - c.B2.Center;
            float r1crossN = Vector2D.ZValueFromCross(this.R1, c.Normal);
            float r2crossN = Vector2D.ZValueFromCross(this.R2, c.Normal);

            this.ImpulseMass = 1.0f / (B1.InverseMass + B2.InverseMass +
                r1crossN * r1crossN * B1.InverseInertia +
                r2crossN * r2crossN * B2.InverseInertia);

            this.ForceDirection = c.Normal;
            this.Bias = GetBias(c, this.R1, this.R2);
        }

        private float GetBias(RigidBodyCollision c, Vector2D r1, Vector2D r2)
        {
            Vector2D v1 = c.B1.Velocity + new Vector2D(-c.B1.AngularVelocity * r1.Y, c.B1.AngularVelocity * r1.X);
            Vector2D v2 = c.B2.Velocity + new Vector2D(-c.B2.AngularVelocity * r2.Y, c.B2.AngularVelocity * r2.X);
            Vector2D relativeVelocity = v2 - v1;
            float velocityInNormal = relativeVelocity * c.Normal;
            float restituion = Math.Min(c.B1.Restituion, c.B2.Restituion);
            return -restituion * velocityInNormal;
        }
    }
}
