using GameHelper.Simulation;
using GraphicPanels;
using LevelEditorControl;
using RigidBodyPhysics.MathHelper;
using System.Drawing;
using System.Reflection;
using System.Windows.Navigation;

namespace DynamicObjCreation.UnitTests
{
    [Collection("Our Test Collection #1")] //Mit diesen Attribut wird verhindert, dass die Testklassen, die auch diesen Collectionname haben parallel laufen. Das ist nötig wegen SetResourceAssembly und dem StaFact-Attribut. https://xunit.net/docs/running-tests-in-parallel
    public class AstroidCreatorTest
    {
        public const string InputData = @"..\..\..\..\..\Data\TestData\DynamicObjCreationTestData\InputData\";
        public const string OuputData = @"..\..\..\..\..\Data\TestData\DynamicObjCreationTestData\";
        public const string Expected = @"..\..\..\..\..\Data\TestData\DynamicObjCreationTestData\ExpectedImages\";
        public const float TimerTickRateInMs = 30; //ms

        #region SetUp
        //Wird benötigt, damit EditorFileConverter.Convert keine Exception wirft (Erkärung: Siehe DemoGameTests.cs)
        public AstroidCreatorTest()
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

        [Fact] //Wird beötigt, da das GraphicPanel2D ansonsten nur 333 Pixel breit/hoch anstatt 500 ist
        public void CreateMultipleAstroids()
        {
            int size = 50;
            var panel = new GraphicPanel2D() { Width = size * 10, Height = size * 10, Mode = Mode2D.OpenGL_Version_3_0 };
            var simulator = new GameSimulator(InputData + "EmptyLevel.txt", panel.Size, TimerTickRateInMs) { ShowSmallWindow = false, CameraModus = Simulator.Simulator.CameraMode.Pixel };

            panel.ClearScreen(Color.White);

            Random rand = new Random(0);

            for (int x=0;x<10;x++)
                for (int y=0;y<10;y++)
                {
                    Vec2D center = new Vec2D((x + 0.5f) * size, (y + 0.5f) * size);
                    //var points = RandomPolygonCreator.CreateAstroidPolygon(rand, size / 2, 25).Select(x => (x + center).ToGrx()).ToList();
                    //panel.DrawPolygon(new Pen(Color.Black, 2), points);
                    //panel.DrawCircle(Pens.Blue, center.ToGrx(), size / 2);

                    var astroid = AstroidCreator.CreateAstroid(center, rand, size / 2, 25, InputData + "muster8.jpg", new Vec2D(0, 0), 0);
                    simulator.AddRigidBody(astroid);
                }

            simulator.Draw(panel);

            //panel.FlipBuffer();
            var image = panel.GetScreenShoot();
            panel.Dispose();

            image.Save(OuputData + "Astroids.bmp");

            TestHelper.CompareTwoBitmaps(new Bitmap(Expected + "Astroids.bmp"), new Bitmap(OuputData + "Astroids.bmp"));
        }
    }
}
