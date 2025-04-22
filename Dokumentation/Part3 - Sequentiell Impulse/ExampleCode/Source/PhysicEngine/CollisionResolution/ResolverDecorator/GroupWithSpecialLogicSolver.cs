using GraphicMinimal;
using PhysicEngine.CollisionDetection;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.ResolverDecorator
{
    //Gruppiere CollidingContact-Collisionspunkte. Es macht mehrere Iterationen, und bis es nur noch Resting-Contactpunkte gibt
    //Bis jetzt ist diese Klasse hier ein Versuch. Die Unittests laufen damit noch nicht
    internal class GroupWithSpecialLogicSolver : IImpulseResolver
    {
        private IImpulseResolver decoree;
        public GroupWithSpecialLogicSolver(IImpulseResolver decoree)
        {
            this.decoree = decoree;
        }

        //Löse erst alle Colliding Schrittweise, indem immer der jeweils schnellste Kontakt genommen wird und danach dann noch die restlichen Kontakte
        //Hier werden 4 Tests rot (3 Tests brauchen einfach nur länger aber der CubeStack-Test funktioniert überhaupt nicht)
        public void Resolve(List<IRigidBody> bodies, RigidBodyCollision[] collisions, float dt, SolverSettings settings)
        {
            List<IRigidBody> handeldBodies = new List<IRigidBody>();

            //Löse alle Colliding-Contacts (Wird für die Newtonscraddle-Tests benötigt)
            int maxTrys = 10;
            for (int i = 0; i < maxTrys; i++)
            {
                var pointGroups = collisions
                    .GroupBy(x => GetVelocityInNormal(x))
                    .Where(x => x.Key < -settings.Gravity * dt) //Betrachte nur Colliding-Kontakte
                    .OrderBy(x => x.Key)
                    .ToList();

                if (pointGroups.Any() == false) break;

                var points = pointGroups.First().ToArray(); //Diese Kontakte bewegen sich am meisten

                var subBodies = points
                    .SelectMany(x => new List<IRigidBody>() { x.B1, x.B2 })
                    .Distinct()
                    .ToList();

                handeldBodies.AddRange(subBodies);
                decoree.Resolve(subBodies, points, dt, settings);

                if (collisions.All(x => IsMovingAway(x))) break;
            }

            //Löse RestingContacts
            {

                var points = collisions
                        .Where(x => IsRestingContact(x, dt, settings.Gravity) == true)
                        .ToArray();

                if (points.Length == 0) return;

                var subBodies = points
                    .SelectMany(x => new List<IRigidBody>() { x.B1, x.B2 })
                    .Distinct()
                    .ToList();

                handeldBodies.AddRange(subBodies);
                decoree.Resolve(subBodies, points, dt, settings);
            }

            handeldBodies = handeldBodies.Distinct().ToList();

            var bodiesWithoutConstraints = bodies.Where(x => handeldBodies.Contains(x) == false).ToList();

            //Wende die externe Kraft für alle Körper an die noch keine Behandlung erfahren haben
            foreach (var body in bodiesWithoutConstraints)
            {
                body.Velocity.X += body.InverseMass * body.Force.X * dt;
                body.Velocity.Y += body.InverseMass * body.Force.Y * dt;
                body.AngularVelocity += body.InverseInertia * body.Torque * dt;
            }
        }

        private static bool IsMovingAway(RigidBodyCollision c)
        {
            return GetVelocityInNormal(c) >= 0;
        }

        private static bool IsRestingContact(RigidBodyCollision c, float dt, float gravity)
        {
            float velocityInNormal = GetVelocityInNormal(c);
            bool isRestingContact = Math.Abs(velocityInNormal) <= gravity * dt;
            return isRestingContact;
        }

        private static float GetVelocityInNormal(RigidBodyCollision c)
        {
            //Hebelarm bestimmen
            Vector2D start = c.Start * (c.B2.InverseMass / (c.B1.InverseMass + c.B2.InverseMass));
            Vector2D end = c.End * (c.B1.InverseMass / (c.B1.InverseMass + c.B2.InverseMass));
            Vector2D p = start + end;
            Vector2D r1 = p - c.B1.Center;
            Vector2D r2 = p - c.B2.Center;

            //VelocityAtContactPoint = V + mAngularVelocity cross R
            Vector2D v1 = c.B1.Velocity + new Vector2D(-c.B1.AngularVelocity * r1.Y, c.B1.AngularVelocity * r1.X);
            Vector2D v2 = c.B2.Velocity + new Vector2D(-c.B2.AngularVelocity * r2.Y, c.B2.AngularVelocity * r2.X);
            Vector2D relativeVelocity = v2 - v1;

            // Relative velocity in normal direction
            float velocityInNormal = relativeVelocity * c.Normal;

            return velocityInNormal;
        }

        public float[] GetLastAppliedImpulsePerConstraint()
        {
            throw new NotImplementedException();
        }
    }
}
