using BridgeBuilderControl.Controls.Simulator.Model.Converter;
using System.Drawing;


namespace DemoApplications.UnitTests.BridgeBuilder
{
    //Erzeugt eine Grafik, welche von allen Leveln alle Push-Pull-Kräfte über der Zeit darstellt
    internal static class ForcePlotter
    {
        public static Bitmap PlotAllLevels(MultipleBridgeTestHelper.TestResult result)
        {
            float minValue = result.Singles.Min(x => x.PossibleLimits[0].MaxPullForce);
            float maxValue = result.Singles.Max(x => x.PossibleLimits[0].MaxPushForce);
            int width = 300;
            int height = 400;

            var images = result.Singles.Select(x => PlotSingleLevel(x.PullForcesForEachTimeStep, x.PushForcesForEachTimeStep, x.PossibleLimits, minValue, maxValue, width, height, new FileInfo(x.BridgeFile).Name)).ToList();
            //var images = result.Singles.Select(x => PlotSingleLevel(x.PullForcesForEachTimeStep, x.PushForcesForEachTimeStep, x.PossibleLimits, x.PossibleMinLimits[0], x.PossibleMaxLimits[0], width, height, new FileInfo(x.BridgeFile).Name)).ToList();

            return BitmapHelper.BitmapHelp.TransformBitmapListToRow(images);
        }

        public static Bitmap PlotSingleLevel(float[] pullForces, float[] pushForces, PushPullLimit[] limits, float minValue, float maxValue, int width, int height, string text)
        {
            //Umrechnen in Y-Bildkooridnaten
            pullForces = GetYValues(pullForces, minValue, maxValue, height);
            pushForces = GetYValues(pushForces, minValue, maxValue, height);
            float[] minLimits = GetYValues(limits.Select(x => x.MaxPullForce).ToArray(), minValue, maxValue, height);
            float[] maxLimits = GetYValues(limits.Select(x => x.MaxPushForce).ToArray(), minValue, maxValue, height);

            Bitmap image = new Bitmap(width, height);
            Graphics grx = Graphics.FromImage(image);
            grx.Clear(Color.White);

            grx.DrawString(text, new Font("Consolas", 20), Brushes.Black, 10, 10);

            for (int i=0;i< pullForces.Length-1;i++)
            {
                float x1 = i / (float)pullForces.Length * width;
                float x2 = (i + 1) / (float)pullForces.Length * width;
                grx.DrawLine(Pens.Blue, x1, pullForces[i], x2, pullForces[i + 1]);
                grx.DrawLine(Pens.Red, x1, pushForces[i], x2, pushForces[i + 1]);
            }

            for (int i = 1; i < minLimits.Length; i++)
            {
                float x1 = 0;
                float x2 = width;
                int c = Math.Min(255, 50 + i * 20);
                Pen pen = new Pen(Color.FromArgb(c, c, c), 1);
                grx.DrawLine(pen, x1, minLimits[i], x2, minLimits[i]);
                grx.DrawLine(pen, x1, maxLimits[i], x2, maxLimits[i]);
            }

            grx.Dispose();
            return image;
        }

        private static float[] GetYValues(float[] values, float minValue, float maxValue, int imageHeight)
        {
            float range = maxValue - minValue;
            return values.Select(x => imageHeight - (x - minValue) / range * imageHeight).ToArray();
        }


    }
}
