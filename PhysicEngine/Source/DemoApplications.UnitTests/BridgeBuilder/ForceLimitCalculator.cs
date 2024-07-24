using BridgeBuilderControl.Controls.Simulator.Model.Converter;

namespace DemoApplications.UnitTests.BridgeBuilder
{
    //Ermittelt, welche MaxPullForce/MaxPushForce man abhängig von BreakAfterNSteps verwenden muss, damit die Brücke gerade noch hält
    internal static class ForceLimitCalculator
    {
        public static PushPullLimit[] GetLimits(string levelFile, float[] pushForces, float[] pullForces, int maxBreakAfterNSteps)
        {
            float[] maxLimits = GetMaxLimits(pushForces, 10);
            float[] minLimits = GetMinLimits(pullForces, 10);

            List< PushPullLimit > limits = new List< PushPullLimit >();
            for (int i = 0; i < maxLimits.Length; i++)
            {
                limits.Add(new PushPullLimit()
                {
                    BreakAfterNSteps = i,
                    MaxPullForce = minLimits[i],
                    MaxPushForce = maxLimits[i],
                    LevelFile = levelFile
                });
            }
            return limits.ToArray();
        }


        //Index:
        //0 -> Maxwert
        //1 -> MaxPullForce wenn BreakAfterNSteps = 1
        //2 -> MaxPullForce wenn BreakAfterNSteps = 2
        //3 -> MaxPullForce wenn BreakAfterNSteps = 3
        public static float[] GetMaxLimits(float[] values, int maxBreakAfterNSteps)
        {
            var ordered = values.OrderBy(x => x).ToList();
            float range = ordered.Last() - ordered.First();

            List<float> between = new List<float>(); //An diesen Positionen kann potentiell eine Schranke sein
            between.Add(ordered.First() - range * 0.01f);
            for (int i = 0; i < ordered.Count - 1; i++)
            {
                between.Add((ordered[i] + ordered[i + 1]) / 2);
            }
            between.Add(ordered.Last() + range * 0.01f);

            List<float> limits = new List<float>();
            limits.Add(ordered.Last());

            for (int breakAfterNSteps = 1; breakAfterNSteps < maxBreakAfterNSteps; breakAfterNSteps++)
            {
                for (int i = between.Count - 1; i >= 0; i--)
                {
                    bool isPossibleLimite = IsPossibleMaxLimit(between[i], values, breakAfterNSteps);
                    if (isPossibleLimite == false)
                    {
                        limits.Add(between[i + 1]);
                        break;
                    }
                }
            }

            return limits.ToArray();
        }

        private static bool IsPossibleMaxLimit(float limit, float[] values, int breakAfterNSteps)
        {
            for (int i = 0; i < values.Length; i++)
            {
                bool allLastStepsOverLimit = true;
                for (int j=0;j<breakAfterNSteps; j++)
                {
                    int index = i - j;
                    if (index >= 0 && values[index] < limit)
                    {
                        allLastStepsOverLimit = false;
                        break;
                    }
                }
                if (allLastStepsOverLimit) return false;
            }

            return true;
        }

        //Index:
        //0 -> Minwert
        //1 -> MaxPushForce wenn BreakAfterNSteps = 1
        //2 -> MaxPushForce wenn BreakAfterNSteps = 2
        //3 -> MaxPushForce wenn BreakAfterNSteps = 3
        public static float[] GetMinLimits(float[] values, int maxBreakAfterNSteps)
        {
            var ordered = values.OrderBy(x => x).ToList();
            float range = ordered.Last() - ordered.First();

            List<float> between = new List<float>(); //An diesen Positionen kann potentiell eine Schranke sein
            between.Add(ordered.First() - range * 0.01f);
            for (int i = 0; i < ordered.Count - 1; i++)
            {
                between.Add((ordered[i] + ordered[i + 1]) / 2);
            }
            between.Add(ordered.Last() + range * 0.01f);

            List<float> limits = new List<float>();
            limits.Add(ordered.First());

            for (int breakAfterNSteps = 1; breakAfterNSteps < maxBreakAfterNSteps; breakAfterNSteps++)
            {
                for (int i = 0; i < between.Count;i++)
                {
                    bool isPossibleLimite = IsPossibleMinLimit(between[i], values, breakAfterNSteps);
                    if (isPossibleLimite == false)
                    {
                        limits.Add(between[i - 1]);
                        break;
                    }
                }
            }

            return limits.ToArray();
        }

        private static bool IsPossibleMinLimit(float limit, float[] values, int breakAfterNSteps)
        {
            for (int i = 0; i < values.Length; i++)
            {
                bool allLastStepsUnderLimit = true;
                for (int j = 0; j < breakAfterNSteps; j++)
                {
                    int index = i - j;
                    if (index >= 0 && values[index] > limit)
                    {
                        allLastStepsUnderLimit = false;
                        break;
                    }
                }
                if (allLastStepsUnderLimit) return false;
            }

            return true;
        }
    }
}
