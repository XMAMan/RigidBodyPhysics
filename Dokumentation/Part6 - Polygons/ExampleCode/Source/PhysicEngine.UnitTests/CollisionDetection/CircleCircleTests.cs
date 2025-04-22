using FluentAssertions;
using PhysicEngine.UnitTests.TestHelper;
using Xunit;

namespace PhysicEngine.UnitTests.CollisionDetection
{
    public class CircleCircleTests
    {
        private static string TestData = @"..\..\..\..\..\Data\CollisionDetectionTestcases\";

        [Fact]
        public void Overlapping_FoundsCollisionPoint()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "CircleCircleOverlapping.txt").Bodies.ToList();
            var collisions = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es genau eine Kollision gab
            collisions.Should().HaveCount(1);
            var c = collisions[0];

            //Prüfe dass Y-Wert vom Kollisionspunkt gleich den Center-Y-Werten der Kreise entspricht
            c.Start.Y.Should().Be(c.End.Y);
            c.Start.Y.Should().Be(c.B1.Center.Y);
            c.End.Y.Should().Be(c.B2.Center.Y);

            //Start muss auf dem rechten Rand von Kreis 1 liegen
            c.Start.X.Should().Be(c.B1.Center.X + c.B1.Radius);

            //End liegt nicht auf den linken Rand von Kreis 2 da der End-Punkt immer vom Startpunkt den Abstand CollisionDepth hat
            //c.End.X.Should().Be(c.B2.Center.X - c.B2.Radius); 

            //Die Eindringtiefe ist die Differenz zwischen den Center-Abstand und der Radiussumme
            c.Depth.Should().Be(c.B1.Radius + c.B2.Radius - (c.B2.Center.X - c.B1.Center.X));

            //Normale muss nach rechts zeigen da B1.X < B2.X ist
            c.Normal.X.Should().Be(1);
        }

        [Fact]
        public void SamePosition_FoundsCollisionPoint()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "CircleCircleSamePosition.txt").Bodies.ToList(); 
            var collisions = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es genau eine Kollision gab
            collisions.Should().HaveCount(1);
            var c = collisions[0];

            //Prüfe dass Y-Wert vom Kollisionspunkt gleich den Center-Y-Werten der Kreise entspricht
            c.Start.Y.Should().Be(c.End.Y);
            c.Start.Y.Should().Be(c.B1.Center.Y);
            c.End.Y.Should().Be(c.B2.Center.Y);

            //Start muss auf dem rechten Rand von Kreis 1 liegen
            c.Start.X.Should().Be(c.B1.Center.X + c.B1.Radius);

            //End liegt nicht auf den linken Rand von Kreis 2 da der End-Punkt immer vom Startpunkt den Abstand CollisionDepth hat
            //c.End.X.Should().Be(c.B2.Center.X - c.B2.Radius); 

            //Die Eindringtiefe ist die Differenz zwischen den Center-Abstand und der Radiussumme
            c.Depth.Should().Be(c.B1.Radius + c.B2.Radius - (c.B2.Center.X - c.B1.Center.X));

            //Normale muss nach rechts zeigen da B1.X < B2.X ist
            c.Normal.X.Should().Be(1);
        }

        [Fact]
        public void NoOverlapping_FoundNoCollisionPoint()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "CircleCircleNoCollision.txt").Bodies.ToList();
            var collisions = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es keine Kollision gab
            collisions.Should().HaveCount(0);
        }
    }
}
