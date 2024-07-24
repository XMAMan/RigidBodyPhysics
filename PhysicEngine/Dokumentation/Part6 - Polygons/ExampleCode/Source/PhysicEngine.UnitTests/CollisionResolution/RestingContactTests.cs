using FluentAssertions;
using PhysicEngine.UnitTests.TestHelper;
using System.Text;
using Xunit;

namespace PhysicEngine.UnitTests.CollisionResolution
{
    //Testfälle wo ein Objekt auf ein anderen ruhig liegt -> Erwartung: Objekt bleibt ruhig liegen
    //Hier ist kein PositionCorrection nötig, da immer nur ein Objekt auf ein anderne liegt (Keine Türme)
    public class RestingContactTests
    {
        private static string TestData = @"..\..\..\..\..\Data\CollisionResolutionTestcases\2_RestingContact\";
        private static float TimeStepTickRate = 50; //[ms]

        //Ein Ball startet ruhig auf ein Tisch liegend. Erwartung: Er bleibt ruhig liegen
        [Fact]
        public void RestingBall_StartsSittingOnTable_BallIsResting()
        {
            int circle = 3;
            var result = SimulateSeveralTimeSteps.DoTest(TestData + "RestingBall.txt", TimeStepTickRate, false, 100, 50);
            var pos = result.TimeSteps.Select(x => x.Bodies[circle].Position.Y).ToList();
            float ground = 680 - (150.92f / 2);
            string posString = string.Join("\r\n", pos.Select(x => ground - x));

            float range = pos.Max() - pos.Min();
            range.Should().Be(0); //Der Ball wackelt nicht innerhalb der Y-Achse
        }

        //Um das Problem des Springenden Balls mit Restition=1 zu verstehen will ich hier mit Integerzahlen das ganze nachstellen
        //Ziel ist ein Weg zu finden, wie ich ein Ball unendlich lange springen lassen kann ohne dass seine Maxhöhe sich ändert
        //Ansatz 1: Erst wirkt eine externe Kraft und danach wird dann geschaut, ob die Geschwindigkeit ok ist -> Dieser Ansatz ist falsch
        [Fact]
        public void JumpingBall1_SemiImplicitEuler_JumpHeightDoesNotIncrease()
        {
            int height  = 3; //Groundlevel = 0; > 0 = Above Ground; <=0 Under Ground
            int velocity = 0; //Velocity in Y-Direction
            int gravity = -1; //Gravity in Y-Direction

            StringBuilder log = new StringBuilder();
            for (int timeStep = 0;timeStep < 30;timeStep++)
            {
                log.AppendLine(timeStep + ":" + height + "\t" + velocity);

                //1. Apply Gravity
                velocity += gravity;

                //2. Apply Impulse if Ball is falling down and under the ground
                if (height <= 0 && velocity < 0)
                    velocity = -velocity;

                //3. Move
                height += velocity;
            }

            string result = log.ToString();
        }

        //Ansatz 2: Es wirkt die Schwerkraft. Sollte der Ball im Tisch sein, wirkt außerdem die Abstoßkraft -> Das ist der richtige Ansatz
        [Fact]
        public void JumpingBall2_SemiImplicitEuler_JumpHeightDoesNotIncrease()
        {
            int height = 3; //Groundlevel = 0; > 0 = Above Ground; <=0 Under Ground
            int velocity = 0; //Velocity in Y-Direction
            int gravity = -1; //Gravity in Y-Direction

            StringBuilder log = new StringBuilder();
            for (int timeStep = 0; timeStep < 30; timeStep++)
            {
                log.AppendLine(timeStep + ":" + height + "\t" + velocity);

                //1. Apply Force
                if (height <= 0 && velocity < 0)
                    velocity = -velocity;   //Ball im Tisch: Lasse Normal-Impuls wirken
                else
                    velocity += gravity;    //Ball über dem Tishc: Lasse Gravitation wirken

                //2. Move
                height += velocity;
            }

            string result = log.ToString();
        }

        //Ein Würfel mit Restituion=0.99 wird über einen Tisch fallen gelassen. Erwartung: Er prall ein paar mal ab und kommt dann zur Ruhe
        //Hinweis: Wenn man eine PGS-Iteration von 10 anstatt 100 verwendet und eine Friction-Constraint verwendet, dann fängt der Würfel
        //nach der Zeit wegen PGS-Genauigkeitsproblemen sich zu drehen an, weil die Friction-Kraft ihn seitlich drückt
        [Fact]
        public void RestingCube_FallingDownOnTable_CubeBecomesResting()
        {
            int cube = 3;
            var result = SimulateSeveralTimeSteps.DoTest(TestData + "RestingCube.txt", TimeStepTickRate, false, 100, 900);

            var pos = result.TimeSteps.Select(x => x.Bodies[cube].Position.Y).ToArray();

            float range = SimulateSeveralTimeSteps.GetMinMaxRangeFromLastNEntrys(pos, 30); //Innerhalb der letzten 30 TimeSteps erwarte ich, dass der Würfel ruhig liegt
            range.Should().BeApproximately(0, 0.01f); //Der Würfel wackelt nicht innerhalb der Y-Achse nachdem er zur Ruhe gekommen ist
        }

        //Ein Würfel mit Restituion=0.9 startet auf der Spitze stehend auf ein Tisch. Er kippt dann um -> Erwartung: Er kommt nach ein paar Wacklern zur Ruhe
        [Fact]
        public void CippingCube_StartsWithCornerOnTable_CubeBecomesResting()
        {
            int cube = 1;
            var result = SimulateSeveralTimeSteps.DoTest(TestData + "CippingCube.txt", TimeStepTickRate, false, 50, 500);

            var pos = result.TimeSteps.Select(x => x.Bodies[cube].Position.Y).ToArray();

            float range = SimulateSeveralTimeSteps.GetMinMaxRangeFromLastNEntrys(pos, 30); //Innerhalb der letzten 30 TimeSteps erwarte ich, dass der Würfel ruhig liegt
            range.Should().Be(0); //Der Würfel wackelt nicht innerhalb der Y-Achse nachdem er zur Ruhe gekommen ist
        }

        [Fact]
        public void SlidingCubes_DifferentFriction_CubesSlidesWithDifferentSpeed()
        {
            var result = SimulateSeveralTimeSteps.DoTest(TestData + "Friction.txt", TimeStepTickRate, true, 100, 27);

            List<float> lenghts = new List<float>();
            for (int i=1;i<result.BodyCount;i++)
            {
                var v = result.TimeSteps.Select(x => x.Bodies[i]).Select(x => x.Velocity).ToList();
                v = v.GetRange(3, v.Count - 3); //Überspringe die ersten 3 TimeSteps da dort die Geschwindigkeit, weil da die Geschwindigkeit noch so gering ist, dass die noramlisierte Länge anders ist, als vom ganzen Rest
                v.Should().AllSatisfy((x) => (x.Normalize() - v[0].Normalize()).Length().Should().BeLessThan(0.3f)); //Alle Geschwindigkeitswerte sollen alle in die gleiche Richtung zeigen
                v.Select(x => x.Length()).Should().BeInAscendingOrder(); //Mit jeden Timestep soll die Geschwindigkeit ansteigen
                lenghts.Add(v.Last().Length());
            }

            lenghts.Should().BeInAscendingOrder(); //Um so kleiner der Friction-Wert um so schneller soll die Geschwindigkeit sein
        }

        //Ein Würfel mit ein kleinen initialen Schwung liegt auf ein Tisch. Erwartung ist dass der Würfel schnell zur Ruhe kommt
        //Hier wird gezeigt, dass wenn man mit PositionCorrection arbeitet der AllowedPenetration-Wert nicht zu klein sein darf,
        //da der Würfel sonst immer wieder aus dem Tisch rausgedrückt wird, was den Kontakt dann lößt, was somit verhindert,
        //dass aus ein Colliding-Contact ein Resting-Contact wird
        [Fact]
        public void CubeOnTable_StartsWithMomentum_BecomesQuickResting()
        {
            int cube = 1;

            //Würfel wird mit AllowedPenetration=0.1 unter Nutzung von PositionCorrection simuliert -> Hier kommt der Würfel nicht zur Ruhe
            var angle_0_1 = SimulateSeveralTimeSteps
                .DoTest(TestData + "CubeOnTableWithMomentum.txt", TimeStepTickRate, true, 100, 100, new SimulateSeveralTimeSteps.ExtraSettings()
                { DoPositionCorrection = true, AllowedPenetration = 0.1f })
                .TimeSteps.Select(x => x.Bodies[cube].Angle + 1)
                .ToArray();

            //Würfel wird mit AllowedPenetration=1.0 unter Nutzung von PositionCorrection simuliert -> Hier kommt der Würfel zur Ruhe
            var angle_1_0 = SimulateSeveralTimeSteps
                .DoTest(TestData + "CubeOnTableWithMomentum.txt", TimeStepTickRate, true, 100, 100, new SimulateSeveralTimeSteps.ExtraSettings()
                { DoPositionCorrection = true, AllowedPenetration = 2.0f })
                .TimeSteps.Select(x => x.Bodies[cube].Angle + 1)
                .ToArray();

            string angle01 = string.Join("\n", angle_0_1);
            string angle10 = string.Join("\n", angle_1_0);

            float range = SimulateSeveralTimeSteps.GetMinMaxRangeFromLastNEntrys(angle_1_0, 90); //Innerhalb der letzten 90 TimeSteps erwarte ich, dass der Würfel ruhig liegt
            range.Should().BeApproximately(0, 0.000001f); //Die Ausrichtung ändert sich nach 10 TimeSteps nicht mehr
        }
    }
}
