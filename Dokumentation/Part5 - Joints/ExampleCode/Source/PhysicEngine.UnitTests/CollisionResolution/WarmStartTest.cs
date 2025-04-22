using FluentAssertions;
using PhysicEngine.UnitTests.TestHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PhysicEngine.UnitTests.CollisionResolution
{
    //Prüft, dass der Einsatz von Warmstart genauere Lösungen erzeugt als ohne
    public class WarmStartTest
	{
		private static string TestData = @"..\..\..\..\..\Data\CollisionResolutionTestcases\5_PGSConvergence\";
		private static float TimeStepTickRate = 50; //[ms]

		[Fact]
		public void CubeStack_WithWarmStart_TheTowerRemainsStanding()
		{
			var result = SimulateSeveralTimeSteps.DoTest(TestData + "CubeStack.txt", TimeStepTickRate, true, 10, 100, new SimulateSeveralTimeSteps.ExtraSettings() { DoPositionCorrection = true, DoWarmStart = true });

			result.TimeSteps.Last().Bodies.Select(x => x.Position.Y).ToArray().Should().BeInDescendingOrder(); //Prüfe das der Turm nicht umgefallen ist
		}

		[Fact]
		public void CubeStack_WithoutWarmStart_TheTowerFallsDown()
		{
			var result = SimulateSeveralTimeSteps.DoTest(TestData + "CubeStack.txt", TimeStepTickRate, true, 10, 100, new SimulateSeveralTimeSteps.ExtraSettings() { DoPositionCorrection = true, DoWarmStart = false });

			result.TimeSteps.Last().Bodies.Select(x => x.Position.Y).ToArray().Should().NotBeInDescendingOrder(); //Prüfe das der Turm umgefallen ist
		}

		[Fact]
		public void HeaveBallStack_WithWarmStart_TheTowerRemainsStanding()
		{
			int smallBall = 1;
			var result = SimulateSeveralTimeSteps.DoTest(TestData + "HeaveBallStack.txt", TimeStepTickRate, true, 10, 500, new SimulateSeveralTimeSteps.ExtraSettings() { DoPositionCorrection = true, DoWarmStart = true });

			result.TimeSteps.Last().Bodies.Select(x => x.Position.Y).ToArray().Should().BeInDescendingOrder(); //Prüfe das der Turm nicht umgefallen ist
			float yRange = Math.Abs(result.TimeSteps.First().Bodies[smallBall].Position.Y - result.TimeSteps.Last().Bodies[smallBall].Position.Y);
			yRange.Should().BeLessThan(1); //Der kleine Ball hat sich weniger als 1 Pixel bewegt
        }

		[Fact]
		public void HeaveBallStack_WithoutWarmStart_TheLittleBallIsPressedIntoTheGround()
		{
            int smallBall = 1;
            var result = SimulateSeveralTimeSteps.DoTest(TestData + "HeaveBallStack.txt", TimeStepTickRate, true, 10, 500, new SimulateSeveralTimeSteps.ExtraSettings() { DoPositionCorrection = true, DoWarmStart = false });

            result.TimeSteps.Last().Bodies.Select(x => x.Position.Y).ToArray().Should().BeInDescendingOrder(); //Prüfe das der Turm nicht umgefallen ist
            float yRange = Math.Abs(result.TimeSteps.First().Bodies[smallBall].Position.Y - result.TimeSteps.Last().Bodies[smallBall].Position.Y);
            yRange.Should().BeGreaterThan(1); //Der kleine Ball hat sich mehr als 1 Pixel bewegt
        }
	}
}
