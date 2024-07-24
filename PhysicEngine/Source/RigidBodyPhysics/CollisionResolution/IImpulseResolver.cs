namespace RigidBodyPhysics.CollisionResolution
{
    //Verändert die Geschwindigkeiten so, dass die Kontaktpunkte/Constraint-Verletzungen aufgelößt werden
    internal interface IImpulseResolver
    {
        void Resolve(SolverInputData data);
    }
}
