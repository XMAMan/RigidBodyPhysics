using GraphicMinimal;
using PhysicEngine.RigidBody;

namespace PhysicEngine.UnitTests.CollisionResolution
{
    //Spult so lange die Zeit vor bis eine Kollision passierte. Es merkt sich dann die Geschwindigkeitswerte vor und nach der Kollision
    internal class NoGravityTestHelper
    {
        public class Body
        {
            public Vector2D Velocity;
            public float AngularVelocity;
            public Vector2D Position;

            public Body(IRigidBody body)
            {
                int roundDecimalPlaces = 7;  //Runde 7 Stellen nach dem Komma damit der Float-Vergleich im UnitTest klappt
                this.Velocity = MathHelp.Round(body.Velocity, roundDecimalPlaces);
                this.AngularVelocity = MathHelp.Round(body.AngularVelocity, roundDecimalPlaces);
                this.Position = new Vector2D(body.Center);
            }

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
        public static NoGravityTestHelper DoTest(string bodysFilePath, float timeStepTickRate, int maxTrysToFindAnCollision, bool useGlobalSolver, int pgsIterations = 100, int extraTimeStepsAfterCollision = 0)
        {
            NoGravityTestHelper result = new NoGravityTestHelper();

            var bodys = ExportHelper.ReadFromFile(bodysFilePath);

            result.BodysBeforeCollision = new Body[bodys.Count];
            result.BodysAfterCollision = new Body[bodys.Count];

            var scene = new PhysicScene(bodys);
            if (useGlobalSolver) scene.UseGlobalSolver(); else scene.UseGroupSolver();
            scene.PushBodysApart(); //Erzeuge ruhige Resting-Kontaktpunkte
            scene.DoPositionalCorrection = false;
            scene.DoWarmStart = false;
            scene.HasGravity = false;
            scene.IterationCount = pgsIterations;
            bool collsionOccured = false;

            List<int> collisionCounts = new List<int>();
            scene.CollisonOccured += (s, collisions) =>
            {
                int lastContactCount = collisionCounts.Count > 0 ? collisionCounts.Last() : -1; 
                collisionCounts.Add(collisions.Length);

                //Wenn lastContactCount == -1 ist, dann sind wir im ersten TimeStep-Aufruf und dort wollen wir noch nicht prüfen, ob sich die Kollisionsanzahl veränder hat
                if (collsionOccured == false && collisions.Length > lastContactCount && lastContactCount != -1)
                {
                    collsionOccured = true;
                    for (int i = 0; i < bodys.Count; i++)
                    {
                        result.BodysBeforeCollision[i] = new Body(bodys[i]);                        
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

            for (int i=0;i< extraTimeStepsAfterCollision;i++) scene.TimeStep(timeStepTickRate);

            for (int i = 0; i < bodys.Count; i++)
            {
                result.BodysAfterCollision[i] = new Body(bodys[i]);
            }

            return result;
        }
    }
}
