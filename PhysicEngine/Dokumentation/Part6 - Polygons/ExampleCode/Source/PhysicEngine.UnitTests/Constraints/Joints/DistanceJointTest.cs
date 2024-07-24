using FluentAssertions;
using PhysicEngine.UnitTests.TestHelper;
using Xunit;

namespace PhysicEngine.UnitTests.Constraints.Joints
{
    //Hier soll geprüft werden, dass DistanceJoints immer die gleiche Länge haben und Feder-DistanceJoints in ihren Min-Max-Schranken bleiben
    public class DistanceJointTest
    {
        private static string TestData = @"..\..\..\..\..\Data\JointsTestcases\Distance\";
        private static float TimeStepTickRate = 50; //[ms]

        [Fact]
        public void Pendulum_SwingsAround_NoLengthChange()
        {
            var result = SimulateSeveralTimeSteps.DoTest(TestData + "Pendulum.txt", TimeStepTickRate, true, 100, 50, new SimulateSeveralTimeSteps.ExtraSettings() { DoPositionCorrection = true, DoWarmStart = true });

            float[] ranges = SimulateSeveralTimeSteps.GetLengthRangeForEachJoint(result.TimeSteps);
            ranges.Max().Should().BeLessThan(24); //Alle DistanceJoints ändert nicht mehr als um 24 Pixel ihre Länge
        }

        [Fact]
        public void SuspensionBridge_SwingsAround_NoLengthChange()
        {
            var result = SimulateSeveralTimeSteps.DoTest(TestData + "suspension_bridge.txt", TimeStepTickRate, true, 100, 50, new SimulateSeveralTimeSteps.ExtraSettings() { DoPositionCorrection = true, DoWarmStart = true });

            float[] ranges = SimulateSeveralTimeSteps.GetLengthRangeForEachJoint(result.TimeSteps);
            ranges.Max().Should().BeLessThan(2); //Alle DistanceJoints ändert nicht mehr als um 2 Pixel ihre Länge
        }

        [Fact]
        public void DistanceJointsWithDamping_SwingsAround_NoLengthChange()
        {
            var result = SimulateSeveralTimeSteps.DoTest(TestData + "DistanceJointsWithDamping.txt", TimeStepTickRate, true, 100, 50, new SimulateSeveralTimeSteps.ExtraSettings() { DoPositionCorrection = true, DoWarmStart = true });

            float[] ranges = SimulateSeveralTimeSteps.GetLengthRangeForEachJoint(result.TimeSteps);
            ranges.Max().Should().BeLessThan(110); //Alle DistanceJoints ändert nicht mehr als um 110 Pixel ihre Länge
        }

        [Fact]
        public void DistaneJointWithMinMax_SwingsAround_NoLengthChange()
        {
            var result = SimulateSeveralTimeSteps.DoTest(TestData + "DistaneJointWithMinMax.txt", TimeStepTickRate, true, 100, 50, new SimulateSeveralTimeSteps.ExtraSettings() { DoPositionCorrection = true, DoWarmStart = true });

            float length1End = (result.TimeSteps.Last().Joints[0].Anchor1 - result.TimeSteps.Last().Joints[0].Anchor2).Length();
            float length2End = (result.TimeSteps.Last().Joints[1].Anchor1 - result.TimeSteps.Last().Joints[1].Anchor2).Length();

            length1End.Should().BeApproximately(144, 0.1f); //Die MinLength von Joint[0] ist 144 Pixel. Prüfe, dass die obere Kugel beim runterfallen gestoppt wurde
            length2End.Should().BeApproximately(344, 0.1f); //Die MaxLength von Joint[1] ist 344 Pixel. Sie darf nicht länger als dieser Abstand gefallen sein.
        }
    }
}
