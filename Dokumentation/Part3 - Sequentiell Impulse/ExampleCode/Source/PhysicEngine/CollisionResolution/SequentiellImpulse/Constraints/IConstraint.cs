using GraphicMinimal;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints
{
    internal interface IConstraint
    {
        //Schritt 1: Erzeuge über den Konstruktor folgende Propertys
        Vector2D ForceDirection { get; } //In diese Richtung wird B2 gedrückt (B1 wird entgegengesetzt gedrückt)
        Vector2D R1 { get; } //Hebelarm vom B1.Center zum Kontaktpunkt
        Vector2D R2 { get; } //Hebelarm vom B2.Center zum Kontaktpunkt
        IRigidBody B1 { get; }
        IRigidBody B2 { get; }
        float Bias { get; } //Zielvorgabe für die Relativegeschwindigkeit der Kontaktpunkte
        float ImpulseMass { get; } //Umrechungsvektor vom Relative-Kontaktpunktgeschwindigkeitswert in ein Impuls
        float MinImpulse { get; }
        float MaxImpulse { get; }

        //Schritt 2: Impulse anwenden
        float AccumulatedImpulse { get; set; }
        void SaveImpulse(); //Speichert den Impulse im CollisionPointWithImpulse
    }
}
