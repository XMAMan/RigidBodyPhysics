using GraphicMinimal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace KeyFrameEditorControl.Dialogs.LoadSprite
{
    //Wenn man Sprites aus dem Internet bekommt, dann sind die Sprite-Einzelbilder meist unterschiedlich groß
    //Hiermit werden die Einzelbilder so angeordnet, dass jedes Einzelbild gleich groß ist
    internal static class SpriteBitmapHelper
    {
        //Der Hintergrund wird weiß und der Rest schwarz
        public static Bitmap TransformImageIntoTwoColors(Bitmap image, float bias = 0.1f)
        {
            Color backColor = image.GetPixel(0, 0);

            if (CompareTwoColors(Color.White, backColor, bias))
                backColor = Color.White;
            else
                if (CompareTwoColors(Color.Black, backColor, bias))
                backColor = Color.Black;

            Bitmap newImage = new Bitmap(image.Width, image.Height);
            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    Color c = image.GetPixel(x, y);
                    if (CompareTwoColors(c, backColor, bias))
                        newImage.SetPixel(x, y, Color.White);
                    else
                        newImage.SetPixel(x, y, Color.Black);
                }
            return newImage;
        }

        //Input = Sprit-Bild wo die Einzelbilder schlampig angeordnet sind
        //Output = Sprit-Bild wo alle Einzelbilder gleich groß ist und der Inhalt in der Mitte ist
        public static Bitmap GetCorrectedSpriteImage(Bitmap bitmap, out int xCount, out int yCount, out Bitmap testOutput)
        {
            Color backColor = Color.White;

            var twoColor = TransformImageIntoTwoColors(bitmap);
            var regions = GetRegionsFromATwoColorBitmap(twoColor, out testOutput);
            regions = OrderRegionsRowByRow(regions);

            //Testausgabe der Boundingboxen
            for (int i = 0; i < regions.Length; i++)
            {
                DrawRectangle(testOutput, regions[i].BoundingBox, Color.Green);

                WriteToBitmap(testOutput, regions[i].Index.X + " " + regions[i].Index.Y, Color.Blue, regions[i].Center.X, regions[i].Center.Y);
            }

            if (regions.Length == 1)
            {
                xCount = yCount = 1;
                return bitmap;
            }

            xCount = regions.Max(x => x.Index.X) + 1; //Anzahl der Einzelbilder
            yCount = regions.Max(y => y.Index.Y) + 1;
            int border = 20; //20 Pixel Rand
            Size s = new Size(regions.Max(x => x.BoundingBox.Width) + border, regions.Max(x => x.BoundingBox.Height) + border); //So groß ist ein Einzelbild

            Bitmap output = GetEmptyImage(xCount * s.Width, yCount * s.Height, backColor);
            foreach (var region in regions)
            {
                Point center = new Point(region.Index.X * s.Width + s.Width / 2, region.Index.Y * s.Height + s.Height / 2);
                Point delta = new Point(center.X - region.Center.X, center.Y - region.Center.Y);
                foreach (var pix in region.Points)
                {
                    Color col = bitmap.GetPixel(pix.X, pix.Y);
                    output.SetPixel(pix.X + delta.X, pix.Y + delta.Y, col);
                }
            }

            return output;
        }

        //Testausgabe der markierten Bereiche
        public static Bitmap MarkEachRegionFromATwoColorImage(Bitmap twoColor)
        {
            var regions = GetRegionsFromATwoColorBitmap(twoColor, out Bitmap testOutput);
            regions = OrderRegionsRowByRow(regions);

            //Testausgabe der Boundingboxen
            for (int i = 0; i < regions.Length; i++)
            {
                DrawRectangle(testOutput, regions[i].BoundingBox, Color.Green);

                WriteToBitmap(testOutput, regions[i].Index.X + " " + regions[i].Index.Y, Color.Blue, regions[i].Center.X, regions[i].Center.Y);
            }

            return testOutput;
        }

        class BitmapRegion
        {
            public Point[] Points;
            public Rectangle BoundingBox;
            public Point Center;
            public Point Index = new Point(-1, -1);
        }
        private static BitmapRegion[] GetRegionsFromATwoColorBitmap(Bitmap twoColor, out Bitmap testOutput)
        {
            twoColor = CreateCopy(twoColor);

            Color[] colors = new Color[] { Color.Red, Color.Green, Color.Blue, Color.Orange, Color.Yellow, Color.Beige, Color.Navy, Color.Olive };
            int col = 0;

            Color backColor = Color.White;
            Color foreColor = Color.Black;

            List<BitmapRegion> regions = new List<BitmapRegion>();

            for (int x = 0; x < twoColor.Width; x++)
                for (int y = 0; y < twoColor.Height; y++)
                {
                    Color c = twoColor.GetPixel(x, y);
                    if (CompareTwoColors(c, foreColor))
                    {
                        var filledPixels = FloodFill(twoColor, colors[col % colors.Length], new Point(x, y));
                        if (filledPixels.Length > 10)
                        {
                            col++;
                            Point min = new Point(filledPixels.Min(x => x.X), filledPixels.Min(x => x.Y));
                            Point max = new Point(filledPixels.Max(x => x.X), filledPixels.Max(x => x.Y));

                            var boundingBox = new Rectangle(min.X, min.Y, max.X - min.X + 1, max.Y - min.Y + 1);
                            regions.Add(new BitmapRegion()
                            {
                                Points = filledPixels,
                                BoundingBox = boundingBox,
                                Center = new Point(boundingBox.X + boundingBox.Width / 2, boundingBox.Y + boundingBox.Height / 2)
                            });
                        }
                        else
                        {
                            //Entferne einzelne Pixel, die zu keiner Region gehören
                            foreach (var p in filledPixels)
                                twoColor.SetPixel(p.X, p.Y, backColor);
                        }

                    }
                }


            testOutput = twoColor;

            return regions.ToArray();
        }

        private static BitmapRegion[] OrderRegionsRowByRow(BitmapRegion[] regions)
        {
            if (regions.Length <= 1) return regions;

            float maxDiff = 0.1f;

            Vector2D min = new Vector2D(regions.Select(x => x.BoundingBox.Left).Min(), regions.Select(x => x.BoundingBox.Top).Min());
            Vector2D max = new Vector2D(regions.Select(x => x.BoundingBox.Right).Max(), regions.Select(x => x.BoundingBox.Bottom).Max());
            Vector2D range = max - min;

            List<Cluster<BitmapRegion>> clustersX = new List<Cluster<BitmapRegion>>();
            List<Cluster<BitmapRegion>> clustersY = new List<Cluster<BitmapRegion>>();
            foreach (var region in regions)
            {
                Vector2D pos = new Vector2D((region.Center.X - min.X) / range.X, (region.Center.Y - min.Y) / range.Y);

                bool foundX = false;
                foreach (var clusterX in clustersX)
                {
                    if (clusterX.AddValue(pos.X, region))
                    {
                        foundX = true;
                        break;
                    }
                }
                if (foundX == false)
                {
                    clustersX.Add(new Cluster<BitmapRegion>(maxDiff));
                    clustersX.Last().AddValue(pos.X, region);
                }

                bool foundY = false;
                foreach (var clusterY in clustersY)
                {
                    if (clusterY.AddValue(pos.Y, region))
                    {
                        foundY = true;
                        break;
                    }
                }
                if (foundY == false)
                {
                    clustersY.Add(new Cluster<BitmapRegion>(maxDiff));
                    clustersY.Last().AddValue(pos.Y, region);
                }
            }

            var orderedX = clustersX.OrderBy(x => x.Center).ToList();
            var orderedY = clustersY.OrderBy(x => x.Center).ToList();

            foreach (var region in regions)
            {
                region.Index.X = orderedX.IndexOf(orderedX.First(x => x.Contains(region)));
                region.Index.Y = orderedY.IndexOf(orderedY.First(x => x.Contains(region)));
            }

            return regions.OrderBy(x => x.Index.Y * orderedX.Count + x.Index.X).ToArray();
        }

        class Cluster<T> where T : class
        {
            private float MaxDiff = 0.2f;

            private List<KeyValuePair<float, T>> values = new List<KeyValuePair<float, T>>();

            public float Center { get; private set; } = float.NaN;

            public bool Contains(T item)
            {
                return this.values.Any(x => x.Value == item);
            }

            public Cluster(float maxDiff)
            {
                this.MaxDiff = maxDiff;
            }

            //Return: true = Wert konnte eingefügt werden, da dessen Abstand zu den anderen Werten klein genug ist
            public bool AddValue(float f, T value)
            {
                if (values.Any() == false)
                {
                    this.values.Add(new KeyValuePair<float, T>(f, value));
                    this.Center = f;
                }
                else
                {
                    float diff = Math.Abs(f - Center);
                    if (diff < this.MaxDiff)
                    {
                        this.values.Add(new KeyValuePair<float, T>(f, value));
                        this.Center = 0.5f * this.values.Select(x => x.Key).Min() + 0.5f * this.values.Select(x => x.Key).Max();
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private static void DrawRectangle(Bitmap bitmap, Rectangle rec, Color color)
        {
            for (int x = rec.X; x < rec.X + rec.Width; x++)
            {
                SetBitmapPixel(bitmap, x, rec.Y, color);
                SetBitmapPixel(bitmap, x, rec.Y + rec.Height, color);
            }

            for (int y = rec.Y; y < rec.Y + rec.Height; y++)
            {
                SetBitmapPixel(bitmap, rec.X, y, color);
                SetBitmapPixel(bitmap, rec.X + rec.Width, y, color);
            }
        }

        private static void SetBitmapPixel(Bitmap bitmap, int x, int y, Color color)
        {
            if (x >= 0 && x < bitmap.Width && y >= 0 && y < bitmap.Height)
            {
                bitmap.SetPixel(x, y, color);
            }
        }

        public static Bitmap GetEmptyImage(int widht, int height, Color color)
        {
            Bitmap image = new Bitmap(widht, height);
            Graphics grx = Graphics.FromImage(image);
            grx.Clear(color);
            grx.Dispose();
            return image;
        }

        public static Bitmap WriteToBitmap(Bitmap bitmap, string text, Color color, int x, int y)
        {
            bitmap.SetResolution(96, 96);
            Graphics grx = Graphics.FromImage(bitmap);
            grx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
            grx.TextContrast = 4;
            grx.DrawString(text, new Font("Arial", 10), new SolidBrush(color), new PointF(x, y));
            grx.Dispose();
            return bitmap;
        }

        public static Bitmap CreateCopy(Bitmap bitmap)
        {
            Bitmap copy = new Bitmap(bitmap.Width, bitmap.Height);
            for (int x = 0; x < bitmap.Width; x++)
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Color c = bitmap.GetPixel(x, y);
                    copy.SetPixel(x, y, c);
                }
            return copy;
        }

        //Füllt das Bild per FloodFill von startPoint aus mit color aus. Returnwert: Diese Pixel wurden alle gefüllt
        //https://simpledevcode.wordpress.com/2015/12/29/flood-fill-algorithm-using-c-net/
        private static Point[] FloodFill(Bitmap bmp, Color replacementColor, Point pt)
        {
            //Color c = bitmap.GetPixel(startPoint.X, startPoint.Y); //Diese Farbe soll überschrieben werden

            List<Point> filledPixels = new List<Point>(); //All diese Pixel wurden verändert

            Stack<Point> pixels = new Stack<Point>();
            Color targetColor = bmp.GetPixel(pt.X, pt.Y);
            pixels.Push(pt);

            while (pixels.Count > 0)
            {
                Point a = pixels.Pop();
                if (a.X < bmp.Width && a.X > 0 &&
                        a.Y < bmp.Height && a.Y > 0)//make sure we stay within bounds
                {

                    if (CompareTwoColors(bmp.GetPixel(a.X, a.Y), targetColor))
                    {
                        bmp.SetPixel(a.X, a.Y, replacementColor);
                        filledPixels.Add(a);
                        pixels.Push(new Point(a.X - 1, a.Y));
                        pixels.Push(new Point(a.X + 1, a.Y));
                        pixels.Push(new Point(a.X, a.Y - 1));
                        pixels.Push(new Point(a.X, a.Y + 1));
                    }
                }
            }

            return filledPixels.ToArray();
        }

        public static bool CompareTwoColors(Color c1, Color c2, float bias)
        {
            Vector3D cv1 = new Vector3D(c1.R / 255f, c1.G / 255f, c1.B / 255f);
            Vector3D cv2 = new Vector3D(c2.R / 255f, c2.G / 255f, c2.B / 255f);
            return (cv1 - cv2).Length() < bias;
        }

        public static bool CompareTwoColors(Color c1, Color c2)
        {
            return c1.R == c2.R && c1.G == c2.G && c1.B == c2.B;
        }
    }
}
