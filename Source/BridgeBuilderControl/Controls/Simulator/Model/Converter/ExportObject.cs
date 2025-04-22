using BridgeBuilderControl.Controls.BridgeEditor.Model;
using RigidBodyPhysics.ExportData.RigidBody;
using TextureEditorGlobal;

namespace BridgeBuilderControl.Controls.Simulator.Model.Converter
{
    //Stellt eine einzelne Brückenstange, Kreis oder den Boden dar. Wird vom Converter benötigt.
    internal class ExportObject
    {
        public IExportRigidBody ExportRigidBody { get; set; }
        public TextureExportData TextureExportData { get; set; }
        public AnchorPoint[] AnchorPoints { get; set; }
        public Bar Bar { get; set; } //Wenn das eine Brückenstange ist, dann steht hier welche Stange das ist
    }
}
