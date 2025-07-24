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

        //Hier wird geprüft, dass "new PhysicScene()+ N mal Aufruf von TimeStep" das gleich erzeugt
        //wie "scene.ResetPosition(initialState) + N mal Aufruf von TimeStep"
        [Fact]
        public void PhysicSceneReset_ShowsSameAsNewObject()
        {
            var sceneData = ExportHelper.ReadFromFile(TestData + "AllJointsStart.txt");
            var scene = new PhysicScene(sceneData);
            var initialState1 = scene.GetExportData();

            int phase1Steps = 1;
            var phase2Steps = 0;

            //Run 1: scene wurde neu über Konstruktur erstellt
            float maxDiff1 = JointSimulator.SimulateAndCompare(scene, TestData + "AllJointsEnd.txt", TimeStepTickRate, phase1Steps, phase2Steps,
                new JointSimulator.JointSetpoint[]
                {
                    new JointSimulator.JointSetpoint(){JointIndex = 5, SetValue = 0.18f}, //Prismatic Joint
                    new JointSimulator.JointSetpoint(){JointIndex = 4, SetValue = 0.24f}, //Revolute Joint
                    new JointSimulator.JointSetpoint(){JointIndex = 8, SetValue = 214},   //Distance Joint
                });
            var endState1 = scene.GetExportData();

            //Run 2: scene wurde über ResetPosition zurück gesetzt
            scene.ResetPosition(initialState1);
            var initialState2 = scene.GetExportData();

            string s1 = JsonHelper.Helper.ToCompactJson(initialState1);
            string s2 = JsonHelper.Helper.ToCompactJson(initialState2);
            s1.Should().Be(s2);

            float maxDiff2 = JointSimulator.SimulateAndCompare(scene, TestData + "AllJointsEnd.txt", TimeStepTickRate, phase1Steps, phase2Steps,
                new JointSimulator.JointSetpoint[]
                {
                    new JointSimulator.JointSetpoint(){JointIndex = 5, SetValue = 0.18f}, //Prismatic Joint
                    new JointSimulator.JointSetpoint(){JointIndex = 4, SetValue = 0.24f}, //Revolute Joint
                    new JointSimulator.JointSetpoint(){JointIndex = 8, SetValue = 214},   //Distance Joint
                });
            var endState2 = scene.GetExportData();

            string s3 = JsonHelper.Helper.ToCompactJson(endState1);
            string s4 = JsonHelper.Helper.ToCompactJson(endState2);

            s3.Should().Be(s4);
        }
    }
}
