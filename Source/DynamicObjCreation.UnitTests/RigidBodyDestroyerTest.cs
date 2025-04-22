using DynamicObjCreation.RigidBodyDestroying;
using GameHelper.Simulation;
using GraphicMinimal;
using GraphicPanels;
using LevelEditorControl;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using System.Drawing;
using System.Reflection;
using System.Windows.Navigation;

namespace DynamicObjCreation.UnitTests
{

    [Collection("Our Test Collection #1")] //Mit diesen Attribut wird verhindert, dass die Testklassen, die auch diesen Collectionname haben parallel laufen. Das ist nötig wegen SetResourceAssembly und dem StaFact-Attribut. https://xunit.net/docs/running-tests-in-parallel
    public class RigidBodyDestroyerTest
    {
        public const string InputData = @"..\..\..\..\..\Data\TestData\DynamicObjCreationTestData\InputData\";
        public const string OuputData = @"..\..\..\..\..\Data\TestData\DynamicObjCreationTestData\";
        public const string Expected = @"..\..\..\..\..\Data\TestData\DynamicObjCreationTestData\ExpectedImages\";
        public const float TimerTickRateInMs = 30; //ms

        #region SetUp
        //Wird benötigt, damit EditorFileConverter.Convert keine Exception wirft (Erkärung: Siehe DemoGameTests.cs)
        public RigidBodyDestroyerTest()
        {
            if (!UriParser.IsKnownScheme("pack")) UriParser.Register(new GenericUriParser(GenericUriParserOptions.GenericAuthority), "pack", -1);
            SetResourceAssembly(typeof(LevelEditorFactory).Assembly);
        }
        //https://github.com/microsoft/testfx/issues/975
        public static void SetResourceAssembly(Assembly assembly)
        {
            var _resourceAssemblyField = typeof(System.Windows.Application).GetField("_resourceAssembly", BindingFlags.Static | BindingFlags.NonPublic);
            if (_resourceAssemblyField != null)
                _resourceAssemblyField.SetValue(null, assembly);

            var resourceAssemblyProperty = typeof(BaseUriHelper).GetProperty("ResourceAssembly", BindingFlags.Static | BindingFlags.NonPublic);
            if (resourceAssemblyProperty != null)
                resourceAssemblyProperty.SetValue(null, assembly);
        }
        #endregion

        [Fact]
        public void CreateWithSingleBox_InputImageIsEqualOutputImage()
        {
            GetImages(IRigidDestroyerParameter.DestroyMethod.SingleBox, out Bitmap inputImage, out Bitmap outputImage);

            inputImage.Save(OuputData + "InputSingleBox.bmp");
            outputImage.Save(OuputData + "OutputSingleBox.bmp");

            TestHelper.CompareTwoBitmaps(new Bitmap(Expected + "InputSingleBox.bmp"), new Bitmap(OuputData + "InputSingleBox.bmp"));
            TestHelper.CompareTwoBitmaps(new Bitmap(Expected + "OutputSingleBox.bmp"), new Bitmap(OuputData + "OutputSingleBox.bmp"));
        }

        [Fact]
        public void CreateWithBoxes_InputImageIsEqualOutputImage()
        {
            GetImages(IRigidDestroyerParameter.DestroyMethod.Boxes, out Bitmap inputImage, out Bitmap outputImage);

            inputImage.Save(OuputData + "InputBoxes.bmp");
            outputImage.Save(OuputData + "OutputBoxes.bmp");

            TestHelper.CompareTwoBitmaps(new Bitmap(Expected + "OutputBoxes.bmp"), new Bitmap(OuputData + "OutputBoxes.bmp"));
        }

        [Fact]
        public void CreateWithVoronoi_InputImageIsEqualOutputImage()
        {
            GetImages(IRigidDestroyerParameter.DestroyMethod.Voronoi, out Bitmap inputImage, out Bitmap outputImage);

            inputImage.Save(OuputData + "InputVoronoi.bmp");
            outputImage.Save(OuputData + "OutputVoronoi.bmp");

            TestHelper.CompareTwoBitmaps(new Bitmap(Expected + "OutputVoronoi.bmp"), new Bitmap(OuputData + "OutputVoronoi.bmp"));
        }

        private static void GetImages(IRigidDestroyerParameter.DestroyMethod method, out Bitmap inputImage, out Bitmap outputImage)
        {
            var panel = new GraphicPanel2D() { Width = 1400, Height = 900, Mode = Mode2D.OpenGL_Version_3_0 };
            var simulator = new GameSimulator(InputData + "TextureDestory.txt", panel.Size, TimerTickRateInMs) { ShowSmallWindow = false, CameraModus = Simulator.Simulator.CameraMode.Pixel };

            simulator.Draw(panel);
            simulator.DrawPhysicItemBorders(panel, Pens.Black);
            simulator.DrawTextureBorders(panel, Pens.Green);
            inputImage = panel.GetScreenShoot();

            List<IPublicRigidPolygon> polys = new List<IPublicRigidPolygon>();

            var bodies = simulator.GetAllBodiesOfType<IPublicRigidBody>().ToList();
            foreach (var body in bodies)
            {
                if (body is IPublicRigidPolygon)
                {
                    polys.Add((IPublicRigidPolygon)body);
                }

                simulator.DestroyRigidBody(body, method);
            }

            simulator.Draw(panel);
            simulator.DrawPhysicItemBorders(panel, Pens.Black);
            simulator.DrawTextureBorders(panel, Pens.Green);
            
            foreach (var poly in polys)
            {
                panel.DrawPolygon(new Pen(Color.Red, 2), poly.Vertex.Select(x => new Vector2D(x.X, x.Y)).ToList());
            }

            outputImage = panel.GetScreenShoot();

            panel.Dispose();
        }

        [Fact]
        public void DestroyPolygonWithoutTexture()
        {
            var panel = new GraphicPanel2D() { Width = 240, Height = 250, Mode = Mode2D.OpenGL_Version_3_0 };
            var simulator = new GameSimulator(InputData + "PolygonWithoutTextureDestroy.txt", panel.Size, TimerTickRateInMs) { ShowSmallWindow = false, CameraModus = Simulator.Simulator.CameraMode.Pixel };

            var body = simulator.GetAllBodiesOfType<IPublicRigidBody>().First();
            var bodies = simulator.DestroyRigidBody(body, new DestroyWithBoxesParameter() { BoxCount = 2 });
            foreach (var body1 in bodies)
            {
                simulator.DestroyRigidBody(body1, new DestroyWithBoxesParameter() { BoxCount = 2 });
            }

            simulator.Draw(panel);
            simulator.DrawPhysicItemBorders(panel, Pens.Black);
            simulator.DrawTextureBorders(panel, Pens.Green);

            var actualImage = panel.GetScreenShoot();

            panel.Dispose();

            actualImage.Save(OuputData + "PolygonWithoutTextureDestroy.bmp");

            TestHelper.CompareTwoBitmaps(new Bitmap(Expected + "PolygonWithoutTextureDestroy.bmp"), new Bitmap(OuputData + "PolygonWithoutTextureDestroy.bmp"));
        }
    }
}