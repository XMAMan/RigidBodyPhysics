namespace BridgeBuilderControl.Controls.Simulator.Model.Converter
{
    //So viele Push-Puhll-Kräfte hält eine Brücke maximal unter Verwendung von BreakAfterNSteps aus 
    public class PushPullLimit
    {
        public string LevelFile { get; set; }
        public int BreakAfterNSteps { get; set; } //Erst wenn die Distanzjoints N Steps hintereinander zu hoher Last ausgesetzt sind brechen sie
        public float MaxPullForce { get; set; } //Stab-Zugkraft
        public float MaxPushForce { get; set; }   //Stab-Druckkraft 
    }
}
