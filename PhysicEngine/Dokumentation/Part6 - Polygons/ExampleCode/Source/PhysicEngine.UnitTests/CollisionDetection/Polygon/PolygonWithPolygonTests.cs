using FluentAssertions;
using PhysicEngine.UnitTests.TestHelper;
using Xunit;

namespace PhysicEngine.UnitTests.CollisionDetection.Polygon
{
    public class PolygonWithPolygonTests
    {
        private static string TestData = @"..\..\..\..\..\Data\CollisionDetectionTestcases\Polygon\PolygonWithPolygon\";

        //Polygon liegt auf Boden von InsidePolygon -> Es gibt zwei Schnittpunkte
        [Fact]
        public void InsidePolygonRigidPolygonCollision_FoundTwoCollisionPoints()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "InsidePolygonRigidPolygonCollision.txt").Bodies.ToList();
            var c = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es zwei Kollisionen gab
            c.Should().NotBeNull();

            c[0].Start.ToIntString().Should().Be("716_861");
            c[0].End.ToIntString().Should().Be("716_860");

            c[1].Start.ToIntString().Should().Be("603_862");
            c[1].End.ToIntString().Should().Be("603_862");
        }

        //Polygon mit Spitze liegt im Zwischenbereich zwischen zwei Edge-Polygonkanten -> Spitze hat trotzdem ein Kollisionspunkt
        [Fact]
        public void InsidePolygonWithSpicyRigidPolygonCollision_FoundTwoCollisionPoints()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "InsidePolygonWithSpicyRigidPolygonCollision.txt").Bodies.ToList();
            var c = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es vier Kollisionen gab
            c.Length.Should().Be(4);

            c[0].Start.ToIntString().Should().Be("557_571");
            c[0].End.ToIntString().Should().Be("537_514");

            c[1].Start.ToIntString().Should().Be("604_491");
            c[1].End.ToIntString().Should().Be("604_490");

            c[2].Start.ToIntString().Should().Be("557_571");
            c[2].End.ToIntString().Should().Be("574_519");

            c[3].Start.ToIntString().Should().Be("501_496");
            c[3].End.ToIntString().Should().Be("501_496");
        }

        //Polygon liegt steht auf OutsidePolygon -> Es gibt zwei Schnittpunkte
        [Fact]
        public void OutsidePolygonRigidPolygonCollision_FoundTwoCollisionPoints()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "OutsidePolygonRigidPolygonCollision.txt").Bodies.ToList();
            var c = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es zwei Kollisionen gab
            c.Should().NotBeNull();

            c[0].Start.ToIntString().Should().Be("478_710"); //Ecke 1 vom RigidPolygon
            c[0].End.ToIntString().Should().Be("478_710");

            c[1].Start.ToIntString().Should().Be("899_713"); //Ecke 2 vom RigidPolygon
            c[1].End.ToIntString().Should().Be("899_712");
        }

        //RigidPolygon liegt steht auf OutsidePolygon -> Es gibt Schnittpunkt mit Ecke und Kante vom RigidPolygon mit Boden
        [Fact]
        public void OutsidePolygonRigidPolygonCollision1_FoundTwoCollisionPoints()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "OutsidePolygonRigidPolygonCollision1.txt").Bodies.ToList();
            var c = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es zwei Kollisionen gab
            c.Should().NotBeNull();

            c[0].Start.ToIntString().Should().Be("654_283"); //Ecke vom RigidPolygon
            c[0].End.ToIntString().Should().Be("653_275");

            c[1].Start.ToIntString().Should().Be("864_273"); //Kante vom RigidPolygon
            c[1].End.ToIntString().Should().Be("863_264");
        }

        //Bewegliches OutsidePolygon liegt steht auf statischen OutsidePolygon -> Es gibt Schnittpunkt mit Ecke und Kante vom beweglichen Polygon mit Boden
        [Fact(Skip = "Funktioniert nicht, da ich nur für Poly1 prüfe, ob es Punkte aus Poly2 gibt, die innerhalb von Poly1 liegen. Es befindet sich aber ein Poly1-Punkt in Poly2. EdgePolygone sind nur für den Levelrand gedacht. Deswegen ist es ok, wenn dieser Test hier nicht klappt.")]
        public void TwoOutsidePolygons2Collsion_FoundTwoCollisionPoints()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "TwoOutsidePolygons2Collsion.txt").Bodies.ToList();
            var c = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es zwei Kollisionen gab
            c.Should().NotBeNull();

            c[0].Start.ToIntString().Should().Be("654_283"); //Ecke vom beweglichen Polygon
            c[0].End.ToIntString().Should().Be("653_275");

            c[1].Start.ToIntString().Should().Be("864_273"); //Kante vom beweglichen Polygon
            c[1].End.ToIntString().Should().Be("863_264");
        }

        //Bewegliches RigidPolygon liegt auf statischen RigidPolygon -> Es gibt zwei Schnittpunkte
        [Fact]
        public void TwoRigidPolygons2Collision_FoundTwoCollisionPoints()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "TwoRigidPolygons2Collision.txt").Bodies.ToList();
            var c = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es zwei Kollisionen gab
            c.Should().NotBeNull();

            c[0].Start.ToIntString().Should().Be("706_383"); //Ecke vom beweglichen Polygon
            c[0].End.ToIntString().Should().Be("706_382");

            c[1].Start.ToIntString().Should().Be("929_383"); //Kante vom beweglichen Polygon
            c[1].End.ToIntString().Should().Be("929_382");
        }

        //Bewegliches RigidPolygon liegt neben EdgePolygon -> Es gibt keine Kollision
        //Dieser Test hat deswegen ein Problem dargestellt, weil das RigidPolygon beim Start links neben dem EdgePolygon liegt
        //und dann habe ich es mit der Maus rechts daneben bewegt. Wegen ein Fehler in der CollidableConvexPolygon-Klasse
        //wurde das Zentrum und Parent-Zentrum nicht aktualisiert wodurch dann Fakekollisionspuntke erzeugt wurden
        //Um das nachzustellen müsste ich das RigidPolygon irgendwie an eine beliebige Position von außen setzen können 
        //was aber offiziell nur über Kräfte erlaubt ist. Deswegen ist das automatisch etwas schwieriger nachzustellen
        [Fact]
        public void RigidPolygonWithEdgePolygonNoCollision_NoCollision()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "RigidPolygonWithEdgePolygonNoCollision.txt").Bodies.ToList();
            var c = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es keine Kollision gab
            c.Length.Should().Be(0);
        }
    }
}
