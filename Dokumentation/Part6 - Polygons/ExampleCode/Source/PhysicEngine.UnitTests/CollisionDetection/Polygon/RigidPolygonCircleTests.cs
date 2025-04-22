using FluentAssertions;
using PhysicEngine.UnitTests.TestHelper;
using Xunit;

namespace PhysicEngine.UnitTests.CollisionDetection.Polygon
{
    public class RigidPolygonCircleTests
    {
        private static string TestData = @"..\..\..\..\..\Data\CollisionDetectionTestcases\Polygon\RigidPolygonCircle\";

        //Kreis kollidiert mit Spitze von RigidPolygon -> Kreis wird nach oben gedrückt
        [Fact]
        public void RigidPolygonCircle1Corner1Collision_FoundOneCollisionPoint()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "RigidPolygonCircle1Corner1Collision.txt").Bodies.ToList();
            var c = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es eine Kollision gab
            c.Should().NotBeNull();

            c[0].Start.ToIntString().Should().Be("925_464");
            c[0].End.ToIntString().Should().Be("955_439");
        }

        //Kreis kollidiert mit Spitze von RigidPolygon -> Kreis wird nach unten gedrückt
        [Fact]
        public void RigidPolygonCircle1Corner2Collision_FoundOneCollisionPoint()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "RigidPolygonCircle1Corner2Collision.txt").Bodies.ToList();
            var c = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es eine Kollision gab
            c.Should().NotBeNull();

            c[0].Start.ToIntString().Should().Be("916_421");
            c[0].End.ToIntString().Should().Be("955_439");
        }

        //Kreis kollidiert mit Kante von RigidPolygon. Kreiszentrum liegt außerhalb vom Polygon -> Kreis wird von Kante weggedrückt
        [Fact]
        public void RigidPolygonCircle1FaceCollision_FoundOneCollisionPoint()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "RigidPolygonCircle1FaceCollision.txt").Bodies.ToList();
            var c = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es eine Kollision gab
            c.Should().NotBeNull();

            c[0].Start.ToIntString().Should().Be("746_401");
            c[0].End.ToIntString().Should().Be("756_372");
        }

        //Kreis kollidiert mit Kante von RigidPolygon. Kreiszentrum liegt innerhalb vom Polygon -> Kreis wird von Kante weggedrückt
        [Fact]
        public void RigidPolygonCircle1InsideCollision_FoundOneCollisionPoint()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "RigidPolygonCircle1InsideCollision.txt").Bodies.ToList();
            var c = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es eine Kollision gab
            c.Should().NotBeNull();

            c[0].Start.ToIntString().Should().Be("699_467");
            c[0].End.ToIntString().Should().Be("734_364");
        }

        //Kreis liegt neben RigidPolygon -> Es gibt keine Kollision
        [Fact]
        public void RigidPolygonCircleNoCollision_FoundNoCollisionPoint()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "RigidPolygonCircleNoCollision.txt").Bodies.ToList();
            var c = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es keine Kollision gab
            c.Length.Should().Be(0);
        }
    }
}
