using FluentAssertions;
using PhysicEngine.CollisionDetection;
using PhysicEngine.CollisionDetection.NearPhase;
using PhysicEngine.RigidBody;
using Xunit;

namespace PhysicEngine.UnitTests.CollisionDetection
{
    //Hiermit prüfe ich die Kollisionsabfragen von Box2D-Light. Aktuell hat es noch viele Fehler die im Originalode so drin sind
    public class RectangleRectangleBox2DLightTests
    {
        private static string TestData = @"..\..\..\..\..\Data\CollisionDetectionTestcases\";

        //Box2D-Light findet zwei Kontaktpunkte obwohl sich die Rechtecke nicht berühren. Warum?
        [Fact]
        public void ThereIsASeparatingAxis_FoundNoCollisionPoint()
        {
            var bodys = JsonHelper.ReadFromFile(TestData + "RectangleRectangleNoCollision.txt");
            var c = RectangleRectangleCollision3.RectangleRectangle(bodys[0] as ICollidableRectangle, bodys[1] as ICollidableRectangle);

            //Prüfe das es keine Kollision gab
            c.Should().BeNull();
        }

        //Box2D-Light sagt das der eine Kontaktpunkte nicht auf der linken unteren Ecke vom oberen Würfel liegt. Warum?
        [Fact]
        public void CollisionOnTopAndRightEdge_Box2DLight_FoundTwoCollisionPoint()
        {
            var bodys = JsonHelper.ReadFromFile(TestData + "RectangleRectangleCollisionOnTopAndRightEdge.txt");
            var c = RectangleRectangleCollision3.RectangleRectangle(bodys[0] as ICollidableRectangle, bodys[1] as ICollidableRectangle);

            //Prüfe das es eine Kollision gab
            c.Should().NotBeNull();

            var b1 = bodys[0] as ICollidableRectangle;
            var b2 = bodys[1] as ICollidableRectangle;
            c[1].End.X.Should().Be(b1.Vertex[1].X);         //Rechte obere Ecke vom Tisch
            c[1].End.Y.Should().Be(b1.Vertex[1].Y);
            c[0].Start.X.Should().Be(b2.Vertex[3].X);       //Linke untere Ecke vom Würfel
            c[0].Start.Y.Should().Be(b2.Vertex[3].Y);
        }
    }
}
