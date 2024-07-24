using BitmapHelper;
using FluentAssertions;
using RigidBodyPhysics.UnitTests.TestHelper;
using System.Drawing;
using Xunit;

namespace RigidBodyPhysics.UnitTests.Constraints
{
    public class AxialFrictionTest
    {
        private static string TestData = @"..\..\..\..\..\Data\TestData\RigidBodyPhysicsTestData\AxialFrictionTestcases\";
        private static string TestResults = @"..\..\..\..\..\Data\TestData\RigidBodyPhysicsTestData\TestResults\";
        private static string ExpectedImages = @"..\..\..\..\..\Data\TestData\RigidBodyPhysicsTestData\ExpectedImages\";
        private static float TimeStepTickRate = 50; //[ms]

        //Ein Autoreifen ist von oben zu sehen. Ein Ball rollt von der Seite (In Achsenrichtung) dagegen.
        //Erwartung: Autoreifen rutscht etwas zur Seite aber er wird wegen der Reibung zwischen Gummi und Straße gebremst
        [Fact]
        public void Wheel_GetAPunchOnTheSide_SlidesWithFriction()
        {
            int wheel = 0;
            var result = SimulateSeveralTimeSteps.DoTest(TestData + "SingleBlock1.txt", TimeStepTickRate, true, 10, 50, false, new SimulateSeveralTimeSteps.ExtraSettings() { DoPositionCorrection = true, DoWarmStart = false });

            var vel = result.TimeSteps.Select(x => x.Bodies[wheel].Velocity.X).ToArray();
            vel = SimulateSeveralTimeSteps.SkipFirstZeros(vel); //Am Anfang liegt das Rad noch ruhig da. Überspringe den Anfang
            vel.Should().BeInDescendingOrder(); //Mit jeden Timestep soll die Geschwindigkeit sich verringern
            vel.Last().Should().Be(0); //Körper liegt am Ende ruhig da
        }

        //Ein Autoreifen ist von oben zu sehen. Ein Ball rollt von hinten (In Laufrichtung des Rades) dagegen.
        //Erwartung: Der Autoreifen rollt in Fahrrichtung ohne Probleme davon
        [Fact]
        public void Wheel_GetAPunchInTravelDirection_RollAway()
        {
            int wheel = 0;
            var result = SimulateSeveralTimeSteps.DoTest(TestData + "SingleBlock2.txt", TimeStepTickRate, true, 10, 50, false, new SimulateSeveralTimeSteps.ExtraSettings() { DoPositionCorrection = true, DoWarmStart = false });

            var vel = result.TimeSteps.Select(x => x.Bodies[wheel].Velocity.Y).ToArray();
            vel = SimulateSeveralTimeSteps.SkipFirstZeros(vel); //Am Anfang liegt das Rad noch ruhig da. Überspringe den Anfang
            vel.Should().AllBeEquivalentTo(vel[0]); //Das Rad bewegt sich ungebremst fort
        }

        //Pendel 1 ist ungedämpft und Pendel 2 ist gedämpft.
        //Erwartung: Die Geschwindigkeit von Pendel 1 sieht wie eine Sinuskurve aus und bei Pendel 2 wie eine gedämpfte Sinuskurve
        [Fact]
        public void Pendulum_With_and_Without_Damping()
        {
            int pendulum1 = 1;
            int pendulum2 = 3;
            var result = SimulateSeveralTimeSteps.DoTest(TestData + "Pendulum.txt", TimeStepTickRate, true, 10, 500, true, new SimulateSeveralTimeSteps.ExtraSettings() { DoPositionCorrection = true, DoWarmStart = false });

            var vel1 = result.TimeSteps.Select(x => x.Bodies[pendulum1].Velocity.Length()).ToArray();
            var vel2 = result.TimeSteps.Select(x => x.Bodies[pendulum2].Velocity.Length()).ToArray();

            var actual = BitmapHelp.TransformBitmapListToRow(new List<System.Drawing.Bitmap>()
            {
                FunctionPlotter.PlotFloatArray(vel1, 1, "No Damping"),
                FunctionPlotter.PlotFloatArray(vel2, 1, "With Damping")
            });
            
            actual.Save(TestResults + "Pendulum_With_and_Without_Damping.bmp");

            var expected = new Bitmap(ExpectedImages + "Pendulum_With_and_Without_Damping.bmp");
            Assert.True(ImageCompare.CompareTwoBitmaps(expected, actual));
        }
    }
}
