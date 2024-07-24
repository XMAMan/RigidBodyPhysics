using FluentAssertions;
using PhysicEngine.CollisionDetection;
using PhysicEngine.CollisionDetection.NearPhase;
using Xunit;

//Hinweis zum Anlegen einer Test-Bibliothek
//-Als Projektvorlage habe ich eine .NET 7.0 Klassenbibliothek erstellt
//-Dann als NuGet xunit und FluentAssertions hinzugefügt
//-Per Hand in der PhysicEngine.UnitTests.csproj-Datei dann noch
//<PackageReference Include="Microsoft.NET.Test.Sdk" Version="16.9.1" />
//<PackageReference Include="xunit.runner.visualstudio" Version="2.4.2" />
//hinzugefügt. Könnte sein, das man das auch über NuGet machen darf/soll. Hinweis dazu: https://www.c-sharpcorner.com/article/how-to-resolve-issue-of-test-project-not-running-the-unit-tests-after-upgrade-to/


namespace PhysicEngine.UnitTests.CollisionDetection
{
    public class RectangleRectangleTests
    {
        private static string TestData = @"..\..\..\..\..\Data\CollisionDetectionTestcases\";

        [Fact]
        public void ThereIsASeparatingAxis_FoundNoCollisionPoint()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "RectangleRectangleNoCollision.txt");
            var c = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es keine Kollision gab
            c.Length.Should().Be(0);    
        }

        [Fact]
        public void CollisionOnTopEdge_FoundOneCollisionPoint()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "RectangleRectangleCollisionOnTopEdge.txt");
            var c = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es eine Kollision gab
            c.Should().NotBeNull();

            var b1 = bodys[1] as ICollidableRectangle;
            c[0].Start.X.Should().Be(b1.Vertex[2].X);
            c[0].Start.Y.Should().Be(b1.Vertex[2].Y);
        }


        [Fact]
        public void CollisionOnTopAndRightEdgeWithLongRec_FoundOneCollisionPoint()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "RectangleRectangleCollisionOnTopAndRightEdgeWithLongRec.txt");
            var c = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es eine Kollision gab
            c.Should().NotBeNull();

            var b1 = bodys[0] as ICollidableRectangle;
            c[0].End.X.Should().Be(b1.Vertex[1].X);
            c[0].End.Y.Should().Be(b1.Vertex[1].Y);
        }

        [Fact]
        public void CollisionOnTopAndRightEdge_FoundTwoCollisionPoint()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "RectangleRectangleCollisionOnTopAndRightEdge.txt");
            var c = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es eine Kollision gab
            c.Should().NotBeNull();

            var b1 = bodys[0] as ICollidableRectangle;
            var b2 = bodys[1] as ICollidableRectangle;
            c[1].End.X.Should().Be(b1.Vertex[1].X);         //Rechte obere Ecke vom Tisch
            c[1].End.Y.Should().Be(b1.Vertex[1].Y);
            c[0].Start.X.Should().Be(b2.Vertex[3].X);       //Linke untere Ecke vom Würfel
            c[0].Start.Y.Should().Be(b2.Vertex[3].Y);
        }

        [Fact]
        public void CubeOnTable_FoundTwoCollisionPoints()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "RectangleRectangleCubeOnTableCollision.txt");
            var c = CollisionHelper.GetAllCollisions(bodys);

            //Ich erwarte das die beiden unteren Eckpunkte vom Würfel die Kontaktpunkte sind
            c.Should().NotBeNull();

            var b2 = bodys[1] as ICollidableRectangle;
            c[0].Start.X.Should().Be(b2.Vertex[2].X);
            c[0].Start.Y.Should().Be(b2.Vertex[2].Y);

            c[1].Start.X.Should().Be(b2.Vertex[3].X);
            c[1].Start.Y.Should().Be(b2.Vertex[3].Y);
        }

        [Fact]
        public void CubeOnTableCorner_FoundTwoCollisionPoints()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "RectangleRectangleCubeOnTableCornerCollision.txt");
            var c = CollisionHelper.GetAllCollisions(bodys);

            c.Should().NotBeNull();

            var b1 = bodys[0] as ICollidableRectangle;
            var b2 = bodys[1] as ICollidableRectangle;
            c[1].End.X.Should().Be(b1.Vertex[1].X);         //Rechte obere Ecke vom Tisch
            c[1].End.Y.Should().Be(b1.Vertex[1].Y);
            c[0].Start.X.Should().Be(b2.Vertex[3].X);       //Linke untere Ecke vom Würfel
            c[0].Start.Y.Should().Be(b2.Vertex[3].Y);
        }

        [Fact]
        public void CubeStackOnTable_FoundFourCollisionPoints()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "RectangleRectangleCubeStackOnTableCollision.txt");

            var c1 = RectangleRectangleCollision.RectangleRectangle(bodys[0] as ICollidableRectangle, bodys[1] as ICollidableRectangle); //Tisch-Unterer Würfel

            var b2 = bodys[1] as ICollidableRectangle;
            c1[0].Start.X.Should().Be(b2.Vertex[2].X);
            c1[0].Start.Y.Should().Be(b2.Vertex[2].Y);

            c1[1].Start.X.Should().Be(b2.Vertex[3].X);
            c1[1].Start.Y.Should().Be(b2.Vertex[3].Y);

            //...
            var c2 = RectangleRectangleCollision.RectangleRectangle(bodys[1] as ICollidableRectangle, bodys[2] as ICollidableRectangle); //Unterer Würfel - Oberer Würfel

            var b3 = bodys[2] as ICollidableRectangle;
            c2[1].End.X.Should().Be(b2.Vertex[0].X);
            c2[1].End.Y.Should().Be(b2.Vertex[0].Y);
            c2[0].End.X.Should().Be(b2.Vertex[1].X);
            c2[0].End.Y.Should().Be(b2.Vertex[1].Y);

            c2[0].Start.X.Should().Be(b3.Vertex[2].X);
            c2[0].Start.Y.Should().Be(b3.Vertex[2].Y);
            c2[1].Start.X.Should().Be(b3.Vertex[3].X);
            c2[1].Start.Y.Should().Be(b3.Vertex[3].Y);
        }

        [Fact]
        public void CippedCube_FoundTwoCollisionPoints()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "RectangleRectangleCippedCube.txt");
            var c = CollisionHelper.GetAllCollisions(bodys);

            c.Should().NotBeNull();

            var b1 = bodys[0] as ICollidableRectangle;
            var b2 = bodys[1] as ICollidableRectangle;
            c[0].Start.X.Should().Be(b2.Vertex[2].X);       //Rechte untere Ecke vom Würfel
            c[0].Start.Y.Should().Be(b2.Vertex[2].Y);

            c[1].Start.X.Should().Be(b2.Vertex[3].X);       //Linke untere Ecke vom Würfel
            c[1].Start.Y.Should().Be(b2.Vertex[3].Y);
        }
    }
}
