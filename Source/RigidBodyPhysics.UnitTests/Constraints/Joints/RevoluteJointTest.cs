using FluentAssertions;
using RigidBodyPhysics.RuntimeObjects.Joints;
using RigidBodyPhysics.UnitTests.TestHelper;
using Xunit;

namespace RigidBodyPhysics.UnitTests.Constraints.Joints
{
    //Gegeben sind 3 Möglichkeiten für ein Hebelarm wo R2 immer jeweils im Min-Max-Limit ist:
    //-Min ist größer als R2 und Max
    //-Min und R2-Arm liegen unter der 360-Grad-Grenze und Max liegt etwas drüber
    //-Min,R2 und Max liegen alle in sortierter Reihenfolge vor
    public class RevoluteJointTest
    {
        private static string TestData = @"..\..\..\..\..\Data\TestData\RigidBodyPhysicsTestData\JointsTestcases\Revolute\";
        private static float TimeStepTickRate = 50; //[ms]


        //Dieser Test prüft, dass nach dem Laden die Hebelarme alle unverändert sind
        [Fact]
        public void SnapArm_NoMotorChange_PositionIsStill()
        {
            var exptected = ExportHelper.ReadExportData(TestData + "SnapArm.txt").Bodies.Select(x => x.Center.ToIntString()).ToArray();

            var result = SimulateSeveralTimeSteps.DoTest(TestData + "SnapArm.txt", TimeStepTickRate, true, 100, 50, true, new SimulateSeveralTimeSteps.ExtraSettings() { DoPositionCorrection = true, DoWarmStart = true });
            var current = result.TimeSteps.Last().Bodies.Select(x => x.Position.ToIntString()).ToArray();

            string allExpected = string.Join(",", exptected);
            string allCurrent = string.Join(",", current);

            allCurrent.Should().Be(allExpected);
        }

        //Dieser Test bewegt alle Hebealrme zur Min-Position und prüft, ob sie dann dort sind, wo man sie erwartet
        [Fact]
        public void SnapArm_GoToMin_R2IsOnMinArm()
        {
            var exptected = ExportHelper.ReadExportData(TestData + "SnapArmMin.txt").Bodies.Select(x => x.Center.ToIntString()).ToArray();

            var scene = new PhysicScene(ExportHelper.ReadExportData(TestData + "SnapArm.txt"));
            scene.GetAllJoints().Cast<IPublicRevoluteJoint>().ToList().ForEach(x => x.MotorPosition = 0);
            for (int i = 0; i < 100; i++) scene.TimeStep(TimeStepTickRate);
            var current = scene.GetExportData().Bodies.Select(x => x.Center.ToIntString()).ToArray();

            string allExpected = string.Join(",", exptected);
            string allCurrent = string.Join(",", current);

            allCurrent.Should().Be(allExpected);
        }

        //Dieser Test bewegt alle Hebealrme zur Max-Position und prüft, ob sie dann dort sind, wo man sie erwartet
        [Fact]
        public void SnapArm_GoToMax_R2IsOnMinArm()
        {
            var exptected = ExportHelper.ReadExportData(TestData + "SnapArmMax.txt").Bodies.Select(x => x.Center.ToIntString()).ToArray();

            var scene = new PhysicScene(ExportHelper.ReadExportData(TestData + "SnapArm.txt"));
            //scene.HasGravity = false;
            scene.GetAllJoints().Cast<IPublicRevoluteJoint>().ToList().ForEach(x => x.MotorPosition = 1);
            for (int i = 0; i < 100; i++) scene.TimeStep(TimeStepTickRate);
            var current = scene.GetExportData().Bodies.Select(x => x.Center.ToIntString()).ToArray();

            string allExpected = string.Join(",", exptected);
            string allCurrent = string.Join(",", current);

            allCurrent.Should().Be(allExpected);
        }
    }
}
