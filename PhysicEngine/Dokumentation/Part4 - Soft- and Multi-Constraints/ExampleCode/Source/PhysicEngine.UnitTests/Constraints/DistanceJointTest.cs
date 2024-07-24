using FluentAssertions;
using PhysicEngine.UnitTests.TestHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PhysicEngine.UnitTests.Constraints
{
    //Hier soll geprüft werden, dass DistanceJoints immer die gleiche Länge haben und Feder-DistanceJoints in ihren Min-Max-Schranken bleiben
    public class DistanceJointTest
    {
        private static string TestData = @"..\..\..\..\..\Data\Constraints\DistanceConstraint\";
        private static float TimeStepTickRate = 50; //[ms]

        //Simuliert eine ungedämpfte Feder und schaut, wie sich die verschiedenen ResolverHelper verhalten
        [Fact]
        public void DistanceJointsWithDamping_CompareSolvers()
        {
            List<float[]> helper = new List<float[]>();
            helper.Add(GetUndampedOscillator(PhysicScene.Helper.Helper1));
            helper.Add(GetUndampedOscillator(PhysicScene.Helper.Helper2));
            helper.Add(GetUndampedOscillator(PhysicScene.Helper.Helper3));

            StringBuilder str = new StringBuilder();
            str.AppendLine("Helper1\tHelper2\tHelper3");
            for (int i = 0; i < helper[0].Length; i++)
            {
                for (int j=0;j<helper.Count;j++)
                {
                    str.Append(helper[j][i] + "\t");
                }
                str.AppendLine();
            }
            string result = str.ToString();
        }

        private static float[] GetUndampedOscillator(PhysicScene.Helper helper)
        {
            var result = SimulateSeveralTimeSteps.DoTest(TestData + "DistanceJointsWithDamping.txt", TimeStepTickRate, true, 100, 60, new SimulateSeveralTimeSteps.ExtraSettings() { DoPositionCorrection = true, DoWarmStart = true, HasGravity = false, ResolverHelper = helper });
            return SimulateSeveralTimeSteps.GetAnchorDistancesFromJoint(result.TimeSteps, 0);
        }

        [Fact]
        public void Pendulum_SwingsAround_NoLengthChange()
        {
            var result = SimulateSeveralTimeSteps.DoTest(TestData + "Pendulum.txt", TimeStepTickRate, true, 100, 50, new SimulateSeveralTimeSteps.ExtraSettings() { DoPositionCorrection = true, DoWarmStart = true});

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

            float length1Start = (result.TimeSteps[0].Joints[0].Anchor1 - result.TimeSteps[0].Joints[0].Anchor2).Length();
            float length1End = (result.TimeSteps.Last().Joints[0].Anchor1 - result.TimeSteps.Last().Joints[0].Anchor2).Length();
            float length1 = length1End / length1Start;

            float length2Start = (result.TimeSteps[0].Joints[1].Anchor1 - result.TimeSteps[0].Joints[1].Anchor2).Length();
            float length2End = (result.TimeSteps.Last().Joints[1].Anchor1 - result.TimeSteps.Last().Joints[1].Anchor2).Length();
            float length2 = length2End / length2Start;

            length1.Should().BeApproximately(0.6f, 0.001f); //Die MinLength von Joint[0] ist 0.6. Sie darf am Ende nicht kürzer als 60% ihrer Startlänge sein
            length2.Should().BeApproximately(1.4f, 0.001f); //Die MaxLength von Joint[1] ist 1.4. Sie darf am Ende nicht länger als 40% ihrer Startlänge sein
        }
    }
}
