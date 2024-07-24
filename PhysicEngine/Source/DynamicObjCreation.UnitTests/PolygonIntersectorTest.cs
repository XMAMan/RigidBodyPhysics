using DynamicObjCreation.PolygonIntersection;
using GraphicPanels;
using RigidBodyPhysics.MathHelper;
using System.Drawing;

namespace DynamicObjCreation.UnitTests
{
    public class PolygonIntersectorTest
    {
        public const string DataFolder = @"..\..\..\..\..\Data\TestData\DynamicObjCreationTestData\";
        public const string Expected = @"..\..\..\..\..\Data\TestData\DynamicObjCreationTestData\ExpectedImages\";

        private Vec2D[] poly1a = new Vec2D[] //oberes Polygon aus dem RigidBodyDestroyerTest
            {
                new Vec2D(859.129395f, 229.027435f),
                new Vec2D(832.791565f, 290.641724f),
                new Vec2D(762.936035f, 290.18985f ),
                new Vec2D(734.070618f, 335.353851f),
                new Vec2D(791.698242f, 402.607513f),
                new Vec2D(705.779846f, 433.316284f),
                new Vec2D(622.547119f, 394.069397f),
                new Vec2D(670.103088f, 336.428864f),
                new Vec2D(611.035767f, 296.994751f),
                new Vec2D(651.996765f, 232.039398f),
                new Vec2D(718.798523f, 244.267288f),
                new Vec2D(753.464783f, 178.698608f),
            };

        private Vec2D[] poly1b = new Vec2D[] //Unteres Polygon aus dem RigidBodyDestroyerTest
            {
                new Vec2D(859.129395f, 622.027466f),
                new Vec2D(832.791565f, 683.641724f),
                new Vec2D(762.936035f, 683.18988f ),
                new Vec2D(734.070618f, 728.353882f),
                new Vec2D(791.698242f, 795.607544f),
                new Vec2D(705.779846f, 826.316284f),
                new Vec2D(622.547119f, 787.069458f),
                new Vec2D(670.103088f, 729.428894f),
                new Vec2D(611.035767f, 689.994751f),
                new Vec2D(651.996765f, 625.039429f),
                new Vec2D(718.798523f, 637.267334f),
                new Vec2D(753.464783f, 571.698669f),
            };

        //Testfall 1: Zwei Polygone liegen nebeneinander -> Es gibt keine Schnittmenge und wird null zurück gegeben
        [Fact]
        public void Intersect1()
        {
            var poly1 = new Vec2D[]
            {
                new Vec2D(40, 194),
                new Vec2D(59, 237),
                new Vec2D(20, 241)
            };

            var poly2 = new Vec2D[]
            {
                new Vec2D(81,194),
                new Vec2D(104, 191),
                new Vec2D(106, 242),
                new Vec2D(77, 242)
            };

            var resultPoly = PolygonIntersector.GetIntersection(poly1, poly2);
            GetTestOutputForPolys(poly1, poly2, true, resultPoly).Save(DataFolder + "Intersect1.bmp");

            TestHelper.CompareTwoBitmaps(new Bitmap(Expected + "Intersect1.bmp"), new Bitmap(DataFolder + "Intersect1.bmp"));
        }

        //Testfall 2: Polygon2 liegt komplett ohne Kantenschnittpunkt in Polygon 1 -> Gebe Polygon2 zurück
        [Fact]
        public void Intersect2()
        {
            var poly1 = new Vec2D[]
            {
                new Vec2D(40, 75),
                new Vec2D(251, 26),
                new Vec2D(239, 213)
            };

            var poly2 = new Vec2D[]
            {
                new Vec2D(161,63),
                new Vec2D(202, 89),
                new Vec2D(212, 143),
                new Vec2D(141, 99)
            };

            var resultPoly = PolygonIntersector.GetIntersection(poly1, poly2);
            GetTestOutputForPolys(poly1, poly2, true, resultPoly).Save(DataFolder + "Intersect2.bmp");

            TestHelper.CompareTwoBitmaps(new Bitmap(Expected + "Intersect2.bmp"), new Bitmap(DataFolder + "Intersect2.bmp"));
        }

        //Testfall 3: Ein Eckpunkt dringt in das andere Polygon ein
        [Fact]
        public void Intersect3()
        {
            var poly1 = new Vec2D[]
            {
                new Vec2D(246, 84),
                new Vec2D(173, 151),
                new Vec2D(96, 27)
            };

            var poly2 = new Vec2D[]
            {
                new Vec2D(64,22),
                new Vec2D(122, 13),
                new Vec2D(81, 56)
            };

            var resultPoly = PolygonIntersector.GetIntersection(poly1, poly2);
            GetTestOutputForPolys(poly1, poly2, true, resultPoly).Save(DataFolder + "Intersect3.bmp");

            TestHelper.CompareTwoBitmaps(new Bitmap(Expected + "Intersect3.bmp"), new Bitmap(DataFolder + "Intersect3.bmp"));
        }

        //Testfall 4: Gegenseitig dringt jeweils ein Eckpunkt des einen Polygon beim anderen ein
        [Fact]
        public void Intersect4()
        {
            var poly2 = new Vec2D[]
            {
                new Vec2D(740, 564),
                new Vec2D(760, 570),
                new Vec2D(753, 595),
                new Vec2D(733, 590),
            };

            var resultPoly = PolygonIntersector.GetIntersection(poly1b, poly2);
            GetTestOutputForPolys(poly1b, poly2, true, resultPoly).Save(DataFolder + "Intersect4.bmp");

            TestHelper.CompareTwoBitmaps(new Bitmap(Expected + "Intersect4.bmp"), new Bitmap(DataFolder + "Intersect4.bmp"));
        }

        //Testfall 5: Zwei Eckpunkte dringen in Polygon ein
        [Fact]
        public void Intersect5()
        {
            var poly2 = new Vec2D[]
            {
                new Vec2D(760, 570),
                new Vec2D(780, 575),
                new Vec2D(774, 600),
                new Vec2D(753, 595),
            };

            var resultPoly = PolygonIntersector.GetIntersection(poly1b, poly2);
            GetTestOutputForPolys(poly1b, poly2, true, resultPoly).Save(DataFolder + "Intersect5.bmp");

            TestHelper.CompareTwoBitmaps(new Bitmap(Expected + "Intersect5.bmp"), new Bitmap(DataFolder + "Intersect5.bmp"));
        }

        //Testfall 6: Ein BorderPoint-Ring
        [Fact]
        public void Intersect6()
        {
            var poly2 = new Vec2D[]
            {
                new Vec2D(843.1117f, 278.7365f),
                new Vec2D(837.7623f, 213.6567f),
                new Vec2D(874.0703f, 311.41f),               
            };

            var resultPoly = PolygonIntersector.GetIntersection(poly1a, poly2);
            GetTestOutputForPolys(poly1a, poly2, true, resultPoly).Save(DataFolder + "Intersect6.bmp");

            TestHelper.CompareTwoBitmaps(new Bitmap(Expected + "Intersect6.bmp"), new Bitmap(DataFolder + "Intersect6.bmp"));
        }

        //Testfall 7: Zwei BorderPoint-Ringe
        [Fact]
        public void Intersect7()
        {
            var poly2 = new Vec2D[]
            {
                new Vec2D(584.3443f, 792.2104f),
                new Vec2D(629.6007f, 626.2712f),
                new Vec2D(638.371f , 806.9451f),
            };

            var resultPoly = PolygonIntersector.GetIntersection(poly1b, poly2);
            GetTestOutputForPolys(poly1b, poly2, true, resultPoly).Save(DataFolder + "Intersect7.bmp");

            TestHelper.CompareTwoBitmaps(new Bitmap(Expected + "Intersect7.bmp"), new Bitmap(DataFolder + "Intersect7.bmp"));
        }

        //Testfall 8: Es wird auf ein BorderPoint gestartet, wo der nächste Polygon-Eckpunkt außerhalb liegt -> Rufe JumpToNextBorderPoint für den Start-BorderPoint auf
        [Fact]
        public void Intersect8()
        {
            var poly2 = new Vec2D[]
            {
                new Vec2D(861, 598),
                new Vec2D(881, 603),
                new Vec2D(874, 629),
                new Vec2D(854, 623)
            };

            var resultPoly = PolygonIntersector.GetIntersection(poly1b, poly2);
            GetTestOutputForPolys(poly1b, poly2, true, resultPoly).Save(DataFolder + "Intersect8.bmp");

            TestHelper.CompareTwoBitmaps(new Bitmap(Expected + "Intersect8.bmp"), new Bitmap(DataFolder + "Intersect8.bmp"));
        }

        //Testfall 9: Provoziert ein startPoint=null-Fehlerfall (Siehe PolygonIntersector.cs Zeile 98)
        [Fact]
        public void Intersect9()
        {
            var poly2 = new Vec2D[]
            {
                new Vec2D(796.108337f, 367.343323f),
                new Vec2D(811.555481f, 382.937073f),
                new Vec2D(791.362854f, 402.939758f),
                new Vec2D(775.91571f,  387.346008f),
            };

            var resultPoly = PolygonIntersector.GetIntersection(poly1a, poly2);
            GetTestOutputForPolys(poly1a, poly2, true, resultPoly).Save(DataFolder + "Intersect9.bmp");

            TestHelper.CompareTwoBitmaps(new Bitmap(Expected + "Intersect9.bmp"), new Bitmap(DataFolder + "Intersect9.bmp"));
        }

        //Testfall 10: Provoziert ein PolyCount=0-Fehlerfall (Siehe PolygonIntersector.cs Zeile 45)
        [Fact]
        public void Intersect10()
        {
            var poly2 = new Vec2D[]
            {
                new Vec2D(652.897156f, 704.844666f),
                new Vec2D(674.263611f, 710.671875f),
                new Vec2D(667.470032f, 735.58136f ),
                new Vec2D(646.103577f, 729.75415f ),
            };

            var resultPoly = PolygonIntersector.GetIntersection(poly1b, poly2);
            GetTestOutputForPolys(poly1b, poly2, true, resultPoly).Save(DataFolder + "Intersect10.bmp");

            TestHelper.CompareTwoBitmaps(new Bitmap(Expected + "Intersect10.bmp"), new Bitmap(DataFolder + "Intersect10.bmp"));
        }

        //Testfall 11: Dieser Testfall zeigt, warum in der Funktion PolyIntersections.GetIntersectionPointBetweenToLines das epsilon auf den Wert 0.0001f (oder größer) stehen muss
        [Fact ] 
        public void Intersect11()
        {
            var poly2 = new Vec2D[]
            {
                new Vec2D(843.22345f,  596.178467f),
                new Vec2D(864.589905f, 602.005676f),
                new Vec2D(857.796387f, 626.915161f),
                new Vec2D(836.429932f, 621.087952f),
            };

            var resultPoly = PolygonIntersector.GetIntersection(poly1b, poly2);
            GetTestOutputForPolys(poly1b, poly2, true, resultPoly).Save(DataFolder + "Intersect11.bmp");

            TestHelper.CompareTwoBitmaps(new Bitmap(Expected + "Intersect11.bmp"), new Bitmap(DataFolder + "Intersect11.bmp"));
        }

        //Erzeugt das Ergebnisbild
        private static Bitmap GetTestOutputForPolys(Vec2D[] poly1, Vec2D[] poly2, bool showIntersectionPixels, List<Vec2D[]> intersectionPolys)
        {
            int border = 10;
            var b = new Vec2D(border, border);
            var box1 = RigidBodyPhysics.MathHelper.BoundingBox.GetBoxFromPoints(poly1);
            var box2 = RigidBodyPhysics.MathHelper.BoundingBox.GetBoxFromPoints(poly2);
            var box = RigidBodyPhysics.MathHelper.BoundingBox.GetBoxFromBoxes(new RigidBodyPhysics.MathHelper.BoundingBox[] { box1, box2 });

            GraphicPanel2D panel = new GraphicPanel2D() { Width = (int)box.Width + border * 2, Height = (int)box.Height + border * 2, Mode = Mode2D.CPU };

            panel.ClearScreen(Color.White);

            if (showIntersectionPixels)
            {
                for (int x = 0; x < panel.Width; x++)
                    for (int y = 0; y < panel.Height; y++)
                    {
                        var point = new Vec2D(x + box.Min.X - border, y + box.Min.Y - border);
                        if (RigidBodyPhysics.MathHelper.PolygonHelper.PointIsInsidePolygon(poly1, point) && RigidBodyPhysics.MathHelper.PolygonHelper.PointIsInsidePolygon(poly2, point))
                        {
                            panel.DrawPixel(new GraphicMinimal.Vector2D(x - 1, y - 1), Color.Red, 1);
                        }
                    }
            }

            if (intersectionPolys != null)
            {
                foreach (var poly in intersectionPolys)
                {
                    panel.DrawFillPolygon(Color.Green, poly.Select(x => (x - box.Min + b).ToGrx()).ToList());
                }
            }                

            panel.DrawPolygon(Pens.Black, poly1.Select(x => (x - box.Min + b).ToGrx()).ToList());
            panel.DrawPolygon(Pens.Black, poly2.Select(x => (x - box.Min + b).ToGrx()).ToList());
            panel.FlipBuffer();
            return panel.GetScreenShoot();
        }
    }
}
