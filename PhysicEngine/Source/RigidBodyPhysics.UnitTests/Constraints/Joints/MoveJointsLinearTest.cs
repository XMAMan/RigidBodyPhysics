using FluentAssertions;
using RigidBodyPhysics.UnitTests.TestHelper;
using Xunit;

namespace RigidBodyPhysics.UnitTests.Constraints.Joints
{
    //Hier werden Gelenke nach Simulationsstart erst bewegt und dann wird kurz gewartet bis sich alles beruhigt hat.
    //Erwartung: Alle Körper befinden sich an der erwarteten Position
    public class MoveJointsLinearTest
    {
        private static string TestData = @"..\..\..\..\..\Data\TestData\RigidBodyPhysicsTestData\JointsTestcases\AllJoints\";
        private static float TimeStepTickRate = 50; //[ms]

        [Fact]
        public void AllJointsAreMoved_ToResultingBodyPositionMatchWithExpectedPositions()
        {
            float maxDiff = JointSimulator.SimulateAndCompare(TestData + "AllJointsStart.txt", TestData + "AllJointsEnd.txt", TimeStepTickRate, 100, 20,
                new JointSimulator.JointSetpoint[]
                {
                    new JointSimulator.JointSetpoint(){JointIndex = 5, SetValue = 0.18f}, //Prismatic Joint
                    new JointSimulator.JointSetpoint(){JointIndex = 4, SetValue = 0.24f}, //Revolute Joint
                    new JointSimulator.JointSetpoint(){JointIndex = 8, SetValue = 214},   //Distance Joint
                });

            maxDiff.Should().BeLessThan(5);
        }

        [Fact]
        public void AllJointsStiffAreMoved_ToResultingBodyPositionMatchWithExpectedPositions()
        {
            float maxDiff = JointSimulator.SimulateAndCompare(TestData + "AllJointsStiffStart.txt", TestData + "AllJointsStiffEnd.txt", TimeStepTickRate, 100, 20,
                new JointSimulator.JointSetpoint[]
                {
                    new JointSimulator.JointSetpoint(){JointIndex = 5, SetValue = 0.81f}, //Prismatic Joint
                    new JointSimulator.JointSetpoint(){JointIndex = 4, SetValue = 0.93f}, //Revolute Joint
                    new JointSimulator.JointSetpoint(){JointIndex = 8, SetValue = 50},    //Distance Joint
                });

            maxDiff.Should().BeLessThan(5);
        }
    }
}
