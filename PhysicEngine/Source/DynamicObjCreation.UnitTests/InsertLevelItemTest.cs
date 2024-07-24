using GameHelper.Simulation;
using GraphicMinimal;
using GraphicPanels;
using LevelEditorControl;
using LevelToSimulatorConverter;
using System.Drawing;
using System.Reflection;
using System.Windows.Navigation;

namespace DynamicObjCreation.UnitTests
{
    [Collection("Our Test Collection #1")] //Mit diesen Attribut wird verhindert, dass die Testklassen, die auch diesen Collectionname haben parallel laufen. Das ist nötig wegen SetResourceAssembly und dem StaFact-Attribut. https://xunit.net/docs/running-tests-in-parallel
    public class InsertLevelItemTest
    {
        public const string InputData = @"..\..\..\..\..\Data\TestData\DynamicObjCreationTestData\InputData\";
        public const string OuputData = @"..\..\..\..\..\Data\TestData\DynamicObjCreationTestData\";
        public const string Expected = @"..\..\..\..\..\Data\TestData\DynamicObjCreationTestData\ExpectedImages\";
        public const float TimerTickRateInMs = 30; //ms

        #region SetUp
        //Wird benötigt, damit EditorFileConverter.Convert keine Exception wirft (Erkärung: Siehe DemoGameTests.cs)
        public InsertLevelItemTest()
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

        //Prüft ab, dass man Kopien von LevelItems erzeugen kann und sie dann beim platzieren rotieren/skalieren kann und
        //dessen TagDaten nutzen kann. Außerdem wird geprüft, dass ich alle erzeugten Objekte per Tastendruck und Autoanimaiton bewegen kann.
        [Fact]
        public void CreateCopyFromLevelItem()
        {
            var panel = new GraphicPanel2D() { Width = 920, Height = 650, Mode = Mode2D.OpenGL_Version_3_0 };
            var simulator = new GameSimulator(InputData + "InsertLevelItem.txt", panel.Size, TimerTickRateInMs) { ShowSmallWindow = false, CameraModus = Simulator.Simulator.CameraMode.Pixel };

            //Bewege zuerst das Originalobjekt und erzeuge erst danach dann Kopien davon
            for (int i=0;i<100;i++)
            {
                simulator.MoveOneStep(TimerTickRateInMs);
            }

            var body = simulator.GetBodiesByTagName("Original").First();
            int levelItemId = simulator.GetTagDataFromBody(body).LevelItemId;
            var exportData = simulator.GetExportDataFromLevelItem(levelItemId); //Kopie nach außen geben
            var box = PhysicSceneExportDataHelper.GetBoundingBoxFromScene(exportData.PhysicSceneData);

            simulator.RemoveLevelItem(levelItemId); //Original löschen

            List<Vector2D> pivotPoints = new List<Vector2D>();

            List<Vector2D> tagPoints = new List<Vector2D>();

            var allOriantations = new LevelItemExportHelper.PivotOriantation[]
            {
                LevelItemExportHelper.PivotOriantation.Center,
                LevelItemExportHelper.PivotOriantation.TopLeft,
                LevelItemExportHelper.PivotOriantation.BottomCenter
            };

            for (int i=0;i<allOriantations.Length;i++)
            {
                var copyData = new LevelEditorGlobal.PhysikLevelItemExportData(exportData); //Kopie vom Original-Export erstellen
                var pivotPoint = new Vector2D(box.Width + i * box.Width * 2, box.Height);
                LevelItemExportHelper.MoveToPivotPoint(copyData, pivotPoint, allOriantations[i], 1, 0); //Kopie bearbeiten
                int newId = simulator.AddLevelItem(copyData);
                pivotPoints.Add(pivotPoint);

                //Hiermit teste ich, dass ich bei eingefügten Objekten auf die Tagdaten zugreifen kann
                tagPoints.Add(simulator.GetBodyByTagName(newId, "body1").Center.ToGrx());
                tagPoints.Add(simulator.GetJointByTagName(newId, "joint1").Anchor1.ToGrx());
                tagPoints.Add(simulator.GetThrusterByTagName(newId, "thruster1").Anchor.ToGrx());
                tagPoints.Add(simulator.GetMotorByTagName(newId, "motor1").Body.Center.ToGrx());
            }

            var allSizes = new float[] { 0.2f, 1, 2 };
            for (int i=0;i<allSizes.Length;i++)
            {
                var copyData = new LevelEditorGlobal.PhysikLevelItemExportData(exportData); //Kopie vom Original-Export erstellen
                var pivotPoint = new Vector2D(box.Width + i * box.Width * 2, box.Height + 1 * box.Height * 2);
                LevelItemExportHelper.MoveToPivotPoint(copyData, pivotPoint,  LevelItemExportHelper.PivotOriantation.Center, allSizes[i], 0); //Kopie bearbeiten
                simulator.AddLevelItem(copyData);
                pivotPoints.Add(pivotPoint);
            }

            var allAngles = new float[] { -45, 0, 45 };
            for (int i = 0; i < allSizes.Length; i++)
            {
                var copyData = new LevelEditorGlobal.PhysikLevelItemExportData(exportData); //Kopie vom Original-Export erstellen
                var pivotPoint = new Vector2D(box.Width + i * box.Width * 2, box.Height + 2 * box.Height * 2);
                LevelItemExportHelper.MoveToPivotPoint(copyData, pivotPoint, LevelItemExportHelper.PivotOriantation.Center, 1, allAngles[i]); //Kopie bearbeiten
                simulator.AddLevelItem(copyData);
                pivotPoints.Add(pivotPoint);
            }

            //Bewege alle erzeugten Kopien gleichzeitig (Per Tastendruck und Autoanimation)
            simulator.HandleKeyDown(System.Windows.Input.Key.Left);
            for (int i = 0; i < 500; i++)
            {
                simulator.MoveOneStep(TimerTickRateInMs);
            }

            simulator.Draw(panel);
            //panel.DrawRectangle(Pens.Red, box.Min.X, box.Min.Y, box.Width, box.Height);
            for (int i=0;i<pivotPoints.Count;i++)
            {
                panel.DrawRectangle(Pens.Green, pivotPoints[i].X - box.Width, pivotPoints[i].Y - box.Height, box.Width * 2, box.Height * 2);
                panel.DrawFillCircle(Color.Red, pivotPoints[i], 2);
            }
            foreach (var tagPoint in tagPoints)
            {
                panel.DrawFillCircle(Color.Blue, tagPoint, 2);
            }
            
            var image = panel.GetScreenShoot();
            panel.Dispose();

            image.Save(OuputData + "InsertLevelItems.bmp");

            TestHelper.CompareTwoBitmaps(new Bitmap(Expected + "InsertLevelItems.bmp"), new Bitmap(OuputData + "InsertLevelItems.bmp"));
        }

        //Prüfe, dass eine manuelle Animation genau an der Stelle weiter geht, wo sie vor dem GetExport aufgehört hat
        //Obwohl der interne Animation-Wert bei 0 losgeht funktioniert dass, weil die Body-Position verändert wurde
        [Fact]
        public void CreateCopyFromManualAnimation()
        {
            var panel = new GraphicPanel2D() { Width = 340, Height = 216, Mode = Mode2D.OpenGL_Version_3_0 };

            //Schritt 1: Animation läuft 400 Steps ab
            var simulator1 = new GameSimulator(InputData + "InsertManualLevelItem.txt", panel.Size, TimerTickRateInMs) { ShowSmallWindow = false, CameraModus = Simulator.Simulator.CameraMode.Pixel };
            simulator1.HandleKeyDown(System.Windows.Input.Key.Left);
            for (int i = 0; i < 400; i++)
            {
                simulator1.MoveOneStep(TimerTickRateInMs);
            }
            simulator1.Draw(panel);
            var image1 = panel.GetScreenShoot();
            image1.Save(OuputData + "InsertManualLevelItem1.bmp");

            //Schritt 2: Animation läuft erst 200 Steps ab. Dann erfolgt Export/Import. Dann nochmal 200 Steps
            var simulator2 = new GameSimulator(InputData + "InsertManualLevelItem.txt", panel.Size, TimerTickRateInMs) { ShowSmallWindow = false, CameraModus = Simulator.Simulator.CameraMode.Pixel };
            simulator2.HandleKeyDown(System.Windows.Input.Key.Left);
            for (int i = 0; i < 200; i++)
            {
                simulator2.MoveOneStep(TimerTickRateInMs);
            }
            simulator2.HandleKeyUp(System.Windows.Input.Key.Left);

            int levelItemId = 1;
            var exportData = simulator2.GetExportDataFromLevelItem(levelItemId); //Kopie nach außen geben
            simulator2.RemoveLevelItem(levelItemId); //Original löschen
            simulator2.AddLevelItem(exportData);

            simulator2.HandleKeyDown(System.Windows.Input.Key.Left);
            for (int i = 0; i < 200; i++)
            {
                simulator2.MoveOneStep(TimerTickRateInMs);
            }

            simulator2.Draw(panel);
            var image2 = panel.GetScreenShoot();
            image2.Save(OuputData + "InsertManualLevelItem2.bmp");

            panel.Dispose();

            //Erwartung: Beide Bilder sind gleich
            TestHelper.CompareTwoBitmaps(new Bitmap(OuputData + "InsertManualLevelItem1.bmp"), new Bitmap(OuputData + "InsertManualLevelItem2.bmp"));

        }

        //Prüfe, dass auch Objekte ohne Animation/Keyboarddaten/TagDaten kopierbar sind
        [Fact]
        public void CreateCopyFromRectangleLevelItem()
        {
            var panel = new GraphicPanel2D() { Width = 200, Height = 200, Mode = Mode2D.OpenGL_Version_3_0 };
            var simulator = new GameSimulator(InputData + "InsertLevelItemRectangle.txt", panel.Size, TimerTickRateInMs) { ShowSmallWindow = false, CameraModus = Simulator.Simulator.CameraMode.Pixel };

            int levelItemId = 1;
            var exportData = simulator.GetExportDataFromLevelItem(levelItemId); //Kopie nach außen geben
            simulator.RemoveLevelItem(levelItemId); //Original löschen
            simulator.AddLevelItem(exportData);

            simulator.Draw(panel);
            var image = panel.GetScreenShoot();
            panel.Dispose();

            image.Save(OuputData + "InsertLevelItemRectangle.bmp");
            TestHelper.CompareTwoBitmaps(new Bitmap(Expected + "InsertLevelItemRectangle.bmp"), new Bitmap(OuputData + "InsertLevelItemRectangle.bmp"));
        }
    }
}
