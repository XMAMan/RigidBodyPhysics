using PhysicEngine.MathHelper;
using System.Drawing;
using Xunit;

namespace PhysicEngine.UnitTests.PolygonHelper
{
    public class PointIsInsidePolygonTest
    {
        private static string TestResults = @"..\..\..\..\..\Data\TestResults\";

        [Fact]
        public void CheckInside_CheckForAllPoints()
        {
            Vec2D[] points = new Vec2D[]
               {
                new Vec2D(491, 163),
                new Vec2D(651, 301),
                new Vec2D(511, 432),
                new Vec2D(366, 346),
                new Vec2D(523, 343),
                new Vec2D(350, 252),
                new Vec2D(483, 242)
            };

            Vec2D min = new Vec2D(points.Min(x => x.X), points.Min(y => y.Y));
            Vec2D max = new Vec2D(points.Max(x => x.X), points.Max(y => y.Y));

            int border = 10;
            min -= new Vec2D(border, border);
            max += new Vec2D(border, border);

            Bitmap bitmap = new Bitmap((int)(max.X - min.X), (int)(max.Y - min.Y)); 
            for (int x=0;x< bitmap.Width;x++)
                for (int y=0;y< bitmap.Height;y++)
                {
                    var p = min + new Vec2D(x, y);
                    bool isInside = MathHelper.PolygonHelper.PointIsInsidePolygon(points, p);
                    bitmap.SetPixel(x, y, isInside ? Color.Red : Color.White);
                }

            bitmap.Save(TestResults + "PolygonPointIsInside.bmp");
        }

        [Fact]
        public void CheckInside_PointIsLeftFromConvexPolygon_y_is_242_ReturnsFalse()
        {
            Vec2D[] points = new Vec2D[]
            {
                new Vec2D(491, 163),
                new Vec2D(651, 301),
                new Vec2D(511, 432),
                new Vec2D(366, 346),
                new Vec2D(523, 343),
                new Vec2D(350, 252),
                new Vec2D(483, 242)
            };

            bool isInside = MathHelper.PolygonHelper.PointIsInsidePolygon(points, new Vec2D(300, 242));

            Assert.False(isInside);
        }

        [Fact]
        public void CheckInside_PointIsLeftFromConvexPolygon_y_is_243_ReturnsFalse()
        {
            Vec2D[] points = new Vec2D[]
            {
                new Vec2D(491, 163),
                new Vec2D(651, 301),
                new Vec2D(511, 432),
                new Vec2D(366, 346),
                new Vec2D(523, 343),
                new Vec2D(350, 252),
                new Vec2D(483, 242)
            };

            bool isInside = MathHelper.PolygonHelper.PointIsInsidePolygon(points, new Vec2D(300, 242.1f));

            Assert.False(isInside);
        }
    }
}
