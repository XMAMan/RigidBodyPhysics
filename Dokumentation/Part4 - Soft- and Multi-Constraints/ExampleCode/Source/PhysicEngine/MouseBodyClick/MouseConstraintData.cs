using GraphicMinimal;
using PhysicEngine.CollisionResolution.SequentiellImpulse;
using PhysicEngine.RigidBody;

namespace PhysicEngine.MouseBodyClick
{
    //Exisistiert so lange die Maustaste unten ist
    internal class MouseConstraintData
    {
        private MouseClickData clickData; //Wird beim Mouse-Down-Event von der PhysicScene erzeugt
        private MouseConstraintUserData userData; //Wird beim Mouse-Down-Event vom Nutzer vorgegeben

        public Vector2D MousePosition { get; private set; }   //Wird beim Mouse-Move-Event aktualisiert
        public Vector2D AccumulatedImpulse = new Vector2D(0, 0);

        //Softness-Parameter
        public readonly bool IsSoftConstraint = false;
        public readonly float Stiffness = 0; //ForceFactor k: 0=Keine Feder    -> Nur diese Parameter werden intern zur Berechnung verwendet
        public readonly float Damping = 0;  //Damping coefficient c            -> Nur diese Parameter werden intern zur Berechnung verwendet


        public IRigidBody RigidBody { get => this.clickData.RigidBody; }
        public Vector2D LocalAnchorDirection { get => this.clickData.LocalAnchorDirection; }
        public float MaxForce { get => this.userData.MaxForce; }

        public MouseConstraintData(MouseClickData mouseClickData, MouseConstraintUserData userData) 
        { 
            this.clickData = mouseClickData;
            this.userData = userData;
            this.MousePosition = mouseClickData.Position;

            if (userData.FrequencyHertz > 0)
            {
                this.IsSoftConstraint = true;
                JointSoftnessHelper.LinearFrequencyToStiffness(userData.FrequencyHertz, userData.DampingRatio, mouseClickData.RigidBody, null, out this.Stiffness, out this.Damping);
            }
        }

        public void UpdateMousePosition(Vector2D position)
        {
            this.MousePosition = position;
        }
    }
}
