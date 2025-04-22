using DynamicObjCreation.RigidBodyDestroying;
using GraphicPanels;
using RigidBodyPhysics.MathHelper;
using System.Drawing;

namespace DynamicObjCreation.UnitTests
{
    public class RotatedBoundingBoxTest
    {
        public const string DataFolder = @"..\..\..\..\..\Data\TestData\DynamicObjCreationTestData\";
        public const string Expected = @"..\..\..\..\..\Data\TestData\DynamicObjCreationTestData\ExpectedImages\";

        private Vec2D[] poly1a = new Vec2D[] //oberes Polygon aus dem RigidBodyDestroyerTest
            {
                new Vec2D(859.1294f, 229.0274f),
                new Vec2D(832.7916f, 290.6417f),
                new Vec2D(762.936f,  290.1898f),
                new Vec2D(734.0706f, 335.3539f),
                new Vec2D(791.6982f, 402.6075f),
                new Vec2D(705.7798f, 433.3163f),
                new Vec2D(622.5471f, 394.0694f),
                new Vec2D(670.1031f, 336.4289f),
                new Vec2D(611.0358f, 296.9948f),
                new Vec2D(651.9968f, 232.0394f),
                new Vec2D(718.7985f, 244.2673f),
                new Vec2D(753.4648f, 178.6986f),
            };

        private float AngleInDegreeA = 45.27072f;

        private Vec2D[] poly1b = new Vec2D[] //Unteres Polygon aus dem RigidBodyDestroyerTest
            {
                new Vec2D(859.1294f, 622.0275f),
                new Vec2D(832.7916f, 683.6417f),
                new Vec2D(762.936f,  683.1899f),
                new Vec2D(734.0706f, 728.3539f),
                new Vec2D(791.6982f, 795.6075f),
                new Vec2D(705.7798f, 826.3163f),
                new Vec2D(622.5471f, 787.0695f),
                new Vec2D(670.1031f, 729.4289f),
                new Vec2D(611.0358f, 689.9948f),
                new Vec2D(651.9968f, 625.0394f),
                new Vec2D(718.7985f, 637.2673f),
                new Vec2D(753.4648f, 571.6987f),
            };

        private float AngleInDegreeB = 15.25519f;

        [Fact]
        public void GetBoxFromPolyA()
        {
            var sut = new RotatedBoundingBox(poly1a, AngleInDegreeA);

            var min = new Vec2D(547, 120);
            var panel = new GraphicPanel2D() { Width = 380, Height = 380, Mode = Mode2D.CPU };
            panel.ClearScreen(Color.White);
            panel.DrawPolygon(Pens.Red, poly1a.Select(x => (x - min).ToGrx()).ToList());
            panel.DrawPolygon(Pens.Green, sut.CornerPoints.Select(x => (x - min).ToGrx()).ToList());
            panel.FlipBuffer();

            panel.GetScreenShoot().Save(DataFolder + "RotatedBoxA.bmp");

            TestHelper.CompareTwoBitmaps(new Bitmap(Expected + "RotatedBoxA.bmp"), new Bitmap(DataFolder + "RotatedBoxA.bmp"));
        }

        [Fact]
        public void GetBoxFromPolyB()
        {
            var sut = new RotatedBoundingBox(poly1b, AngleInDegreeB);

            var min = new Vec2D(573, 540);
            var panel = new GraphicPanel2D() { Width = 306, Height = 325, Mode = Mode2D.CPU };
            panel.ClearScreen(Color.White);
            panel.DrawPolygon(Pens.Red, poly1b.Select(x => (x - min).ToGrx()).ToList());
            panel.DrawPolygon(Pens.Green, sut.CornerPoints.Select(x => (x - min).ToGrx()).ToList());
            panel.FlipBuffer();

            panel.GetScreenShoot().Save(DataFolder + "RotatedBoxB.bmp");

            TestHelper.CompareTwoBitmaps(new Bitmap(Expected + "RotatedBoxB.bmp"), new Bitmap(DataFolder + "RotatedBoxB.bmp"));
        }
    }
}
