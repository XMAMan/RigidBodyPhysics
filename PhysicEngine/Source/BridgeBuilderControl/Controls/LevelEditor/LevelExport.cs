using System.Drawing;

namespace BridgeBuilderControl.Controls.LevelEditor
{
    public class LevelExport
    {
        public Point[] GroundPolygon; //x = Kästchenindex; y = Das wie vielte Kästchen über der GroundHeight-Linie
        public Point[] AnchorPoints;  //Befestigungspunkte am Boden, wo die Brücke dran hängt

        public uint XCount = 64; //So viele kleine Kästchen ist das Level breit
        public uint YCount = 36; //So viele kleine Kästchen ist das Level hoch
        public uint GroundHeight = 18; //Kästchenindex wo der Boden ist
        public uint WaterHeight = 4; //So viele Kästchen beginnt das Wasser unter dem Boden       
        public uint Budget = 2000; //So viel Geld steht für den Bau der Brücke zur verfügung     
        public float TrainExtraSpeed = 0; //Bei Level 13,14,15 benötigt der Zug mehr Geschwindigkeit um es zu schaffen
        public float BarExtraPull = 0; //Bei Level 13,14,15 muss die Stange etwas mehr Zugkraft aushalten
    }
}
