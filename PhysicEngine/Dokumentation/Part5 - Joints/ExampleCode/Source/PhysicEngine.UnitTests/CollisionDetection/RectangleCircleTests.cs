using FluentAssertions;
using PhysicEngine.CollisionDetection;
using PhysicEngine.CollisionDetection.NearPhase;
using PhysicEngine.RigidBody;
using PhysicEngine.UnitTests.TestHelper;
using Xunit;

namespace PhysicEngine.UnitTests.CollisionDetection
{
    public class RectangleCircleTests
    {
        private static string TestData = @"..\..\..\..\..\Data\CollisionDetectionTestcases\";

        [Fact]
        public void CircleCenterIsInside_FoundCollisionPoint()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "RectangleCircleCenterIsInside.txt").Bodies.ToList();
            var collisions = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es genau eine Kollision gab
            collisions.Should().HaveCount(1);
            var c = collisions[0];

            float recRightBorder = c.B1.Center.X + (c.B1 as RigidRectangle).Size.X / 2;
            float circleLeftBorder = c.B2.Center.X - c.B2.Radius;

            c.Start.X.Should().Be(circleLeftBorder);
            c.Start.Y.Should().Be(c.B2.Center.Y);

            c.Normal.X.Should().Be(1);

            c.Depth.Should().Be(recRightBorder - circleLeftBorder);
        }

        [Fact]
        public void CircleHitCorner1_FoundCollisionPoint()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "RectangleCircleCorner1Collision.txt").Bodies.ToList();
            var collisions = CollisionHelper.GetAllCollisions(bodys); ;

            //Prüfe das es genau eine Kollision gab
            collisions.Should().HaveCount(1);
            var c = collisions[0];

            float recLeftBorder = c.B1.Center.X - (c.B1 as RigidRectangle).Size.X / 2;
            float recTopBorder = c.B1.Center.Y - (c.B1 as RigidRectangle).Size.Y / 2;

            c.End.X.Should().Be(recLeftBorder);
            c.End.Y.Should().Be(recTopBorder);

            //Da der Kreis rechts oben ist erwarte ich, dass die Normale ihn nach rechts oben wegdrückt
            c.Normal.X.Should().BeLessThan(0);
            c.Normal.Y.Should().BeLessThan(0);
        }

        [Fact]
        public void CircleHitCorner2_FoundCollisionPoint()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "RectangleCircleCorner2Collision.txt").Bodies.ToList();
            var collisions = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es genau eine Kollision gab
            collisions.Should().HaveCount(1);
            var c = collisions[0];

            float recRightBorder = c.B1.Center.X + (c.B1 as RigidRectangle).Size.X / 2;
            float recTopBorder = c.B1.Center.Y - (c.B1 as RigidRectangle).Size.Y / 2;

            c.End.X.Should().Be(recRightBorder);
            c.End.Y.Should().Be(recTopBorder);

            //Da der Kreis rechts oben ist erwarte ich, dass die Normale ihn nach rechts oben wegdrückt
            c.Normal.X.Should().BeGreaterThan(0);
            c.Normal.Y.Should().BeLessThan(0);
        }

        [Fact]
        public void CircleHitRightFace_FoundCollisionPoint()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "RectangleCircleFaceCollision.txt").Bodies.ToList();
            var collisions = CollisionHelper.GetAllCollisions(bodys);

            //Prüfe das es genau eine Kollision gab
            collisions.Should().HaveCount(1);
            var c = collisions[0];

            float recRightBorder = c.B1.Center.X + (c.B1 as RigidRectangle).Size.X / 2;
            float circleLeftBorder = c.B2.Center.X - c.B2.Radius;

            c.Start.X.Should().Be(circleLeftBorder);
            c.Start.Y.Should().Be(c.B2.Center.Y);

            c.Normal.X.Should().Be(1);

            c.Depth.Should().Be(recRightBorder - circleLeftBorder);
        }

        [Fact]
        public void CircleDontHitCorner1_FoundNoCollisionPoint()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "RectangleCircleCorner1NoCollision.txt").Bodies.ToList();
            var c = NearPhaseTests.RectangleCircle(bodys[0] as ICollidableRectangle, bodys[1] as ICollidableCircle);

            //Prüfe das es keine Kollision gab
            c.Should().BeNull();
        }

        [Fact]
        public void CircleDontHitCorner2_FoundNoCollisionPoint()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "RectangleCircleCorner2NoCollision.txt").Bodies.ToList();
            var c = NearPhaseTests.RectangleCircle(bodys[0] as ICollidableRectangle, bodys[1] as ICollidableCircle);

            //Prüfe das es keine Kollision gab
            c.Should().BeNull();
        }

        [Fact]
        public void CircleDontHitRightFace_FoundNoCollisionPoint()
        {
            var bodys = ExportHelper.ReadFromFile(TestData + "RectangleCircleFaceNoCollision.txt").Bodies.ToList();
            var c = NearPhaseTests.RectangleCircle(bodys[0] as ICollidableRectangle, bodys[1] as ICollidableCircle);

            //Prüfe das es keine Kollision gab
            c.Should().BeNull();
        }
    }
}
