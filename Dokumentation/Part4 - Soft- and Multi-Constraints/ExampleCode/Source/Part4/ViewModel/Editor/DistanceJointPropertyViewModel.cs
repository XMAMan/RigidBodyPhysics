using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using PhysicEngine.ExportData.Joints;

namespace Part4.ViewModel.Editor
{
    public class DistanceJointPropertyViewModel : ReactiveObject
    {
        [Reactive] public float LengthFactor { get; set; } = 1; //Die Sollwertlänge ergibt sich aus dem Abstand der Anchor-Punkte zum DistanceJoint-Definitionszeitpunkt mal diesen LengthFactor
        [Reactive] public float MinLength { get; set; } = 0;
        [Reactive] public float MaxLength { get; set; } = float.MaxValue;


        [Reactive] public DistanceJointExportData.SpringParameter SpringParameter { get; set; } = DistanceJointExportData.SpringParameter.NoSoftness;
        [Reactive] public float FrequencyHertz { get; set; } = 0; //Wie oft in der Sekunde soll die Feder schwingen
        [Reactive] public float DampingRatio { get; set; } = 0; //0 = Keine Dämpfung (Unendliche Schwingung); 1=Komplette Dämpfung
        [Reactive] public float Stiffness { get; set; } = 0; //ForceFactor k
        [Reactive] public float Damping { get; set; } = 0;  //Damping coefficient c
    }
}
