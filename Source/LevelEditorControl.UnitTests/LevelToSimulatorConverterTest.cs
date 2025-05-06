using GraphicPanels;
using LevelEditorControl.Controls.EditorControl;
using LevelEditorControl.Controls.LevelEditorControl1;
using System.Drawing;
using System.Reactive;
using System.Reflection;
using System.Windows.Navigation;
using Xunit;

namespace LevelEditorControl.UnitTests
{
    public class LevelToSimulatorConverterTest
    {
        public const string InputData = @"..\..\..\..\..\Data\TestData\LevelEditorTestData\InputData\";
        public const string OuputData = @"..\..\..\..\..\Data\TestData\LevelEditorTestData\";
        public const string Expected = @"..\..\..\..\..\Data\TestData\LevelEditorTestData\ExpectedImages\";
        public const float TimerTickRateInMs = 30; //ms

        #region SetUp
        //Wird benötigt, damit EditorFileConverter.Convert keine Exception wirft (Erkärung: Siehe DemoGameTests.cs)
        public LevelToSimulatorConverterTest()
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

        [StaFact]
        public void ConvertLevelWithAllLevelItemTypes()
        {
            var panel = new GraphicPanel2D() { Width = 1500, Height = 600, Mode = Mode2D.OpenGL_Version_3_0 };

            var vm = (LevelEditorViewModel)new LevelEditorFactory().CreateEditorViewModel(new WpfControls.Model.EditorInputData()
            {
                DataFolder = InputData,
                TimerTickRateInMs = LevelToSimulatorConverterTest.TimerTickRateInMs,
                Panel = panel
            });

            vm.LoadFromTextFile(InputData + "ConvertLevelToSimulator.txt");

            
            vm.HandleTimerTick(TimerTickRateInMs); //Trigger das Zeichnen
            panel.GetScreenShoot().Save(OuputData + "ConvertLevelToSimulator_Editor.png");

            ((EditorViewModel)vm.ContentUserControl.DataContext).SimulatorViewModel.RestartClick.Execute(Unit.Default).Subscribe();

            vm.HandleTimerTick(TimerTickRateInMs); //Trigger das Zeichnen
            panel.GetScreenShoot().Save(OuputData + "ConvertLevelToSimulator_Simulator.png");


            TestHelper.CompareTwoBitmaps(new Bitmap(Expected + "ConvertLevelToSimulator_Editor.png"), new Bitmap(OuputData + "ConvertLevelToSimulator_Editor.png"));
            TestHelper.CompareTwoBitmaps(new Bitmap(Expected + "ConvertLevelToSimulator_Simulator.png"), new Bitmap(OuputData + "ConvertLevelToSimulator_Simulator.png"));
        }
    }
}