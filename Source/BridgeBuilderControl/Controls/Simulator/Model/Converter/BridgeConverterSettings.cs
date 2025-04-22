namespace BridgeBuilderControl.Controls.Simulator.Model.Converter
{
    public class BridgeConverterSettings
    {
        public float TrainDensity = 10f;
        public float RectangleDensity = 0.8f; //Der Boden darf von der Dichte nicht deutlich kleiner als der Zug sein. Sonst sinkt er ein.
        public float TrainSpeed = 0.03f;
        public float MaxPullForce = -0.0137814879f; //Stab-Zugkraft (Defaultwert: Dieser Wert wird laut PushPullLimits.txt noch überschrieben)
        public float MaxPushForce = 0.14034459f;  //Stab-Druckkraft (Defaultwert: Dieser Wert wird laut PushPullLimits.txt noch überschrieben)
        public int BreakAfterNSteps = 3; //Erst wenn die Distanzjoints N Steps hintereinander zu hoher Last ausgesetzt sind brechen sie
        public bool BridgeIsBreakable = true;
        public float PositionalCorrectionRate = 0.01f; //Um so kleiner diese Zahl ist um so weniger bringt der Zug die Brücke in Schwingungen. Für die NoTrain-Tests muss das aber auf 0.1f gestellt werden
    }
}
