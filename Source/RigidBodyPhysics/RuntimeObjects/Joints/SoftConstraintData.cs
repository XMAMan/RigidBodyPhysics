using RigidBodyPhysics.ExportData;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using static RigidBodyPhysics.RuntimeObjects.Joints.IPublicJoint;

namespace RigidBodyPhysics.RuntimeObjects.Joints
{
    //Hilfsklasse für die Formeln (2.12), (2.17) aus "3D Constraint Derivations for Impulse Solvers - Marijn 2015" und "Soft Constraints - Erin Catto 2011" Seite 34
    internal class SoftConstraintData
    {
        internal SpringParameter ParameterType { get; set; } = SpringParameter.FrequenceyAndDampingRatio;
        internal float FrequencyHertz { get; } = 0; //Wie oft in der Sekunde soll die Feder schwingen
        internal float DampingRatio { get; } = 0; //0 = Keine Dämping (Unendliche Schwingung); 1=Komplette Dämpfung
        internal float Stiffness { get; set; } = 0; //ForceFactor k: 0=Keine Feder    -> Nur diese Parameter werden intern zur Berechnung verwendet
        internal float Damping { get; set; } = 0;  //Damping coefficient c            -> Nur diese Parameter werden intern zur Berechnung verwendet


        public SoftConstraintData(SoftExportData data, IRigidBody bodyA, IRigidBody bodyB)
        {
            ParameterType = data.ParameterType;
            FrequencyHertz = data.FrequencyHertz;
            DampingRatio = data.DampingRatio;
            Stiffness = data.Stiffness;
            Damping = data.Damping;

            if (ParameterType == SpringParameter.FrequenceyAndDampingRatio)
            {
                float stiffness, damping;
                JointSoftnessHelper.LinearFrequencyToStiffness(FrequencyHertz, DampingRatio, bodyA, bodyB, out stiffness, out damping);
                Stiffness = stiffness;
                Damping = damping;
            }
            else if (ParameterType == SpringParameter.StiffnessAndDamping)
            {
                float frequencyHertz, dampingRatio;
                JointSoftnessHelper.LinearStiffnessToFrequency(Stiffness, Damping, bodyA, bodyB, out frequencyHertz, out dampingRatio);
                FrequencyHertz = frequencyHertz;
                DampingRatio = dampingRatio;
            }
        }

        public SoftExportData GetExportData()
        {
            return new SoftExportData()
            {
                ParameterType = ParameterType,
                FrequencyHertz = FrequencyHertz,
                DampingRatio = DampingRatio,
                Stiffness = Stiffness,
                Damping = Damping
            };
        }

        //k=J*M^-1*J^T -> Bei ein Multiconstraint wird für k float.NaN übergeben
        public void GetSoftConstraintParameters(float dt, float k, Action<float> gammaOut, Action<float> betaOut, Action<float> ImpulseMassOut)
        {
            float h = dt;

            //Quelle1: https://github.com/erincatto/box2d/blob/main/src/dynamics/b2_distance_joint.cpp#L138C27-L138C27
            //Quelle2: 3D Constraint Derivations for Impulse Solvers - Marijn 2015" Seite 9 Formel (2.17)
            float gamma = 1.0f / (h * (Damping + h * Stiffness)); //Gamma entspricht Gamma/dt aus (2.17) "3D Constraint Derivations for Impulse Solvers - Marijn 2015"
            float beta = h * Stiffness * gamma; //Beta entspricht Beta/dt-Wert aus (2.17)
            float ImpulseMass = 1.0f / (k + gamma); //ImpulseMass entspricht K aus der Formel (2.12)

            //Da Propertys nicht als Out-Parameter verwendet werden dürfen und das IConstraint-Interface aber sagt, dass das Propertys sein müssen nutze ich Out-Actions
            gammaOut(gamma);
            betaOut(beta);
            ImpulseMassOut(ImpulseMass);
        }
    }
}
