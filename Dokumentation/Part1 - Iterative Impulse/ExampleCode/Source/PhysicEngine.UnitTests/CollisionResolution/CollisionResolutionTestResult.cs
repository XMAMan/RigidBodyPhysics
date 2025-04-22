using GraphicMinimal;

namespace PhysicEngine.UnitTests.CollisionResolution
{
    internal class CollisionResolutionTestResult
    {
        public class Body
        {
            public Vector2D Velocity;
            public float AngularVelocity;
            public Vector2D Position;

            public override string ToString()
            {
                return this.Velocity.ToString() + " " + this.AngularVelocity.ToString();
            }
        }

        public Body[] BodysBeforeCollision = null;
        public Body[] BodysAfterCollision = null;

        //Ermittelt beim ersten Time-Step erstmal wie viele Kollisionspunkte es gibt
        //Dann ruft es in einer Schleife immer wieder die Time-Step-Funktion auf und schaut, ob sich die Kollisionspunkte-Anzahl gegenüber dem
        //letzten Schritt erhöht hat. Wenn ja, ist dass dann der gesuchte Zeitpunkt, wo untersucht wird, wie die Geschwindigkeiten vor and nach
        //der Kollisionsauflösung aussehen
        public static CollisionResolutionTestResult DoTest(string bodysFilePath, float timeStepTickRate, int maxTrysToFindAnCollision, int maxIterionsForImpulseResolution = 15)
        {
            CollisionResolutionTestResult result = new CollisionResolutionTestResult();

            var bodys = JsonHelper.ReadFromFile(bodysFilePath);

            result.BodysBeforeCollision = new Body[bodys.Count];
            result.BodysAfterCollision = new Body[bodys.Count];

            var scene = new PhysicScene(bodys);
            scene.MaxIterionsForImpulseResolution = maxIterionsForImpulseResolution;
            bool collsionOccured = false;

            List<int> collisionCounts = new List<int>();
            scene.CollisonOccured += (s, collisions) =>
            {
                int lastContactCount = collisionCounts.Count > 0 ? collisionCounts.Last() : -1; 
                collisionCounts.Add(collisions.Length);

                //Wenn lastContactCount == -1 ist, dann sind wir im ersten TimeStep-Aufruf und dort wollen wir noch nicht prüfen, ob sich die Kollisionsanzahl veränder hat
                if (collisions.Length > lastContactCount && lastContactCount != -1)
                {
                    collsionOccured = true;
                    for (int i = 0; i < bodys.Count; i++)
                    {
                        result.BodysBeforeCollision[i] = new Body()
                        {
                            Velocity = bodys[i].Velocity,
                            AngularVelocity = bodys[i].AngularVelocity,
                            Position = bodys[i].Center
                        };
                    }
                }
            };

            int j;
            for (j=0;j<maxTrysToFindAnCollision && collsionOccured == false; j++)
            //while (collsionOccured == false)
            {
                int countBefore = collisionCounts.Count;
                scene.TimeStep(timeStepTickRate);

                //Es passierte keine Kollision
                if (collisionCounts.Count == countBefore)
                {
                    collisionCounts.Add(0);
                }
            }
                
            if (j == maxTrysToFindAnCollision) throw new Exception("There was no collision");

            for (int i = 0; i < bodys.Count; i++)
            {
                result.BodysAfterCollision[i] = new Body()
                {
                    Velocity = bodys[i].Velocity,
                    AngularVelocity = bodys[i].AngularVelocity,
                    Position = bodys[i].Center
                };
            }

            return result;
        }
    }
}
