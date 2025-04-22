using FluentAssertions;
using RigidBodyPhysics.UnitTests.TestHelper;
using Xunit;

namespace RigidBodyPhysics.UnitTests.CollisionDetection.Polygon
{
    public class RigidPolygonRectangleTests
    {
        private static string TestData = @"..\..\..\..\..\Data\TestData\RigidBodyPhysicsTestData\CollisionDetectionTestcases\Polygon\RigidPolygonRectangle\";
        private static string TestResults = @"..\..\..\..\..\Data\TestData\RigidBodyPhysicsTestData\TestResults\";

        //Rechteck liegt auf Polygon -> Die beiden unteren Rechteckpunkte erzeugen jeweils ein Kollisionspunkt
        [Fact]
        public void ConcavePolygonRectangleCollision_FoundTwoCollisionPoints()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "ConcavePolygonRectangleCollision.txt").Bodies.ToList();
            var c = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es zwei Kollisionen gab
            c.Should().NotBeNull();

            c[0].Start.ToIntString().Should().Be("353_295"); //Untere linke Ecke vom Rechteck
            c[0].End.ToIntString().Should().Be("353_294");

            c[1].Start.ToIntString().Should().Be("585_291"); //Untere rechte Ecke vom Rechteck
            c[1].End.ToIntString().Should().Be("585_290");
        }

        //Konkaves Polygon steht auf Rechteck (Statisch) -> Für zwei Polygonpunkte gibt es jeweils eine Kollision
        [Fact]
        public void RectangleConcavePolygonCollision_FoundTwoCollisionPoints()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "RectangleConcavePolygonCollision.txt").Bodies.ToList();
            var c = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es zwei Kollisionen gab
            c.Should().NotBeNull();

            c[0].Start.ToIntString().Should().Be("631_672"); //Ecke 1 vom Polygon
            c[0].End.ToIntString().Should().Be("631_672");

            c[1].Start.ToIntString().Should().Be("775_673"); //Ecke 2 vom Polygon
            c[1].End.ToIntString().Should().Be("775_672");
        }

        //Polygon steht auf statischen Rechteck -> Für eine Polygonecke + Polygonkante gibt es eine Kollision
        [Fact]
        public void RectanglePolyRecCollision_FoundTwoCollisionPoints()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "RectanglePolyRecCollision.txt").Bodies.ToList();
            var c = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es zwei Kollisionen gab
            c.Should().NotBeNull();

            c[0].Start.ToIntString().Should().Be("956_547"); //Kante vom Polygon
            c[0].End.ToIntString().Should().Be("956_543");

            c[1].Start.ToIntString().Should().Be("676_546"); //Ecke vom Polygon
            c[1].End.ToIntString().Should().Be("676_543");
        }

        //Rechteck liegt auf statischen Polygon -> Für eine Rechteckecke + Rechteckaknte gibt es eine Kollision
        [Fact]
        public void RigidPolygonRectangle1Collision_FoundTwoCollisionPoints()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "RigidPolygonRectangle1Collision.txt").Bodies.ToList();
            var c = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es zwei Kollisionen gab
            c.Should().NotBeNull();

            c[0].Start.ToIntString().Should().Be("701_347"); //Kante vom Rechteck
            c[0].End.ToIntString().Should().Be("701_346");

            c[1].Start.ToIntString().Should().Be("846_346"); //Ecke vom Rechteck
            c[1].End.ToIntString().Should().Be("846_346");
        }
    }
}
