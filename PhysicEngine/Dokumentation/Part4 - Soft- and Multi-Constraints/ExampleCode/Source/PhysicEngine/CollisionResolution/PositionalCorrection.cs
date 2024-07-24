using GraphicMinimal;
using PhysicEngine.CollisionDetection;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution
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
            Vector2D correctionAmount = c.Normal * f;
            c.B1.MoveCenter(-correctionAmount * c.B1.InverseMass);
            c.B2.MoveCenter(correctionAmount * c.B2.InverseMass);
        }

        //Ich benötige hier eine eigene Collision-Klasse, da bei der RigidBodyCollision der Start- und End-Punkt
        //sich nicht verschiebt, wenn ich die Body-Positition ändere und ich somit beim iterieren über alle
        //Collision-Punkte sonst nicht weiß, was der "echte" Abstand der Kollisionspunkte ist
        class CollisionWithLeverArm : RigidBodyCollision
        {
            private Vector2D r1;
            private Vector2D r2;
            public CollisionWithLeverArm(RigidBodyCollision c) : base(c)
            {
                this.r1 = c.End - c.B1.Center;
                this.r2 = c.Start - c.B2.Center;
            }

            public float GetDepth()
            {
                Vector2D p1 = this.B1.Center + this.r1;
                Vector2D p2 = this.B2.Center + this.r2;
                return (p2 - p1).Length();
            }
        }

        public static void CreateCalmRestingContacts(List<IRigidBody> bodies, float allowedPenetration)
        {
            int maxTrys = 100;

            for (int i = 0; i < maxTrys; i++)
            {
                var collisions = CollisionHelper
                    .GetAllCollisions(bodies)
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
