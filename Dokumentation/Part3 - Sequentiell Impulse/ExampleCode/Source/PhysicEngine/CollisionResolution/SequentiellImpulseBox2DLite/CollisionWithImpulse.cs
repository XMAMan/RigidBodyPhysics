using GraphicMinimal;
using PhysicEngine.CollisionDetection;
using PhysicEngine.MathHelper;

namespace PhysicEngine.CollisionResolution.SequentiellImpulseBox2DLite
{
    //Speichert die Impulsenergie über alle letzten TimeSteps
    //Quelle für die Idee: https://github.com/erincatto/box2d-lite/blob/master/src/Arbiter.cpp
    //Hier soll per sequentiell Impuls gearbeitet werden aber das funktioniert nicht, weil er die Geschwindigkeit sofort beim
    //iterieren verändert
    internal class CollisionWithImpulse : RigidBodyCollision
    {
        //Wird über alle TimeSteps berechnet
        public float AccumulatedNormalImpulse = 0;
        public float AccumulatedTangentImpulse = 0;

        //Wird in jeden TimeStep erneut berechnet
        private Vector2D r1; //Hebelarm von Body1Center zum Kollisionspunkt
        private Vector2D r2; //Hebelarm von Body2Center zum Kollisionspunkt
        private float massNormal;
        private float massTangent;
        private float normalBias;
        private float maxFrictionImpulse;

        public CollisionWithImpulse(RigidBodyCollision c)
            : base(c)
        {            
        }

        public void TakeDataFromOtherPoint(CollisionWithImpulse c)
        {
            this.AccumulatedNormalImpulse = c.AccumulatedNormalImpulse;
            this.AccumulatedTangentImpulse = c.AccumulatedTangentImpulse;
        }

        //Berechnet den Nenner von der Impuls-Energieformel (Siehe Formel 11 Part1.odt)
        //invDt = 1.0f / dt
        public void PreStep(float dt, float invDt, SolverSettings s)
        {
            //the direction of collisionInfo is always from s1 to s2
            //but the Mass is inversed, so start scale with s2 and end scale with s1
            Vector2D start = Start * (B2.InverseMass / (B1.InverseMass + B2.InverseMass));
            Vector2D end = End * (B1.InverseMass / (B1.InverseMass + B2.InverseMass));
            Vector2D p = start + end;

            //r is vector from center of object to collision point
            r1 = p - B1.Center;
            r2 = p - B2.Center;

            float r1crossN = Vector2D.ZValueFromCross(r1, Normal);
            float r2crossN = Vector2D.ZValueFromCross(r2, Normal);

            massNormal = 1.0f / (B1.InverseMass + B2.InverseMass +
                r1crossN * r1crossN * B1.InverseInertia +
                r2crossN * r2crossN * B2.InverseInertia);

            Vector2D tangent = Vector2D.CrossWithZ(Normal, 1.0f);
            float r1CrossT = Vector2D.ZValueFromCross(r1, tangent);
            float r2CrossT = Vector2D.ZValueFromCross(r2, tangent);

            massTangent = 1.0f / (B1.InverseMass + B2.InverseMass +
                r1CrossT * r1CrossT * B1.InverseInertia +
                r2CrossT * r2CrossT * B2.InverseInertia);


            //RestituionBias berechnen
            //VelocityAtContactPoint = V + mAngularVelocity cross R
            Vector2D v1 = B1.Velocity + new Vector2D(-B1.AngularVelocity * r1.Y, B1.AngularVelocity * r1.X);
            Vector2D v2 = B2.Velocity + new Vector2D(-B2.AngularVelocity * r2.Y, B2.AngularVelocity * r2.X);
            Vector2D relativeVelocity = v2 - v1;

            // Relative velocity in normal direction
            float velocityInNormal = relativeVelocity * Normal;
            float restituion = Math.Min(B1.Restituion, B2.Restituion);

            float restitutionBias = -restituion * velocityInNormal;

            float biasFactor = s.DoPositionalCorrection ? s.PositionalCorrectionRate : 0.0f;
            float positionBias = biasFactor * invDt * Math.Max(0, Depth - s.AllowedPenetration);

            this.normalBias = restitutionBias + positionBias;

            //Max-Friction-Impulse
            float friction = Math.Max(B1.Friction, B2.Friction);
            this.maxFrictionImpulse = s.Gravity * friction * 0.15f * dt;
        }

        public void DoWarmStartImpulse()
        {
            // Apply normal + friction impulse
            Vector2D tangent = Vector2D.CrossWithZ(Normal, 1.0f);
            Vector2D impulse = AccumulatedNormalImpulse * Normal + AccumulatedTangentImpulse * tangent;
            ApplyImpulse(impulse);
        }

        public void ApplyImpulse(bool accumulateImpulse)
        {
            //VelocityAtContactPoint = V + mAngularVelocity cross R
            Vector2D v1 = B1.Velocity + new Vector2D(-B1.AngularVelocity * r1.Y, B1.AngularVelocity * r1.X);
            Vector2D v2 = B2.Velocity + new Vector2D(-B2.AngularVelocity * r2.Y, B2.AngularVelocity * r2.X);
            Vector2D relativeVelocity = v2 - v1;

            // Relative velocity in normal direction
            float velocityInNormal = relativeVelocity * Normal;

            //float dPn = massNormal * (-velocityInNormal + positionBias);        //https://github.com/erincatto/box2d-lite/blob/master/src/Arbiter.cpp Line 142

            float dPn = this.massNormal * (this.normalBias - velocityInNormal);//https://kevinyu.net/2018/01/17/understanding-constraint-solver-in-physics-engine/

            if (accumulateImpulse)
            {
                // Clamp the accumulated impulse
                float Pn0 = AccumulatedNormalImpulse;
                AccumulatedNormalImpulse = Math.Max(Pn0 + dPn, 0.0f);
                dPn = AccumulatedNormalImpulse - Pn0;
            }
            else
            {
                dPn = Math.Max(dPn, 0.0f);
            }

            // Apply contact impulse
            Vector2D normalImpulse = dPn * Normal;
            ApplyImpulse(normalImpulse);

            //#######################################################################################

            // Relative velocity at contact after NormalImpulse
            v1 = B1.Velocity + new Vector2D(-B1.AngularVelocity * r1.Y, B1.AngularVelocity * r1.X);
            v2 = B2.Velocity + new Vector2D(-B2.AngularVelocity * r2.Y, B2.AngularVelocity * r2.X);
            relativeVelocity = v2 - v1;

            Vector2D tangent = Vector2D.CrossWithZ(Normal, 1.0f);
            float velocityInTangent = relativeVelocity * tangent;
            float dPt = massTangent * -velocityInTangent;

            float friction = Math.Max(B1.Friction, B2.Friction);

            if (accumulateImpulse)
            {
                // Compute friction impulse
                //float maxPt = friction * AccumulatedNormalImpulse; //https://github.com/erincatto/box2d-lite/blob/master/src/Arbiter.cpp Line 175
                float maxPt = this.maxFrictionImpulse;

                // Clamp friction
                float oldTangentImpulse = AccumulatedTangentImpulse;
                AccumulatedTangentImpulse = MathHelp.Clamp(oldTangentImpulse + dPt, -maxPt, maxPt);
                dPt = AccumulatedTangentImpulse - oldTangentImpulse;
            }
            else
            {
                float maxPt = friction * dPn;
                dPt = MathHelp.Clamp(dPt, -maxPt, maxPt);
            }

            // Apply contact impulse
            Vector2D tangentImpulse = dPt * tangent;
            ApplyImpulse(tangentImpulse);
        }

        private void ApplyImpulse(Vector2D impulse)
        {
            B1.Velocity -= impulse * B1.InverseMass;
            B1.AngularVelocity -= Vector2D.ZValueFromCross(r1, impulse) * B1.InverseInertia;

            B2.Velocity += impulse * B2.InverseMass;
            B2.AngularVelocity += Vector2D.ZValueFromCross(r2, impulse) * B2.InverseInertia;
        }
    }
}
