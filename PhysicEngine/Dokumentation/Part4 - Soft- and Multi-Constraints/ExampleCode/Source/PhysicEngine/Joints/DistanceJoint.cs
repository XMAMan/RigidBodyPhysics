using GraphicMinimal;
using PhysicEngine.CollisionResolution.SequentiellImpulse;
using PhysicEngine.ExportData.Joints;
using PhysicEngine.MathHelper;
using PhysicEngine.RigidBody;
using static PhysicEngine.ExportData.Joints.DistanceJointExportData;

namespace PhysicEngine.Joints
{
    public class DistanceJoint : IJoint
    {
        private Vector2D r1; //lokaler Richtungsvektor von B1.Center nach Anchor1
        private Vector2D r2;

        public IRigidBody B1 { get; }
        public IRigidBody B2 { get; }
        public Vector2D Anchor1 { get; private set; } //Angabe in Weltkoordinaten
        public Vector2D Anchor2 { get; private set; }
        public float LengthFactor { get; } = 1;
        public float Length { get; } //Sollwertlänge (=Anchorpoint-Distance * LengthFactor)
        public float MinLength { get; } //Minimale Länge in Prozent im Bezug zur Length-Property
        public float MaxLength { get; } //Maximale Länge in Prozent im Bezug zur Length-Property

        //Vom Nutzer vorgegebene Softness-Parameter
        public SpringParameter ParameterType { get; set; } = SpringParameter.FrequenceyAndDampingRatio;
        public float FrequencyHertz { get; } = 0; //Wie oft in der Sekunde soll die Feder schwingen
        public float DampingRatio { get; } = 0; //0 = Keine Dämping (Unendliche Schwingung); 1=Komplette Dämpfung
        public float Stiffness { get; set; } = 0; //ForceFactor k: 0=Keine Feder    -> Nur diese Parameter werden intern zur Berechnung verwendet
        public float Damping { get; set; } = 0;  //Damping coefficient c            -> Nur diese Parameter werden intern zur Berechnung verwendet



        public float AccumulatedImpulse { get; set; } = 0; //Für DistanceJointConstraint
        public float AccumulatedImpulseForMinMax { get; set; } = 0; //Für MinMaxDistanceConstraint

        public DistanceJoint(DistanceJointExportData data, List<IRigidBody> bodies)
        {
            this.B1 = bodies[data.BodyIndex1];
            this.B2 = bodies[data.BodyIndex2];
            this.r1 = data.R1;
            this.r2 = data.R2;
            this.LengthFactor = data.LengthFactor;
            this.MinLength = data.MinLength;
            this.MaxLength = data.MaxLength;

            this.ParameterType = data.ParameterType;
            this.FrequencyHertz = data.FrequencyHertz;
            this.DampingRatio = data.DampingRatio;
            this.Stiffness = data.Stiffness;
            this.Damping = data.Damping;

            if (this.ParameterType == SpringParameter.FrequenceyAndDampingRatio)
            {
                float stiffness, damping;
                JointSoftnessHelper.LinearFrequencyToStiffness(this.FrequencyHertz, this.DampingRatio, this.B1, this.B2, out stiffness, out damping);
                this.Stiffness = stiffness;
                this.Damping = damping;
            }
            else if (this.ParameterType == SpringParameter.StiffnessAndDamping)
            {
                float frequencyHertz, dampingRatio;
                JointSoftnessHelper.LinearStiffnessToFrequency(this.Stiffness, this.Damping, this.B1, this.B2, out frequencyHertz, out dampingRatio);
                this.FrequencyHertz = frequencyHertz;
                this.DampingRatio = dampingRatio;
            }

            UpdateAnchorPoints();
            this.Length = (Anchor1 - Anchor2).Length() * data.LengthFactor;            
        }

        public void UpdateAnchorPoints()
        {
            this.Anchor1 = MathHelp.GetWorldPointFromLocalDirection(this.B1, this.r1);
            this.Anchor2 = MathHelp.GetWorldPointFromLocalDirection(this.B2, this.r2);
        }

        public IExportJoint GetExportData(List<IRigidBody> bodies)
        {
            return new DistanceJointExportData()
            {
                BodyIndex1 = bodies.IndexOf(B1),
                BodyIndex2 = bodies.IndexOf(B2),
                R1 = this.r1,
                R2 = this.r2,
                LengthFactor = this.LengthFactor,
                MinLength = this.MinLength,
                MaxLength = this.MaxLength,
                ParameterType = this.ParameterType,
                FrequencyHertz = this.FrequencyHertz,
                DampingRatio = this.DampingRatio,
                Stiffness = this.Stiffness,
                Damping = this.Damping,
            };
        }
    }
}
