using FluentAssertions;
using PhysicEngine.UnitTests.TestHelper;
using Xunit;

namespace PhysicEngine.UnitTests.Constraints.Joints
{
    //Hier soll werden gelenke nach Simulationsstart erst bewegt und dann wird kurz gewartet bis sich alles beruhigt hat.
    //Erwartung: Alle Körper befinden sich an der erwarteten Position
    public class MoveJointsLinearTest
    {
        private static string TestData = @"..\..\..\..\..\Data\JointsTestcases\";
        private static float TimeStepTickRate = 50; //[ms]

        [Fact]
        public void AllJointsAreMoved_ToResultingBodyPositionMatchWithExpectedPositions()
        {
            float maxDiff = JointSimulator.SimulateAndCompare(TestData + "AllJointsStart.txt", TestData + "AllJointsEnd.txt", TimeStepTickRate, 100, 20,
                new JointSimulator.JointSetpoint[]
                {
                    new JointSimulator.JointSetpoint(){JointIndex = 5, SetValue = 0.14f},
                    new JointSimulator.JointSetpoint(){JointIndex = 4, SetValue = 0.26f},
                    new JointSimulator.JointSetpoint(){JointIndex = 8, SetValue = 0.62f},
                });

            maxDiff.Should().BeLessThan(5);
        }

        [Fact]
        public void AllJointsStiffAreMoved_ToResultingBodyPositionMatchWithExpectedPositions()
        {
            float maxDiff = JointSimulator.SimulateAndCompare(TestData + "AllJointsStiffStart.txt", TestData + "AllJointsStiffEnd.txt", TimeStepTickRate, 100, 20,
                new JointSimulator.JointSetpoint[]
                {
                    new JointSimulator.JointSetpoint(){JointIndex = 5, SetValue = 0.92f},
                    new JointSimulator.JointSetpoint(){JointIndex = 4, SetValue = 0.91f},
                    new JointSimulator.JointSetpoint(){JointIndex = 8, SetValue = 0.50f},
                });

            maxDiff.Should().BeLessThan(5);
        }
    }
}
