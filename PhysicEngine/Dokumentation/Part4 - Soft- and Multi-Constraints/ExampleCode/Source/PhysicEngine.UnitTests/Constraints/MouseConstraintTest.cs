using GraphicMinimal;
using PhysicEngine.MathHelper;
using PhysicEngine.UnitTests.TestHelper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PhysicEngine.UnitTests.Constraints
{
    //Hier soll geprüft werden, dass Würfel mit unterschiedlichen Gewicht mit der Maus angehoben werden können ohne das sie unkontrolliert wegfliegen
    public class MouseConstraintTest
    {
        private static string TestData = @"..\..\..\..\..\Data\Constraints\\MouseConstraint\";
        private static string TestResults = @"..\..\..\..\..\Data\TestResults\";
        private static string ExpectedImages = @"..\..\..\..\..\Data\ExpectedImages\";
        private static float TimeStepTickRate = 50; //[ms]

        private enum MouseResult { NoObject, Ok, Fail}

        [Fact]
        public void MouseConstraint_TestForEachPixel_ObjectRemainsStickedOnMousePoint()
        {
            int sizeFactor = 7;
            Bitmap result = new Bitmap(870 / sizeFactor, 200 / sizeFactor);
            for (int x=0;x<result.Width;x++)
                for (int y=0;y<result.Height;y++)
                {
                    var r = DoMouseTest(new Vector2D(x * sizeFactor, y * sizeFactor));
                    Color color = Color.Black;
                    switch (r)
                    {
                        case MouseResult.NoObject:
                            color = Color.White;
                            break;
                        case MouseResult.Ok:
                            color = Color.Green;
                            break;
                        case MouseResult.Fail:
                            color = Color.Red;
                            break;
                    }
                    result.SetPixel(x, y, color);
                }

            result.Save(TestResults + "MouseStick.bmp");

            var expected = new Bitmap(ExpectedImages + "MouseStick.bmp");
            Assert.True(ImageCompare.CompareTwoBitmaps(expected, result));
        }

        private static MouseResult DoMouseTest(Vector2D mousePosition)
        {
            var result = SimulateSeveralTimeSteps.DoTest(TestData + "CubesWithDifferentMass.txt", TimeStepTickRate, true, 10, 5, new SimulateSeveralTimeSteps.ExtraSettings() { 
                DoPositionCorrection = true, DoWarmStart = true, 
                MouseEvents = new SimulateSeveralTimeSteps.MouseEvent[]
                {
                    new SimulateSeveralTimeSteps.MouseEvent(SimulateSeveralTimeSteps.MouseEvent.EventType.MouseDown, mousePosition, 0),
                    new SimulateSeveralTimeSteps.MouseEvent(SimulateSeveralTimeSteps.MouseEvent.EventType.MouseMove, mousePosition - new Vector2D(0,10), 1),
                }
            });

            var m = result.TimeSteps.Last().MouseData;
            if (m!=null)
            {
                Vector2D mousePos = m.Position;
                Vector2D objPosition = MathHelp.GetWorldPointFromLocalDirection(m.ClickData.RigidBody, m.ClickData.LocalAnchorDirection);
                if ((mousePos - objPosition).Length() < 1)
                    return MouseResult.Ok;
                else
                    return MouseResult.Fail;
            }
            return MouseResult.NoObject;
        }
    }
}
