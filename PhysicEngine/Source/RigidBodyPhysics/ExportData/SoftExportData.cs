using static RigidBodyPhysics.RuntimeObjects.Joints.IPublicJoint;

namespace RigidBodyPhysics.ExportData
{
    //Damit kann ich von ein Joint einen Soll-Wert soft machen (Oder die Maus auch)
    public class SoftExportData
    {
        public SpringParameter ParameterType { get; set; } = SpringParameter.FrequenceyAndDampingRatio;
        public float FrequencyHertz { get; set; } = 0; //Wie oft in der Sekunde soll die Feder schwingen
        public float DampingRatio { get; set; } = 0;  //0 = Keine Dämpfung (Unendliche Schwingung); 1=Komplette Dämpfung
        public float Stiffness { get; set; } = 0; //ForceFactor k
        public float Damping { get; set; } = 0;  //Damping coefficient c

        public SoftExportData() { }

        public SoftExportData(SoftExportData copy)
        {
            this.ParameterType = copy.ParameterType;
            this.FrequencyHertz = copy.FrequencyHertz;
            this.DampingRatio = copy.DampingRatio;
            this.Stiffness = copy.Stiffness;
            this.Damping = copy.Damping;
        }
    }
}
