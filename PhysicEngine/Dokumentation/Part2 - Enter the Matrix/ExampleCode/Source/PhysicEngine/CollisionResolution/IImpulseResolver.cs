using PhysicEngine.CollisionDetection;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution
{
    //Verändert die Geschwindigkeiten so, dass die Kontaktpunkte/Constraint-Verletzungen aufgelößt werden
    internal interface IImpulseResolver
    {
        void Resolve(List<IRigidBody> bodies, RigidBodyCollision[] collisions, float dt, SolverSettings settings);
    }
}
