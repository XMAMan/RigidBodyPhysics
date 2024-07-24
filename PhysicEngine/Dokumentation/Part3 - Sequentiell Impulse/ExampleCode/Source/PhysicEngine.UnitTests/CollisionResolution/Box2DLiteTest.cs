using GraphicMinimal;
using PhysicEngine.ExportData;
using PhysicEngine.RigidBody;
using Xunit;

namespace PhysicEngine.UnitTests.CollisionResolution
{
    //Scenes from https://github.com/erincatto/box2d-lite/blob/master/samples/main.cpp
    public class Box2DLiteTest
    {
        [Fact]
        public void SingleBox_FallingDown_LiesStillOnGround()
        {
            PhysicScene scene = new PhysicScene(new List<RigidBody.IRigidBody>()
            {
                new RigidRectangle(new RectangleExportData(){ Center = new Vector2D(0, -0.5f * 20), Size = new Vector2D(100, 20), AngleInDegree = 0, MassData = new MassData(MassData.MassType.Infinity, 0, 0), Friction = 0.2f, Restituion = 0 }), //Ground
                new RigidRectangle(new RectangleExportData(){ Center =new Vector2D(0, 4), Size = new Vector2D(1, 1), AngleInDegree = 0, MassData = new MassData(MassData.MassType.Mass, 200, 0), Friction = 0.2f, Restituion = 0 }), //Cube
            });

            scene.DoPositionalCorrection = true;
            scene.DoWarmStart = true;
            scene.IterationCount = 10;
            scene.HasGravity = true;
            scene.Gravity = -10;
            float timeStep = 1.0f / 60.0f;

            for (int i=0;i<1000;i++)
            {
                scene.TimeStep(timeStep);
            }
        }
    }
}
