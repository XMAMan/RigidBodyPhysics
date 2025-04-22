using BridgeBuilderControl.Controls.BridgeEditor.Model;
using BridgeBuilderControl.Controls.Helper;
using BridgeBuilderControl.Controls.LevelEditor;
using BridgeBuilderControl.Controls.Simulator.Model;
using BridgeBuilderControl.Controls.Simulator.Model.Converter;
using GraphicPanels;
using System.IO;
using WpfControls.Controls.CameraSetting;

namespace BridgeBuilderControl.Testing
{
    public class BridgeSimulatorFactory
    {
        private GraphicPanel2D panel;
        private string dataFolder;
        private float timerIntervallInMilliseconds;
        public BridgeSimulatorFactory(GraphicPanel2D panel, string dataFolder, float timerIntervallInMilliseconds)
        {
            this.panel = panel;
            this.dataFolder = dataFolder;
            this.timerIntervallInMilliseconds = timerIntervallInMilliseconds;
        }
        public IBridgeSimulator CreateSimulator(string bridgeFile, BridgeConverterSettings settings)
        {
            var bridgeData = JsonHelper.Helper.CreateFromJson<BridgeExport>(File.ReadAllText(bridgeFile));
            string levelFile = new DirectoryInfo(this.dataFolder).FullName + "\\Levels\\" + bridgeData.AssociatedLevel;
            var levelExport = JsonHelper.Helper.CreateFromJson<LevelExport>(File.ReadAllText(levelFile));
            var camera = new EditorCamera(panel.Width, panel.Height, levelExport.XCount, levelExport.YCount);
            camera.InitialPosition = Camera2D.InitialPositionIfAutoZoomIsActivated.ToLeftTopCorner;
            var simulatorInput = new SimulatorInput()
            {
                Camera = camera,
                LevelExport = levelExport,
                BridgeExport = bridgeData,
                LevelFile = levelFile
            };

            var simulateBridgeFunction = new SimulateBridgeFunction(panel, dataFolder, timerIntervallInMilliseconds, simulatorInput, settings);

            return simulateBridgeFunction;
        }
    }
}
