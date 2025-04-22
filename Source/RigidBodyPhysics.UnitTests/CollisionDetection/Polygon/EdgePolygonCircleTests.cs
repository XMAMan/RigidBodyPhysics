using FluentAssertions;
using RigidBodyPhysics.UnitTests.TestHelper;
using Xunit;

namespace RigidBodyPhysics.UnitTests.CollisionDetection.Polygon
{
    public class EdgePolygonCircleTests
    {
        private static string TestData = @"..\..\..\..\..\Data\TestData\RigidBodyPhysicsTestData\CollisionDetectionTestcases\Polygon\EdgePolygonCircle\";

        //Kreis schneidet Konvex-Punkt von InsidePolygon -> Die beiden Kollisionsnormalen zeigen nach oben
        [Fact]
        public void InsidePolygonCircle2Collision_FoundTwoCollisionPoints()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "InsidePolygonCircle2Collision.txt").Bodies.ToList();
            var c = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es zwei Kollisionen gab
            c.Should().NotBeNull();

            c[0].Start.ToIntString().Should().Be("729_489");
            c[0].End.ToIntString().Should().Be("774_472");

            c[1].Start.ToIntString().Should().Be("837_479");
            c[1].End.ToIntString().Should().Be("794_472");
        }

        //Kreis liegt innerhalb vom InsidePolygon und berührt nicht den Rand -> Es gibt keine Kollision
        [Fact]
        public void InsidePolygonCircleNoCollision_FoundNoCollision()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "InsidePolygonCircleNoCollision.txt").Bodies.ToList();
            var c = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es keine Kollision gab
            c.Length.Should().Be(0);
        }

        //Kreis durchschneidet bei Outsidepolygon zwei Kanten aber nur die obere Kante erzeugt Kollisionspunkt, da nur dort
        //das Kreiszentrum vor der Kante liegt
        [Fact]
        public void OutsidePolygonCircle2Collision_FoundOneCollision()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "OutsidePolygonCircle2Collision.txt").Bodies.ToList();
            var c = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es eine Kollision gab
            c.Should().NotBeNull();

            c[0].Start.ToIntString().Should().Be("1039_313");
            c[0].End.ToIntString().Should().Be("1039_283");
        }

        //Kreis liegt über den Outsidepolygon -> Es gibt keine Kollision
        [Fact]
        public void OutsidePolygonCircleNoCollision_FoundNoCollision()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "OutsidePolygonCircleNoCollision.txt").Bodies.ToList();
            var c = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es keine Kollision gab
            c.Length.Should().Be(0);
        }
    }
}
