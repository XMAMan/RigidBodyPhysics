using BridgeBuilderControl.Controls.Simulator.Model.Converter;
using BridgeBuilderControl.Testing;
using GraphicPanels;

namespace DemoApplications.UnitTests.BridgeBuilder
{
    //Testet für eine einzelne Brücke ob sie hält und welche Kräfte gewirkt haben, wenn da ein Zug drüber fährt
    internal static class SingleBridgeTestHelper
    {
        public const string GameFolder = @"..\..\..\..\..\Data\GameData\BridgeBuilder\";
        
        public const float TimerTickRateInMs = 30; //ms

        public enum SingleBridgeResult { LevelPassed, LevelFailed, Timeout }
        public class SingleBridgeInputData
        {
            public string BridgeFile;
            public BridgeConverterSettings Settings;
            public int FramesToWaitForTrain;        //So viele Frame läuft die Simulation ohne den Zug am Anfang
            public int MaxFramesToWaitForFinish;
        }

        public class SingleBridgeTestResult
        {
            public string BridgeFile { get; set; }
            public SingleBridgeResult BridgeResult { get; set; }
            public int Frames { get; set; } //So viele Frames hat der Test gedauert
            public double DurrationInSeconds { get; set; } //So viele Millisekunden hat der Test gedauert            
            public float MaxPullForce { get; set; }
            public float MaxPushForce { get; set; }
            public float[] PullForcesForEachTimeStep { get; set; }
            public float[] PushForcesForEachTimeStep { get; set; }
            public PushPullLimit[] PossibleLimits { get; set; }
            public float MaxAllowedPullForce { get; set; }
            public float MaxAllowedPushForce { get; set; }
            public bool SomeBarsAreBroken { get; set; }
            public string FailReason { get; set; } = "";

            public string GetCsvHeader()
            {
                string header = "File\tResult\tFrames\tDurrationInSeconds\tMaxPullForce\tMaxPushForce\tFailReason\t";
                for (int i = 1; i < PossibleLimits.Length; i++)
                {
                    header += $"MinLimit{i}\tMaxLimit{i}\t";
                }
                return header;
            }

            public string ToCsvLine()
            {
                string line = new FileInfo(BridgeFile).Name + "\t" + BridgeResult + "\t" + Frames + "\t" + DurrationInSeconds.ToString("F2") + "\t" + MaxPullForce.ToString().PadRight(15) + "\t" + MaxPushForce.ToString().PadRight(15) + "\t" + (FailReason == null ? "" : FailReason) + "\t";
                for (int i = 1; i < PossibleLimits.Length; i++)
                {
                    line += PossibleLimits[i].MaxPullForce.ToString().PadRight(15) + "\t" + PossibleLimits[i].MaxPushForce.ToString().PadRight(15) + "\t";
                }
                return line;
            }
        }

        public static SingleBridgeTestResult SimulateSingleBridge(SingleBridgeInputData input)
        {
            var panel = new GraphicPanel2D() { Width = 1000, Height = 800, Mode = Mode2D.CPU };
            var simulatorFactory = new BridgeSimulatorFactory(panel, GameFolder, TimerTickRateInMs);
            var simulator = simulatorFactory.CreateSimulator(input.BridgeFile, input.Settings);

            string failReason = null;
            simulator.OnFirstBarIsBroken += (force, maxPull, maxPush) =>
            {
                string reason = "";
                if (force < maxPull) reason = "Pull-Break";
                if (force > maxPush) reason = "Push-Break";
                failReason = reason + " with force=" + force;
            };

            DateTime start = DateTime.Now;

            for (int i = 0; i < input.FramesToWaitForTrain; i++)
            {
                simulator.DoTimeStep(TimerTickRateInMs);
            }

            if (input.MaxFramesToWaitForFinish >= 0)
            {
                simulator.RunTrain();
            }            

            SingleBridgeResult bridgeResult = SingleBridgeResult.Timeout;
            int frames = 0;
            for (int i = 0; i < Math.Abs(input.MaxFramesToWaitForFinish); i++)
            {
                frames = i;
                simulator.DoTimeStep(TimerTickRateInMs);

                if (simulator.TrainIsInWater())
                {
                    bridgeResult = SingleBridgeResult.LevelFailed;
                    break;
                }

                if (simulator.TrainHasPassedTheBridge())
                {
                    bridgeResult = SingleBridgeResult.LevelPassed;
                    break;
                }
            }

            float[] pushForces = simulator.GetPushForcesForEachTimeStep();
            float[] pullForces = simulator.GetPullForcesForEachTimeStep();
            var limits = ForceLimitCalculator.GetLimits(new FileInfo(input.BridgeFile).Name, pushForces, pullForces, 10);

            return new SingleBridgeTestResult()
            {
                BridgeFile = input.BridgeFile,
                BridgeResult = bridgeResult,
                Frames = frames,
                DurrationInSeconds = (DateTime.Now - start).TotalSeconds,
                MaxPullForce = simulator.GetMaxPullForce(),
                MaxPushForce = simulator.GetMaxPushForce(),
                PullForcesForEachTimeStep = pullForces,
                PushForcesForEachTimeStep = pushForces,
                PossibleLimits = limits,
                MaxAllowedPullForce = simulator.GetMaxAllowedPullForce(),
                MaxAllowedPushForce = simulator.GetMaxAllowedPushForce(),
                SomeBarsAreBroken = simulator.SomeBarsAreBroken(),
                FailReason = failReason
            };
        }
    }
}
