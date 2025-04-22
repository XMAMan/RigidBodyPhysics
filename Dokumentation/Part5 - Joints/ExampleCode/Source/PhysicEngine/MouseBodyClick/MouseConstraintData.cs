using PhysicEngine.CollisionResolution.SequentiellImpulse;
using PhysicEngine.Joints;
using PhysicEngine.MathHelper;
using PhysicEngine.RigidBody;

namespace PhysicEngine.MouseBodyClick
{
    //Exisistiert so lange die Maustaste unten ist
    internal class MouseConstraintData
    {
        private MouseClickData clickData; //Wird beim Mouse-Down-Event von der PhysicScene erzeugt

        internal Vec2D MousePosition { get; private set; }   //Wird beim Mouse-Move-Event aktualisiert
        internal Vec2D AccumulatedImpulse = new Vec2D(0, 0);


        internal IRigidBody RigidBody {get; private set;}
        internal Vec2D LocalAnchorDirection { get => this.clickData.LocalAnchorDirection; }
        internal float MaxForce { get; }
        internal SoftConstraintData Soft { get; }

        internal MouseConstraintData(MouseClickData mouseClickData, MouseConstraintUserData userData) 
        { 
            this.clickData = mouseClickData;
            this.RigidBody = mouseClickData.RigidBody as IRigidBody;
            this.MaxForce = userData.MaxForce;
            this.Soft = new SoftConstraintData(userData.SoftData, RigidBody, null);
            this.MousePosition = mouseClickData.Position;
        }

        internal void UpdateMousePosition(Vec2D position)
        {
            this.MousePosition = position;
        }
    }
}
