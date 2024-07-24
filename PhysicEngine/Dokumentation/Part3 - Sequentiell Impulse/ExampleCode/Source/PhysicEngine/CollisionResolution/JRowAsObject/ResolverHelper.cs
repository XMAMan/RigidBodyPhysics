using GraphicMinimal;
using PhysicEngine.CollisionResolution.JRowAsObject.Constraints;
using PhysicEngine.MathHelper;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.JRowAsObject
{
    internal static class ResolverHelper
    {
        public static void MoveBodiesWithConstraint(List<IRigidBody> bodies, CollisionPointWithLambda[] collisions, float dt, SolverSettings settings)
        {
            //Schritt 1: Erzeuge J1, J2 und Bias für jedes Constraint-Objekt
            float invDt = dt > 0.0f ? 1.0f / dt : 0.0f;
            var constraints = new ConstraintFactory().CreateConstraints(new ConstraintConstructorData()
            {
                Bodies = bodies,
                Settings = settings,
                Dt = dt,
                InvDt = invDt
            },
                collisions);

            //Schritt 2: Erzeuge für jedes Constraint die A-Zeile und den B-Spaltenwert
            foreach (var constraint in constraints)
            {
                constraint.ARow = CreateARowForConstraint(constraint, constraints);
                constraint.B = CreateBValueForConstraint(constraint, invDt);
            }

            //Schritt 3: Ermittle mit PGS für jedes Constraint Lambda
            SolveLambaWithPGS(constraints, settings.IterationCount);

            //Schritt 4: Verschiebe die Körper aufgrund der Constraintkraft
            foreach (var constraint in constraints)
            {
                constraint.SaveLambda();
                MoveBodyWithDeltaV(constraint.J1, constraint.Lambda, constraint.B1, dt);
                MoveBodyWithDeltaV(constraint.J2, constraint.Lambda, constraint.B2, dt);
            }

            //Schritt 5: Verschiebe die Körper aufgrund der externen Kraft
            foreach (var body in bodies)
            {
                var force = new Vector3D(body.Force.X, body.Force.Y, body.Torque);
                body.Velocity.X += body.InverseMass * force.X * dt;
                body.Velocity.Y += body.InverseMass * force.Y * dt;
                body.AngularVelocity += body.InverseInertia * force.Z * dt;
            }
        }

        private static float[] CreateARowForConstraint(IConstraint c, IConstraint[] constraints)
        {
            //Berechne die Zeile aus der J*mInverse-Matrix
            Vector3D J1mInverse = new Vector3D(c.J1.X * c.B1.InverseMass, c.J1.Y * c.B1.InverseMass, c.J1.Z * c.B1.InverseInertia);
            Vector3D J2mInverse = new Vector3D(c.J2.X * c.B2.InverseMass, c.J2.Y * c.B2.InverseMass, c.J2.Z * c.B2.InverseInertia);

            //Berechne die Zeile aus der A-Matrix
            float[] ARow = new float[constraints.Length];
            for (int i = 0; i < constraints.Length; i++)
            {
                ARow[i] = 0;
                var cJ1 = constraints[i].JRow[c.Body1Index];
                if (cJ1 != null) ARow[i] += J1mInverse * cJ1; //Läßt constraints[i] eine Kraft auf c.Body1Index wirken?

                var cJ2 = constraints[i].JRow[c.Body2Index];
                if (cJ2 != null) ARow[i] += J2mInverse * cJ2; //Läßt constraints[i] eine Kraft auf c.Body2Index wirken?
            }

            return ARow;
        }

        private static float CreateBValueForConstraint(IConstraint c, float invDt)
        {
            //B-Wert
            float a = invDt * c.Bias;
            Vector3D b1 = invDt * new Vector3D(c.B1.Velocity.X, c.B1.Velocity.Y, c.B1.AngularVelocity) +
                new Vector3D(c.B1.Force.X * c.B1.InverseMass, c.B1.Force.Y * c.B1.InverseMass, c.B1.Torque * c.B1.InverseInertia);

            Vector3D b2 = invDt * new Vector3D(c.B2.Velocity.X, c.B2.Velocity.Y, c.B2.AngularVelocity) +
                new Vector3D(c.B2.Force.X * c.B2.InverseMass, c.B2.Force.Y * c.B2.InverseMass, c.B2.Torque * c.B2.InverseInertia);

            return a - (c.J1 * b1 + c.J2 * b2);
        }

        private static void SolveLambaWithPGS(IConstraint[] constraints, int iterations)
        {
            for (int i = 0; i < iterations; i++)
            {
                for (int y = 0; y < constraints.Length; y++)
                {
                    //Löse constraints[y].Lambda unter Nutzung von Zeile y
                    float sum = 0;
                    for (int j = 0; j < constraints.Length; j++)
                    {
                        if (j != y)
                            sum += constraints[y].ARow[j] * constraints[j].Lambda;
                    }
                    constraints[y].Lambda = (constraints[y].B - sum) / constraints[y].ARow[y];
                    constraints[y].Lambda = MathHelp.Clamp(constraints[y].Lambda, constraints[y].MinLambda, constraints[y].MaxLambda);
                }
            }
        }

        //Verschiebe den Körper um DeltaV aufgrund der Constraint-Kraft
        private static void MoveBodyWithDeltaV(Vector3D J, float lambda, IRigidBody B, float dt)
        {
            var force = J * lambda;
            B.Velocity.X += B.InverseMass * force.X * dt;
            B.Velocity.Y += B.InverseMass * force.Y * dt;
            B.AngularVelocity += B.InverseInertia * force.Z * dt;
        }
    }
}
