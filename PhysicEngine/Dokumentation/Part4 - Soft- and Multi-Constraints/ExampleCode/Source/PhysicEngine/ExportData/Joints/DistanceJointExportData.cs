using GraphicMinimal;

namespace PhysicEngine.ExportData.Joints
{
    public class DistanceJointExportData : IExportJoint
    {
        public int BodyIndex1 { get; set; }
        public int BodyIndex2 { get; set; }
        public Vector2D R1 { get; set; } //Hebelarm im lokalen Bodyspace von B1.Center nach Anchor1-Punkt
        public Vector2D R2 { get; set; }
        public float LengthFactor { get; set; } = 1; //Die Sollwertlänge ergibt sich aus dem Abstand der Anchor-Punkte zum DistanceJoint-Definitionszeitpunkt mal diesen LengthFactor
        public float MinLength { get; set; } = 0;
        public float MaxLength { get; set; } = float.MaxValue;

        public enum SpringParameter { FrequenceyAndDampingRatio, StiffnessAndDamping, NoSoftness }
        public SpringParameter ParameterType { get; set; } = SpringParameter.FrequenceyAndDampingRatio;
        public float FrequencyHertz { get; set; } = 0; //Wie oft in der Sekunde soll die Feder schwingen
        public float DampingRatio { get; set; } = 0;  //0 = Keine Dämpfung (Unendliche Schwingung); 1=Komplette Dämpfung
        public float Stiffness { get; set; } = 0; //ForceFactor k
        public float Damping { get; set; } = 0;  //Damping coefficient c
    }
}
