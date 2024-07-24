using FluentAssertions;
using RigidBodyPhysics.UnitTests.TestHelper;
using Xunit;

namespace RigidBodyPhysics.UnitTests.CollisionDetection.Polygon
{
    public class EdgePolygonRectangleTests
    {
        private static string TestData = @"..\..\..\..\..\Data\TestData\RigidBodyPhysicsTestData\CollisionDetectionTestcases\Polygon\EdgePolygonRectangle\";

        private static float TimeStepTickRate = 50; //[ms]

        //Rechteck liegt mit Kante auf Bodenhuckel -> Es gibt Kollision zwei Kollisionspunkte
        [Fact]
        public void InsideConvexPolygonRectangel2Collision_FoundTwoCollisionPoints()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "InsideConvexPolygonRectangel2Collision.txt").Bodies.ToList();
            var c = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es zwei Kollisionen gab
            c.Should().NotBeNull();

            c[0].Start.ToIntString().Should().Be("842_655"); //Schnittpunkte Bodenkante - Rechteckpunkt
            c[0].End.ToIntString().Should().Be("842_655");

            c[1].Start.ToIntString().Should().Be("600_596"); //Schnittpunkt mit Bodenhuckel - Rechteckkante
            c[1].End.ToIntString().Should().Be("600_595");
        }

        //Rechteckkante liegt im Bereich zwishen zwei Edge-Kanten -> Es muss trotzdem Schnittpunkte geben
        [Fact]
        public void InsidePolygonRectangel2Collision_FoundTwoCollisionPoints()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "InsidePolygonRectangel2Collision.txt").Bodies.ToList();
            var c = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es vier Kollisionen gab
            c.Should().NotBeNull();

            c[0].Start.ToIntString().Should().Be("724_449");
            c[0].End.ToIntString().Should().Be("724_448");

            c[1].Start.ToIntString().Should().Be("548_535");
            c[1].End.ToIntString().Should().Be("540_513");

            c[2].Start.ToIntString().Should().Be("534_507");
            c[2].End.ToIntString().Should().Be("534_507");

            c[3].Start.ToIntString().Should().Be("548_535");
            c[3].End.ToIntString().Should().Be("556_513");
        }

        //Rechteck liegt mit linker unterer Ecke und unterer Kante auf Outside-Polygon -> Es gibt Schnittpunkt mit Rechteckpunkt und 
        //Rechteckkante
        [Fact]
        public void OutsidePolygonRectangle2Collision_FoundTwoCollisionPoints()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "OutsidePolygonRectangle2Collision.txt").Bodies.ToList();
            var c = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es zwei Kollisionen gab
            c.Should().NotBeNull();

            c[0].Start.ToIntString().Should().Be("696_340"); //Linker unterer Rechteckpunkt
            c[0].End.ToIntString().Should().Be("696_339");

            c[1].Start.ToIntString().Should().Be("895_340"); //Untere Kante vom Rechteck
            c[1].End.ToIntString().Should().Be("895_339");
        }

        //Rechteck rutscht Ski-Schanze runter -> Rechteck fliegt über die Schanze hinaus
        [Fact]
        public void OutsidePolygonRectangleSliding_FoundNoCollisionAfterSliding()
        {
            int rectangle = 1;
            var result = SimulateSeveralTimeSteps.DoTest(TestData + "OutsidePolygonRectangleSliding.txt", TimeStepTickRate, true, 50, 100);

            float recPosition = result.TimeSteps.Last().Bodies[rectangle].Position.X;
            recPosition.Should().BeGreaterThan(1500); //Rechteck soll über die rechte Kante von Schanze gesprungen sein
        }

        //Rechteck befindet sich hinter der Schanze -> Keine Kollision
        [Fact]
        public void OutsidePolygonSkiJump_FoundNoCollision()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "OutsidePolygonSkiJump.txt").Bodies.ToList();
            var c = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es keine Kollisionen gab
            c.Length.Should().Be(0);
        }

        //Rechteck befindet sich hinter der über dem Mond -> Keine Kollision        
        [Fact]
        public void Moonlander_FoundNoCollision()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "Moonlander.txt").Bodies.ToList();
            var c = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es keine Kollisionen gab
            c.Length.Should().Be(0);
        }
    }
}
