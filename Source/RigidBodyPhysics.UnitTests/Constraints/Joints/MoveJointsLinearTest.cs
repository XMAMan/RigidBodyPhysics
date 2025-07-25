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
            float maxDiff = JointSimulator.SimulateAndCompare(TestData + "AllJointsStart.txt", TestData + "AllJointsEnd.txt", "Output.txt", TimeStepTickRate, 100, 20,
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
            float maxDiff = JointSimulator.SimulateAndCompare(TestData + "AllJointsStiffStart.txt", TestData + "AllJointsStiffEnd.txt", "OutputStiff.txt", TimeStepTickRate, 100, 20,
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

            int phase1Steps = 100;
            var phase2Steps = 20;

            //Run 1: scene wurde neu über Konstruktur erstellt
            float maxDiff1 = JointSimulator.SimulateAndCompare(scene, TestData + "AllJointsEnd.txt", "Output.txt", TimeStepTickRate, phase1Steps, phase2Steps,
                new JointSimulator.JointSetpoint[]
                {
                    new JointSimulator.JointSetpoint(){JointIndex = 5, SetValue = 0.18f}, //Prismatic Joint
                    new JointSimulator.JointSetpoint(){JointIndex = 4, SetValue = 0.24f}, //Revolute Joint
                    new JointSimulator.JointSetpoint(){JointIndex = 8, SetValue = 214},   //Distance Joint
                },
                (s) => 
                {
                    //Prüfe ab, dass beim Export/Neuerstellen immer das gleiche rauskommt
                    var export1 = s.GetExportData();
                    var export2 = new PhysicScene(export1).GetExportData();
                    string export1Text = JsonHelper.Helper.ToCompactJson(export1);
                    string export2Text = JsonHelper.Helper.ToCompactJson(export2);
                    if (export1Text != export2Text)
                    {
                        throw new Exception("Exporterror");
                    }
                });
            var endState1 = scene.GetExportData();

            //Run 2: scene wurde über ResetPosition zurück gesetzt
            scene.ResetPosition(initialState1);
            //scene = new PhysicScene(initialState1);
            var initialState2 = scene.GetExportData();

            string initStateText1 = JsonHelper.Helper.ToCompactJson(initialState1);
            string initStateText2 = JsonHelper.Helper.ToCompactJson(initialState2);
            initStateText1.Should().Be(initStateText2);

            float maxDiff2 = JointSimulator.SimulateAndCompare(scene, TestData + "AllJointsEnd.txt", "Output.txt", TimeStepTickRate, phase1Steps, phase2Steps,
                new JointSimulator.JointSetpoint[]
                {
                    new JointSimulator.JointSetpoint(){JointIndex = 5, SetValue = 0.18f}, //Prismatic Joint
                    new JointSimulator.JointSetpoint(){JointIndex = 4, SetValue = 0.24f}, //Revolute Joint
                    new JointSimulator.JointSetpoint(){JointIndex = 8, SetValue = 214},   //Distance Joint
                },
                (s) =>
                {
                    //Prüfe ab, dass beim Export/Neuerstellen immer das gleiche rauskommt
                    var export1 = s.GetExportData();
                    var export2 = new PhysicScene(export1).GetExportData();
                    string export1Text = JsonHelper.Helper.ToCompactJson(export1);
                    string export2Text = JsonHelper.Helper.ToCompactJson(export2);
                    if (export1Text != export2Text)
                    {
                        throw new Exception("Exporterror");
                    }
                });
            var endState2 = scene.GetExportData();

            string endStateText1 = JsonHelper.Helper.ToCompactJson(endState1);
            string endStateText2 = JsonHelper.Helper.ToCompactJson(endState2);

            endStateText1.Should().Be(endStateText2);
        }
    }
}
