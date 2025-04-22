using BridgeBuilderControl.Controls.Simulator.Model.Converter;
using System.Text;
using static DemoApplications.UnitTests.BridgeBuilder.SingleBridgeTestHelper;
using SingleResult = DemoApplications.UnitTests.BridgeBuilder.SingleBridgeTestHelper.SingleBridgeTestResult;

namespace DemoApplications.UnitTests.BridgeBuilder
{
    //Testet für mehrere Brücken ob sie gehalten haben und welche Kräfte gewirkt haben, wenn da ein Zug darüber gefahren ist
    internal static class MultipleBridgeTestHelper
    {
        public class TestResult
        {
            public SingleResult[] Singles { get; set; }
            public SingleResult OverallResult { get; set; }

            public PushPullLimit[] AllLimits { get; set; }

            public TestResult(SingleResult[] singles)
            {
                this.Singles = singles;
                this.OverallResult = GetOverallResult(singles);
                this.AllLimits = singles.SelectMany(x => x.PossibleLimits).ToArray();
            }

            private static SingleResult GetOverallResult(SingleResult[] singles)
            {
                SingleResult result = new SingleResult();

                result.BridgeFile = "*";

                result.BridgeResult = SingleBridgeResult.LevelPassed;

                if (singles.Any(x => x.BridgeResult == SingleBridgeResult.LevelFailed))
                    result.BridgeResult = SingleBridgeResult.LevelFailed;

                if (singles.Any(x => x.BridgeResult == SingleBridgeResult.Timeout))
                    result.BridgeResult = SingleBridgeResult.Timeout;

                result.Frames = singles.Max(x => x.Frames);
                result.DurrationInSeconds = singles.Max(x => x.DurrationInSeconds);
                result.MaxPullForce = singles.Min(x => x.MaxPullForce);
                result.MaxPushForce = singles.Max(x => x.MaxPushForce);

                result.PossibleLimits = new PushPullLimit[singles[0].PossibleLimits.Length];
                for (int i = 0; i < result.PossibleLimits.Length;i++)
                {
                    float pullForce = singles.Min(x => x.PossibleLimits[i].MaxPullForce);
                    float pushForce = singles.Max(x => x.PossibleLimits[i].MaxPushForce);
                    result.PossibleLimits[i] = new PushPullLimit() 
                    { 
                        BreakAfterNSteps = singles[0].PossibleLimits[i].BreakAfterNSteps, 
                        MaxPullForce = pullForce,
                        MaxPushForce = pushForce,
                        LevelFile = "*",
                    };
                }

                return result;
            }

            public override string ToString()
            {
                StringBuilder lines = new StringBuilder();
                lines.AppendLine(Singles.First().GetCsvHeader());

                foreach (var single in Singles)
                {
                    lines.AppendLine(single.ToCsvLine());
                }

                lines.AppendLine(new string('=', Singles.First().GetCsvHeader().Length));

                lines.AppendLine(this.OverallResult.ToCsvLine());

                return lines.ToString();
            }
        }

        public static TestResult SimulateMultipleBridges(string[] bridgeFiles, int[] timeToWaitForTrain, BridgeConverterSettings settings)
        {
            var testData = new SingleBridgeInputData()
            {
                Settings = settings,
                MaxFramesToWaitForFinish = 3000,

            };

            List<SingleResult> singles = new List<SingleResult>();

            for (int i=0; i<bridgeFiles.Length;i++)
            {
                testData.BridgeFile = bridgeFiles[i];
                testData.FramesToWaitForTrain = timeToWaitForTrain[i];
                var result = SingleBridgeTestHelper.SimulateSingleBridge(testData);

                singles.Add(result);
            }

            return new TestResult(singles.ToArray());
        }
    }
}
