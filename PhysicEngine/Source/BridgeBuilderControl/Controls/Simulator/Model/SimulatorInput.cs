using BridgeBuilderControl.Controls.BridgeEditor.Model;
using BridgeBuilderControl.Controls.Helper;
using BridgeBuilderControl.Controls.LevelEditor;

namespace BridgeBuilderControl.Controls.Simulator.Model
{
    internal class SimulatorInput
    {
        public EditorCamera Camera { get; set; }
        public LevelExport LevelExport { get; set; }
        public BridgeExport BridgeExport { get; set; }
        public string LevelFile { get; set; }
    }
}
