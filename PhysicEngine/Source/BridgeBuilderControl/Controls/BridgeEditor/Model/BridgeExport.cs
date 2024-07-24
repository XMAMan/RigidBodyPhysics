using System.Drawing;

namespace BridgeBuilderControl.Controls.BridgeEditor.Model
{
    public class BridgeExport
    {
        public string AssociatedLevel { get; set; }
        public Bar[] Bars { get; set; }
    }

    public class Bar
    {
        public Point P1 { get; set; }
        public Point P2 { get; set; }
    }
}
