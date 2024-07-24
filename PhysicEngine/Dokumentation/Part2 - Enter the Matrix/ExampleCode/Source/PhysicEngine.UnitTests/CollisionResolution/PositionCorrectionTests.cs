using FluentAssertions;
using Xunit;

namespace PhysicEngine.UnitTests.CollisionResolution
{
    //Testfälle wo Objekte zu Boden fallen -> Erwartung: Sie kommen dann am Boden zur Ruhe und überlappen sich nicht
    //Besonderheit: Weil die Objekte übereinander liegen, ist PositionCorrection nötig
    public class PositionCorrectionTests
    {
        private static string TestData = @"..\..\..\..\..\Data\CollisionResolutionTestcases\3_PositionCorrection\";
        private static float TimeStepTickRate = 50; //[ms]

        [Fact]
        public void CircleStack_FallingDown_BecomeCalm()
        {
            var result = SimulateSeveralTimeSteps.DoTest(TestData + "CircleStack.txt", TimeStepTickRate, true, 100, 50, new SimulateSeveralTimeSteps.ExtraSettings() { DoPositionCorrection = true });
            
            //Gibt zurück, wie viele TimeSteps der letzte zur Ruhe kommende Körper braucht, um die Ruhelage zu erreichen
            int maxCalm = SimulateSeveralTimeSteps.GetTimeStepCountForGettingCalmForEachBody(result.TimeSteps).Max();

            //PGS-ItertionCount=10  -> [0]=0 [1]=26	[2]=29 [3]=27 [4]=30 
            //PGS-ItertionCount=100 -> [0]=0 [1]=21 [2]=23 [3]=26 [4]=27 -> Um so mehr PGS-Iterationen, um so schneller kommt das System zur Ruhe

            maxCalm.Should().BeLessThanOrEqualTo(46); //Spätestens nach 46 TimeSteps soll jeder Körper in Ruhelage sein

            result.TimeSteps.Last().Bodies.Select(x => x.Position.Y).ToArray().Should().BeInDescendingOrder(); //Prüfe das der Turm nicht umgefallen ist
        }

        [Fact]
        public void CubeStack_FallingDown_BecomeCalm()
        {
            var result = SimulateSeveralTimeSteps.DoTest(TestData + "CubeStack.txt", TimeStepTickRate, true, 300, 50, new SimulateSeveralTimeSteps.ExtraSettings() { DoPositionCorrection = true });

            //Gibt zurück, wie viele TimeSteps der letzte zur Ruhe kommende Körper braucht, um die Ruhelage zu erreichen
            int maxCalm = SimulateSeveralTimeSteps.GetTimeStepCountForGettingCalmForEachBody(result.TimeSteps).Max(); //PGS-ItertionCount=100 -> [0]=0 [1]=23 [2]=29 [3]=29 [4]=30 [5]=49

            maxCalm.Should().BeLessThanOrEqualTo(50); //Spätestens nach 50 TimeSteps soll jeder Körper in Ruhelage sein

            result.TimeSteps.Last().Bodies.Select(x => x.Position.Y).ToArray().Should().BeInDescendingOrder(); //Prüfe das der Turm nicht umgefallen ist
        }

        [Fact]
        public void FunnelBalls_FallingDown_BecomeCalm()
        {
            var result = SimulateSeveralTimeSteps.DoTest(TestData + "FunnelBalls.txt", TimeStepTickRate, true, 100, 200, new SimulateSeveralTimeSteps.ExtraSettings() { DoPositionCorrection = true });

            //Gibt zurück, wie viele TimeSteps der letzte zur Ruhe kommende Körper braucht, um die Ruhelage zu erreichen
            int maxCalm = SimulateSeveralTimeSteps.GetTimeStepCountForGettingCalmForEachBody(result.TimeSteps).Max(); //PGS-ItertionCount=100 ->[0]=0 [1]=0 [2]=44 [3]=56 [4]=65 [5]=131 [6]=112

            maxCalm.Should().BeLessThanOrEqualTo(131); //Spätestens nach 131 TimeSteps soll jeder Körper in Ruhelage sein
        }

        //Dieser Test braucht bei PGS-Iterations=100 14.8 Sekunden weil die Matrix, die hier gebaut wird so groß ist
        //D.h. es ist wichtig die Sparse-Density-Eigenschaft der Matrix zu nutzen, um PGS zu optimieren
        [Fact]
        public void Pyramid_FallingDown_BecomeCalm()
        {
            var result = SimulateSeveralTimeSteps.DoTest(TestData + "Pyramid.txt", TimeStepTickRate, true, 100, 200, new SimulateSeveralTimeSteps.ExtraSettings() { DoPositionCorrection = true });

            //Gibt zurück, wie viele TimeSteps der letzte zur Ruhe kommende Körper braucht, um die Ruhelage zu erreichen
            int maxCalm = SimulateSeveralTimeSteps.GetTimeStepCountForGettingCalmForEachBody(result.TimeSteps).Max(); //PGS-ItertionCount=100 ->[0]=0 [1]=0 [2]=0 [3]=0 [4]=0 [5]=0 [6]=0 [7]=0 [8]=0 [9]=0 [10=137 [11=0 [12=69 [13=55 [14=171 [15]=0

            maxCalm.Should().BeLessThanOrEqualTo(171); //Spätestens nach 171 TimeSteps soll jeder Körper in Ruhelage sein
        }
    }
}
