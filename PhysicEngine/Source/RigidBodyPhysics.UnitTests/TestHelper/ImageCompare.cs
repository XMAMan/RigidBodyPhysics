using System.Drawing;

namespace RigidBodyPhysics.UnitTests.TestHelper
{
    internal class ImageCompare
    {
        private static string TestResults = @"..\..\..\..\..\Data\TestData\RigidBodyPhysicsTestData\TestResults\";

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
                errorImage.Save(TestResults + "ErrorImage.bmp");
                throw new Exception($"ErrorCount={errors.Count}");
            }

            return true;
        }
    }
}
