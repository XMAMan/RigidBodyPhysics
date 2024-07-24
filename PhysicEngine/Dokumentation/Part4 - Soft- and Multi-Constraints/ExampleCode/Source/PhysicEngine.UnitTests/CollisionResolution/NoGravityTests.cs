using FluentAssertions;
using Xunit;

namespace PhysicEngine.UnitTests.CollisionResolution
{
    //Testfälle wo es keine Schwerkraft gibt und wo ein Objekt eine Anfangsgeschwindigkeit
    //hat und gegen ein anders Objekt stößt. -> Erwartung: Objekte prallen voneiander ab
    public class NoGravityTests
    {
        private static string TestData = @"..\..\..\..\..\Data\CollisionResolutionTestcases\1_NoGravity\";
        private static float TimeStepTickRate = 50; //[ms]
        private static int MaxTrysToFindAnCollision = 1000; //Am Anfang wird so viele TimeSteps gesucht, bis ein Objekt gegen ein anders fliegt

        //Ein Circle fliegt gegen eine Wand. Erwartung: Er prallt davon ab
        [Fact]
        public void CircleAgainsWall_CircleGoesLeftAfterHit()
        {
            int circle = 1;
            var result = NoGravityTestHelper.DoTest(TestData + "CircleAgainsWall.txt", TimeStepTickRate, MaxTrysToFindAnCollision, false);
            result.BodysAfterCollision[circle].Velocity.X.Should().Be(-result.BodysBeforeCollision[circle].Velocity.X);
        }

        //Ein Würfel fliegt gegen eine Wand. Erwartung: Er prallt ab ohne sich zu drehen. Das geht hier nicht da die Collisionen iterativ gelößt werden.
        [Fact]
        public void CubeAgainstWall_CubeGoesLeftWithoutRotationAfterHit()
        {
            int cube = 0;
            var result = NoGravityTestHelper.DoTest(TestData + "CubeAgainstWall.txt", TimeStepTickRate, MaxTrysToFindAnCollision, true);
            result.BodysAfterCollision[cube].Velocity.X.Should().Be(-result.BodysBeforeCollision[cube].Velocity.X);
            result.BodysAfterCollision[cube].AngularVelocity.Should().Be(0);
        }

        //Zwei Kugeln fliegen oben und unten gegen ein Stab. Erwartung: Der Stab fliegt ohne Rotation nach rechts weg
        [Fact]
        public void TwoCirclesAgainstCube_RightCubeGoesRightWithoutRotationAfterHit()
        {
            int cube = 2;
            var result = NoGravityTestHelper.DoTest(TestData + "TwoCirclesAgainstCube.txt", TimeStepTickRate, MaxTrysToFindAnCollision, true);
            result.BodysAfterCollision[cube].AngularVelocity.Should().Be(0);
            result.BodysAfterCollision[cube].Velocity.X.Should().BeGreaterThan(0);
            result.BodysAfterCollision[cube].Velocity.Y.Should().Be(0);
        }

        //Eine leichte Kugel liegt vor einer Wand. Eine schwere Kugel fliegt dagegen. Erwartung: Die schwere Kugel wird an der leichten Kugel reflektiert
        [Fact]
        public void HeavyBallAgainstLightBall_HeyvyBallIsReflected()
        {
            //Die TimeStep-Zeit ist hier mit Faktor 0.1 kleiner und die Impulse-Iteration-Schleife hat 1500 anstatt nur
            //15 Durchläufe um somit mit vielen kleinen Impulsen die schwere Kugel zu stoppen.

            int wall = 0;        //Wand
            int circleHeavy = 2; //Schwere Kugel
            int circleLight = 1; //Leichte Kugel
            var result = NoGravityTestHelper.DoTest(TestData + "HeavyBallAgainstLightBall.txt", TimeStepTickRate * 0.01f, MaxTrysToFindAnCollision * 10, false, 1, 15000);

            //Die schwere Kugel muss nach links fliegen
            result.BodysAfterCollision[circleHeavy].Velocity.X.Should().BeLessThan(0);

            //Die leichte Kugel muss sich zwischen der schweren Kugel und der Wand befinden (Sie darf nicht durch die Wand hindurch gedrückt worden sein)
            result.BodysAfterCollision[circleLight].Position.X.Should().BeGreaterThan(result.BodysAfterCollision[circleHeavy].Position.X);
            result.BodysAfterCollision[circleLight].Position.X.Should().BeLessThan(result.BodysAfterCollision[wall].Position.X);

        }

        //Ein Circle fliegt gegen zwei Circles in Reihe. Erwartung: Der linke Circle wird gestoppt und der rechte Circle fliegt nach rechts weg
        [Fact]
        public void CircleAgainstCircleRow_RightCircleGoesRightAfterHit()
        {
            int circle1 = 0;
            int circle2 = 2;
            var result = NoGravityTestHelper.DoTest(TestData + "CircleAgainstCircleRow.txt", TimeStepTickRate, MaxTrysToFindAnCollision, false);
            result.BodysAfterCollision[circle1].Velocity.X.Should().Be(0);
            result.BodysAfterCollision[circle2].Velocity.X.Should().Be(result.BodysBeforeCollision[circle1].Velocity.X);
        }

        //Ein Circle fliegt gegen viele Circles in Reihe. Erwartung: Mit ein TimeStep-Aufruf wird er Impuls bis zum ganz rechten Circle übertragen
        [Fact]
        public void CircleAgaintLongCircleRow_RightCircleGoesRightAfterHit()
        {
            int circle1 = 6;
            int circle2 = 0;
            var result = NoGravityTestHelper.DoTest(TestData + "CircleAgaintLongCircleRow.txt", TimeStepTickRate, MaxTrysToFindAnCollision, false, 10, 8);
            result.BodysAfterCollision[circle1].Velocity.X.Should().Be(0);
            result.BodysAfterCollision[circle2].Velocity.X.Should().Be(result.BodysBeforeCollision[circle1].Velocity.X);
        }

        //Ein Würfel fliegt gegen zwei Würfel in Reihe. Erwartung: Der linke Würfel wird gestoppt und der rechte Würfel fliegt ohne Rotation nach rechts weg
        [Fact]
        public void CubeAgainstCubeRow_RightCubeGoesRightWithoutRotationAfterHit()
        {
            int cube1 = 0;
            int cube2 = 2;
            var result = NoGravityTestHelper.DoTest(TestData + "CubeAgainstCubeRow.txt", TimeStepTickRate, MaxTrysToFindAnCollision, false);
            result.BodysAfterCollision[cube1].Velocity.X.Should().Be(0);
            result.BodysAfterCollision[cube2].Velocity.X.Should().Be(result.BodysBeforeCollision[cube1].Velocity.X);
            result.BodysAfterCollision[cube2].AngularVelocity.Should().Be(0);
        }

        //Ein Würfel fliegt gegen vier Würfel in Reihe. Erwartung: Der linke Würfel wird gestoppt und der rechte Würfel fliegt ohne Rotation nach rechts weg
        [Fact]
        public void CubeAgainstLongCubeRow_RightCubeGoesRightWithoutRotationAfterHit()
        {
            int cube1 = 0;
            int cube2 = 4;
            var result = NoGravityTestHelper.DoTest(TestData + "CubeAgainstLongCubeRow.txt", TimeStepTickRate, MaxTrysToFindAnCollision, false);
            result.BodysAfterCollision[cube1].Velocity.X.Should().Be(0);
            result.BodysAfterCollision[cube2].Velocity.X.Should().Be(result.BodysBeforeCollision[cube1].Velocity.X);
            result.BodysAfterCollision[cube2].AngularVelocity.Should().Be(0);
        }

        //Von links und rechts fliegt jeweils eine Kugel gegen eine Kugel in der Mitte. Erwartung: Die Mitte bleibt ruhig. Links und Rechts prallen ab.
        [Fact]
        public void TwoCirclesAgainstMiddleCircle_LeftAndRightAreReflectedMiddleStaysCalmHit()
        {
            int circle1 = 0; //Links
            int circle2 = 1; //Mitte
            int circle3 = 2; //Rechts
            var result = NoGravityTestHelper.DoTest(TestData + "TwoCirclesAgainstMiddleCircle.txt", TimeStepTickRate, MaxTrysToFindAnCollision, true);
            result.BodysAfterCollision[circle1].Velocity.X.Should().Be(-result.BodysBeforeCollision[circle1].Velocity.X);
            result.BodysAfterCollision[circle2].Velocity.X.Should().Be(0);
            result.BodysAfterCollision[circle3].Velocity.X.Should().Be(-result.BodysBeforeCollision[circle3].Velocity.X);
        }

        //Von links und rechts fliegt jeweils eine Kugel gegen eine Kugel in der Mitte. Erwartung: Die Mitte bleibt ruhig. Links und Rechts vertauschen ihre Geschwindigkeit miteinander
        [Fact]
        public void TwoCirclesAgainstMiddleCircleDifferentSpeed_LeftAndRightSwapsThereVelocity()
        {
            int circle1 = 0; //Links
            int circle2 = 1; //Mitte
            int circle3 = 2; //Rechts
            var result = NoGravityTestHelper.DoTest(TestData + "TwoCirclesAgainstMiddleCircleDifferentSpeed.txt", TimeStepTickRate, MaxTrysToFindAnCollision, false, 1, 3);
            result.BodysAfterCollision[circle1].Velocity.X.Should().BeApproximately(result.BodysBeforeCollision[circle3].Velocity.X, 0.0001F);
            result.BodysAfterCollision[circle2].Velocity.X.Should().Be(0);
            result.BodysAfterCollision[circle3].Velocity.X.Should().BeApproximately(result.BodysBeforeCollision[circle1].Velocity.X, 0.0001F);
        }

        //Von links und rechts fliegt jeweils ein Würfel gegen ein Würfel in der Mitte. Erwartung: Die Mitte bleibt ruhig. Links und Rechts prallen ab.
        [Fact]
        public void TwoCubesAgainstMiddleCube_LeftAndRightAreReflectedMiddleStaysCalmHit()
        {
            int cube1 = 0; //Links
            int cube2 = 1; //Mitte
            int cube3 = 2; //Rechts
            var result = NoGravityTestHelper.DoTest(TestData + "TwoCubesAgainstMiddleCube.txt", TimeStepTickRate, MaxTrysToFindAnCollision, true);
            result.BodysAfterCollision[cube1].Velocity.X.Should().Be(-result.BodysBeforeCollision[cube1].Velocity.X);
            result.BodysAfterCollision[cube1].AngularVelocity.Should().Be(0);

            result.BodysAfterCollision[cube2].Velocity.X.Should().Be(0);
            result.BodysAfterCollision[cube2].AngularVelocity.Should().Be(0);

            result.BodysAfterCollision[cube3].Velocity.X.Should().Be(-result.BodysBeforeCollision[cube3].Velocity.X);
            result.BodysAfterCollision[cube3].AngularVelocity.Should().Be(0);
        }
    }

    

}
