using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using RigidBodyPhysics.ExportData;
using static RigidBodyPhysics.RuntimeObjects.Joints.IPublicJoint;

namespace PhysicSceneEditorControl.Controls.SoftProperty
{
    internal class SoftPropertyViewModel : ReactiveObject
    {
        [Reactive] public SpringParameter SpringParameter { get; set; } = SpringParameter.StiffnessAndDamping;
        [Reactive] public float FrequencyHertz { get; set; } = 100; //Wie oft in der Sekunde soll die Feder schwingen
        [Reactive] public float DampingRatio { get; set; } = 0.5f; //0 = Keine Dämpfung (Unendliche Schwingung); 1=Komplette Dämpfung
        [Reactive] public float Stiffness { get; set; } = 200; //ForceFactor k
        [Reactive] public float Damping { get; set; } = 0.1f;  //Damping coefficient c

        public SoftPropertyViewModel() { }

        public SoftPropertyViewModel(SoftExportData ctor)
        {
            this.SpringParameter = ctor.ParameterType;
            this.FrequencyHertz = ctor.FrequencyHertz;
            this.DampingRatio = ctor.DampingRatio;
            this.Stiffness = ctor.Stiffness;
            this.Damping = ctor.Damping;
        }

        public SoftExportData GetExportData()
        {
            return new SoftExportData()
            {
                ParameterType = this.SpringParameter,
                FrequencyHertz = this.FrequencyHertz,
                DampingRatio = this.DampingRatio,
                Stiffness = this.Stiffness,
                Damping = this.Damping
            };
        }
    }
}
