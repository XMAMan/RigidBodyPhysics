using GraphicMinimal;
using PhysicEngine.RigidBody;

namespace PhysicEngine.MouseBodyClick
{
    internal interface IClickableBodyList
    {
        MouseClickData TryToGetBodyWithMouseClick(Vector2D mousePosition);
        void SetMouseConstraint(MouseClickData mouseClick, MouseConstraintUserData userData); //Maus wird geklickt und hält ein Körper fest        
        void ClearMouseConstraint(); //Maus wird losgelassen
        void UpdateMousePosition(Vector2D mousePosition);
    }

    //Wird beim Mouse-Down-Event von der PhysicSecne erzeugt erzeugt
    public class MouseClickData
    {
        public IRigidBody RigidBody;
        public Vector2D LocalAnchorDirection;       //Angabe im Lokal-Space
        public Vector2D Position; //Position des Mausklicks in Weltkoordinaten
    }

    //Legt der Nutzer fest, wenn er mit der Maus ein Objekt festelegen will
    public class MouseConstraintUserData
    {
        public readonly float MaxForce; //Mit so viel Kraft zieht die Maus maximal am Ankerpunkt
        public readonly float FrequencyHertz; //0 = Ankerpunkt springt sofort zum Mauszeiger ohne Dämpfung
        public readonly float DampingRatio; //0..1 (0=No Damping; 1=Max-Damping)

        private MouseConstraintUserData(float maxForce, float frequencyHertz, float dampingRatio) 
        {
            if (maxForce < 0) throw new ArgumentOutOfRangeException("maxForce must be greater 0");
            if (frequencyHertz < 0) throw new ArgumentOutOfRangeException("frequencyHz must be greater 0");
            if (dampingRatio < 0 || dampingRatio > 1) throw new ArgumentOutOfRangeException("dampingRatio must be in range 0..1");

            MaxForce = maxForce;
            FrequencyHertz = frequencyHertz;
            DampingRatio = dampingRatio;
        }

        public static MouseConstraintUserData CreateWithoutDamping(float maxForce = 0.1f)
        {
            return new MouseConstraintUserData(maxForce, 0, 0);
        }

        public static MouseConstraintUserData CreateWithDamping(float maxForce = 0.1f, float frequencyHz = 5, float dampingRatio = 0.7f)
        {
            return new MouseConstraintUserData(maxForce, frequencyHz, dampingRatio);
        }
    }
}
