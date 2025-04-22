using GraphicMinimal;
using PhysicEngine.CollisionDetection;

namespace PhysicEngine.CollisionResolution
{
    //Geht durch eine Liste von Kontaktpunkten durch und überall, wo die Kollisionspunkte aufeinander zufliegen, wird ein Impuls angewendet
    static class IterativeImpulse
    {
        public static void ApplyImpulsesUntilAllCollisionsAreResolved(RigidBodyCollision[] collisions, int maxIterationCount)
        {
            //Rufe so oft die ApplyImpulses-Funktion auf, bis alle Kollision-Kontaktpunkte sich voneinander wegbewegen
            //oder die Max-IterationCount erreicht ist
            for (int i=0;i< maxIterationCount;i++)
            {
                if (ApplyImpulses(collisions) == false)
                    return;
            }

            //throw new Exception("not all collisions could be resolved");
        }

        //Returnvalue: true = some Impulse was applyed; false = No Impulse was applyed
        public static bool ApplyImpulses(RigidBodyCollision[] collisions)
        {
            bool impluseWasApplyed = false;
            foreach (var collision in collisions)
            {
                if (ApplyImpulse(collision))
                    impluseWasApplyed = true;
            }

            return impluseWasApplyed;
        }

        //Quelle: https://github.com/Apress/building-a-2d-physics-game-engine/blob/master/978-1-4842-2582-0_source%20code/Chapter5/Chapter5.1ACoolDemo/public_html/EngineCore/Physics.js
        //Returnvalue: true = Impulse was applyed; false = No Impulse was applyed
        private static bool ApplyImpulse(RigidBodyCollision collision)
        {
            var s1 = collision.B1;
            var s2 = collision.B2;

            if (s1.InverseMass == 0 && s2.InverseMass == 0)
                return false;

            Vector2D n = collision.Normal;

            //the direction of collisionInfo is always from s1 to s2
            //but the Mass is inversed, so start scale with s2 and end scale with s1
            Vector2D start = collision.Start * (s2.InverseMass / (s1.InverseMass + s2.InverseMass));
            Vector2D end = collision.End * (s1.InverseMass / (s1.InverseMass + s2.InverseMass));
            Vector2D p = start + end;

            //r is vector from center of object to collision point
            Vector2D r1 = p - s1.Center;
            Vector2D r2 = p - s2.Center;

            //newV = V + mAngularVelocity cross R
            Vector2D v1 = s1.Velocity + new Vector2D(-s1.AngularVelocity * r1.Y, s1.AngularVelocity * r1.X);
            Vector2D v2 = s2.Velocity + new Vector2D(-s2.AngularVelocity * r2.Y, s2.AngularVelocity * r2.X);
            Vector2D relativeVelocity = v2 - v1;

            // Relative velocity in normal direction
            float velocityInNormal = relativeVelocity * n;

            //if objects moving apart ignore
            if (velocityInNormal >= 0)
                return false;

            // compute and apply response impulses for each object  
            float newRestituion = Math.Min(s1.Restituion, s2.Restituion);
            float newFriction = Math.Min(s1.Friction, s2.Friction);

            float R1crossN = Vector2D.ZValueFromCross(r1, n);
            float R2crossN = Vector2D.ZValueFromCross(r2, n);

            // Calc impulse scalar
            // the formula of jN can be found in http://www.myphysicslab.com/collision.html
            //float jN = -velocityInNormal - newRestituion * velocityInNormal;
            float jN = -(1 + newRestituion) * velocityInNormal;            
            jN = jN / (s1.InverseMass + s2.InverseMass +
                R1crossN * R1crossN * s1.InverseInertia +
                R2crossN * R2crossN * s2.InverseInertia);

            //impulse is in direction of normal ( from s1 to s2)
            Vector2D impulse = n * jN;

            // impulse = Integral F dt = m * delta-v
            // delta-v = impulse / m
            s1.Velocity -= impulse * s1.InverseMass;
            s2.Velocity += impulse * s2.InverseMass;

            s1.AngularVelocity -= R1crossN * jN * s1.InverseInertia;
            s2.AngularVelocity += R2crossN * jN * s2.InverseInertia;

            Vector2D tangent = relativeVelocity - n * (relativeVelocity * n);

            float tangentLength = tangent.Length();
            if (tangentLength == 0) return true;

            //relativeVelocity.dot(tangent) should less than 0
            tangent = -tangent / tangentLength;

            float r1CrossT = Vector2D.ZValueFromCross(r1, tangent);
            float r2CrossT = Vector2D.ZValueFromCross(r2, tangent);

            float jT = -(1 + newRestituion) * (relativeVelocity * tangent) * newFriction;
            jT = jT / (s1.InverseMass + s2.InverseMass +
                r1CrossT * r1CrossT * s1.InverseInertia +
                r2CrossT * r2CrossT * s2.InverseInertia);

            //friction should less than force in normal direction
            if (jT > jN)
                jT = jN;

            //impulse is from s1 to s2 (in opposite direction of velocity)
            impulse = tangent * jT;

            s1.Velocity -= impulse * s1.InverseMass;
            s2.Velocity += impulse * s2.InverseMass;
            s1.AngularVelocity -= r1CrossT * jT * s1.InverseInertia;
            s2.AngularVelocity += r2CrossT * jT * s2.InverseInertia;

            return true;
        }

    }
}
