using FluentAssertions;
using Xunit;

namespace PhysicEngine.UnitTests.CollisionResolution
{
    //Testfälle wo Objekte mit Restiution=1 springen. Erwartung: Das Objekt springt unendlich lange und erreicht exakt
    //immer die gleiche Ausgangshöhe (Es kommt keine Höhe hinzu/es verliert keine Höhe)
    //Besonderheit: Man darf hier keine PositionCorrection verwenden, weil sonst dem Objekt mehr Höhe gegeben wird
    public class JumpingTests
    {
        private static string TestData = @"..\..\..\..\..\Data\CollisionResolutionTestcases\4_Jumping\";
        private static float TimeStepTickRate = 50; //[ms]

        [Fact]
        public void JumpingBall_RestitionIsOne_JumpsInfinity()
        {
            int circle = 3;
            var result = SimulateSeveralTimeSteps.DoTest(TestData + "JumpingBall.txt", TimeStepTickRate, true, 10, 200);

            int[] maxYValues = SimulateSeveralTimeSteps
                .GetMaxFloatValuesFromSingleBody(result.TimeSteps, circle, (b)=>b.Position.Y)
                .Select(x => (int)Math.Round(x))
                .ToArray();

            int[] minYValues = SimulateSeveralTimeSteps
                .GetMinFloatValuesFromSingleBody(result.TimeSteps, circle, (b) => b.Position.Y)
                .Select(x => (int)Math.Round(x))
                .ToArray();

            (maxYValues.Max() - maxYValues.Min()).Should().Be(0); //Prüfe dass alle Max-Werte gleich sind
            (minYValues.Max() - minYValues.Min()).Should().Be(0); //Prüfe dass alle Min-Werte gleich sind
        }

        [Fact]
        public void JumpingCube45_RestitionIsOne_JumpsInfinity()
        {
            int cube = 1;
            var result = SimulateSeveralTimeSteps.DoTest(TestData + "JumpingCube45.txt", TimeStepTickRate, true, 10, 200);

            int[] maxYValues = SimulateSeveralTimeSteps
                .GetMaxFloatValuesFromSingleBody(result.TimeSteps, cube, (b) => b.Position.Y)
                .Select(x => (int)Math.Round(x))
                .ToArray();

            int[] minYValues = SimulateSeveralTimeSteps
                .GetMinFloatValuesFromSingleBody(result.TimeSteps, cube, (b) => b.Position.Y)
                .Select(x => (int)Math.Round(x))
                .ToArray();

            (maxYValues.Max() - maxYValues.Min()).Should().Be(0); //Prüfe dass alle Max-Werte gleich sind
            (minYValues.Max() - minYValues.Min()).Should().Be(0); //Prüfe dass alle Min-Werte gleich sind
        }

        [Fact]
        public void JumpingCubes_FallingDown_BecomesCalm()
        {
            var result = SimulateSeveralTimeSteps.DoTest(TestData + "JumpingCubes.txt", TimeStepTickRate, true, 100, 500);

            //Gibt zurück, wie viele TimeSteps der letzte zur Ruhe kommende Körper braucht, um die Ruhelage zu erreichen
            int maxCalm = SimulateSeveralTimeSteps.GetTimeStepCountForGettingCalmForEachBody(result.TimeSteps).Max();

            maxCalm.Should().BeLessThanOrEqualTo(265); //Spätestens nach 265 TimeSteps soll jeder Körper in Ruhelage sein
        }
		
		//3 Würfel fallen nach unten. Zwischen Würfel 1 und 2 gibt es Kollisionspunkte
        //Erwarunt: Alle Würfel fallen gleich schnell
        //Hier wird geprüft, dass die externe Kraft nicht gleichzeitig von der PhysicScene-TimeStep-Methode 
        //und dem ImpulseResolver angewendet wird. Pro TimeStep darf die Schwerkraft nur einmal pro Körper wirken.
        [Fact]
        public void ThreeCubesFalling_AllThreeCubesFallsWithSameSpeed()
        {
            var result = SimulateSeveralTimeSteps.DoTest(TestData + "ThreeCubesFalling.txt", TimeStepTickRate, true, 10, 50, new SimulateSeveralTimeSteps.ExtraSettings() {  DoPositionCorrection = false});

            var velocitys = result.TimeSteps.Last().Bodies.Select(x => x.Velocity.Y).ToList();

            //Prüfe das alle 3 Würfel gleich schnell sind
            velocitys.Should().AllBeEquivalentTo(velocitys[0]);
        }
    }
}
