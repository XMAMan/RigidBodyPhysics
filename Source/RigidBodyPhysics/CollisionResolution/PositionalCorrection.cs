using PhysicGlobal;
using RigidBodyPhysics.CollisionDetection;

namespace RigidBodyPhysics.CollisionResolution
{
    internal class PositionalCorrection
    {
        //Wenn die Kontaktpunkte ein größeren Abstand als allowedPenetration haben, dann werden sie so weit zusammen geschoben
        //bis der Abstand allowedPenetration ist (Wenn man bei posCorrectionRate=1 einstellt). Wenn man posCorrectionRate< 1 einstellt,
        //dann wird entsprechend weniger Prozent von der Korrektur ausgeführt
        //Diese Funktion ist nötig, um Körper, die initial mit großer Überlappung (aber ohne Geschwindigkeit) erzeugt wurden
        //so weit auseinander zu schieben, dass daraus dann ein ruhiger Resting-Kontaktpunkt wird
        //Return: true=es wurde was korrigiert
        private static void DoCorrection(CollisionWithLeverArm c, float posCorrectionRate, float allowedPenetration)
        {
            float f = (Math.Max(0, c.GetDepth() - allowedPenetration) / (c.B1.InverseMass + c.B2.InverseMass) * posCorrectionRate);
            Vec2D correctionAmount = c.Normal * f;

            Vec2D newPosition1 = c.B1.Center - correctionAmount * c.B1.InverseMass;
            Vec2D newPosition2 = c.B2.Center + correctionAmount * c.B2.InverseMass;
            c.B1.MoveTo(newPosition1, c.B1.Angle);
            c.B2.MoveTo(newPosition2, c.B2.Angle);
        }

        //Ich benötige hier eine eigene Collision-Klasse, da bei der RigidBodyCollision der Start- und End-Punkt
        //sich nicht verschiebt, wenn ich die Body-Positition ändere und ich somit beim iterieren über alle
        //Collision-Punkte sonst nicht weiß, was der "echte" Abstand der Kollisionspunkte ist
        class CollisionWithLeverArm : RigidBodyCollision
        {
            private Vec2D r1;
            private Vec2D r2;
            internal CollisionWithLeverArm(RigidBodyCollision c) : base(c)
            {
                this.r1 = c.End - c.B1.Center;
                this.r2 = c.Start - c.B2.Center;
            }

            internal float GetDepth()
            {
                Vec2D p1 = this.B1.Center + this.r1;
                Vec2D p2 = this.B2.Center + this.r2;
                return (p2 - p1).Length();
            }
        }

        internal static void CreateCalmRestingContacts(CollisionManager collisionManager, float allowedPenetration)
        {
            int maxTrys = 100;

            for (int i = 0; i < maxTrys; i++)
            {
                var collisions = collisionManager.GetAllCollisions()
                    .Select(x => new CollisionWithLeverArm(x))
                    .ToArray();

                if (collisions.Any() == false) return;


                foreach (var collision in collisions)
                {
                    DoCorrection(collision, 0.5f, allowedPenetration / 2);
                }


                if (collisions.All(x => x.GetDepth() < allowedPenetration)) return;
            }
        }
    }
}
