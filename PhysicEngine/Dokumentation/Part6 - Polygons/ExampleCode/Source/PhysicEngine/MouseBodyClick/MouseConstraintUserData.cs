using PhysicEngine.ExportData;

namespace PhysicEngine.MouseBodyClick
{
    //Legt der Nutzer fest, wenn er mit der Maus ein Objekt festelegen will
    public class MouseConstraintUserData
    {
        public readonly float MaxForce; //Mit so viel Kraft zieht die Maus maximal am Ankerpunkt
        public readonly SoftExportData SoftData;

        public readonly float FrequencyHertz; //0 = Ankerpunkt springt sofort zum Mauszeiger ohne Dämpfung
        public readonly float DampingRatio; //0..1 (0=No Damping; 1=Max-Damping)

        private MouseConstraintUserData(float maxForce, SoftExportData softData)
        {
            if (maxForce < 0) throw new ArgumentOutOfRangeException("maxForce must be greater 0");
            

            MaxForce = maxForce;
            this.SoftData = softData;
        }

        public static MouseConstraintUserData CreateWithoutDamping(float maxForce = 100.0f)
        {
            return new MouseConstraintUserData(maxForce, new SoftExportData() { ParameterType = Joints.IPublicJoint.SpringParameter.NoSoftness });
        }

        public static MouseConstraintUserData CreateWithDamping(float maxForce = 100.0f, float frequencyHertz = 5, float dampingRatio = 0.7f)
        {
            if (frequencyHertz < 0) throw new ArgumentOutOfRangeException("frequencyHz must be greater 0");
            if (dampingRatio < 0 || dampingRatio > 1) throw new ArgumentOutOfRangeException("dampingRatio must be in range 0..1");
            return new MouseConstraintUserData(maxForce, new SoftExportData() { ParameterType = Joints.IPublicJoint.SpringParameter.FrequenceyAndDampingRatio, FrequencyHertz = frequencyHertz, DampingRatio = dampingRatio});
        }
    }
}
