using DemoApplicationHelper;
using DemoApplications.UnitTests.SoundMocking;
using GameHelper;
using GraphicPanels;
using System.Drawing;
using WpfControls.Model;

namespace DemoApplications.UnitTests
{
    internal class TestHelper
    {
        public static TestResult RunSimulation(IEditorFactory editorFactory, string gameFolder, string levelFile, string keyboardFile, int singleImageWidth = 300, int singleImageHeight = 200, int imageCountPerRow = 10)
        {
            var sound = new SoundGeneratorMock();
            var panel = new GraphicPanel2D() { Width = singleImageWidth, Height = singleImageHeight, Mode = Mode2D.OpenGL_Version_3_0 };

            var sut = (IPhysicSimulated)editorFactory.CreateEditorViewModel(new EditorInputDataWithSound()
            {
                Panel = panel,
                DataFolder = DemoGameTests.GameFolder + gameFolder + "\\",
                SoundGenerator = sound,
                TimerTickRateInMs = DemoGameTests.TimerTickRateInMs,
            });

            int timerTicks = sut.LoadSimulation(levelFile, keyboardFile);
            int imageCount = imageCountPerRow * imageCountPerRow;
            int ticksPerImage = timerTicks / imageCount;

            List<Bitmap> images = new List<Bitmap>();
            for (int i = 0; i < timerTicks; i++)
            {
                bool isFinish = sut.DoTimeStep(DemoGameTests.TimerTickRateInMs);
                if (i % ticksPerImage == 0)
                {
                    var image = panel.GetScreenShoot();
                    images.Add(image);
                }
            }

            List<Bitmap> rows = new List<Bitmap>();
            while (images.Any())
            {
                List<Bitmap> del = new List<Bitmap>();
                for (int i = 0; i < imageCountPerRow; i++)
                {
                    if (images.Any())
                    {
                        var image = images[0];
                        images.RemoveAt(0);
                        del.Add(image);
                    }
                }
                var row = BitmapHelper.BitmapHelp.TransformBitmapListToRow(del);
                rows.Add(row);
            }

            var masterImage = BitmapHelper.BitmapHelp.TransformBitmapListToCollum(rows);
            string soundLog = sound.GetLoggingText().Replace(DemoGameTests.GameFolder, "");

            panel.Dispose();

            return new TestResult()
            {
                Image = masterImage,
                SoundLog = soundLog,
            };
        }

        public static bool CompareTwoBitmaps(Bitmap img1, Bitmap img2, bool throwException = true)
        {
            if (img1.Width != img2.Width || img1.Height != img2.Height) throw new Exception($"img1.Width={img1.Width}, img1.Height={img1.Height} <-> img2.Width={img2.Width}, img2.Height={img2.Height}");

            Bitmap errorImage = new Bitmap(img1.Width, img1.Height);

            int maxColorError = 2;
            int maxPixelCount = 600;

            List<string> errors = new List<string>();

            for (int x = 0; x < img1.Width; x++)
                for (int y = 0; y < img1.Height; y++)
                {
                    Color c1 = img1.GetPixel(x, y);
                    Color c2 = img2.GetPixel(x, y);

                    //errorImage.SetPixel(x, y, c2);

                    if (Math.Abs(c1.R - c2.R) > maxColorError ||
                        Math.Abs(c1.G - c2.G) > maxColorError ||
                        Math.Abs(c1.B - c2.B) > maxColorError ||
                        Math.Abs(c1.A - c2.A) > maxColorError)
                    {
                        errors.Add($"([{x};{y}] R={c1.R}/{c2.R}  G={c1.G}/{c2.G}  B={c1.B}/{c2.B})");

                        errorImage.SetPixel(x, y, Color.Red);
                    }
                }

            if (throwException && errors.Count > maxPixelCount)
            {
                errorImage.Save(DemoGameTests.TestResultsFolder + "ErrorImage.bmp");
                throw new Exception($"ErrorCount={errors.Count}");
            }

            return true;
        }
    }

    internal class TestResult
    {
        public Bitmap Image;
        public string SoundLog;
    }
}
