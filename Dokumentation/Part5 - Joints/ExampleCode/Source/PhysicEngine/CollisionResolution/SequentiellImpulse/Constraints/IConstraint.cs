using PhysicEngine.MathHelper;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints
{
    internal interface IConstraint
    {
        IRigidBody B1 { get; }
        IRigidBody B2 { get; }
        Vec2D R1 { get; } //Hebelarm vom B1.Center zum Kontaktpunkt
        Vec2D R2 { get; } //Hebelarm vom B2.Center zum Kontaktpunkt
        float MinImpulse { get; }
        float MaxImpulse { get; }

        void SaveImpulse(); //Speichert den Impulse im CollisionPointWithImpulse oder IJoint
        void ApplyWarmStartImpulse();
        void DoSingleSIStep();
    }

    //Es wirkt eine Kraft zwischen zwei Ankerpunkten / Kontaktpunkten entlang der Richtung ForceDirection (Linearer 1D-Impuls)
    internal interface ILinear1DConstraint : IConstraint
    {
        Vec2D ForceDirection { get; } //In diese Richtung wird B2 gedrückt (B1 wird entgegengesetzt gedrückt)
        float Bias { get; } //Zielvorgabe für die Relativegeschwindigkeit der Kontaktpunkte
        float ImpulseMass { get; } //Umrechungsvektor vom Relative-Kontaktpunktgeschwindigkeitswert in ein Impuls (Entspricht InverseK)
        float AccumulatedImpulse { get; set; }
    }

    //Linearer 2D-Impuls
    internal interface ILinear2DConstraint : IConstraint
    {
        Matrix2x2 InverseK { get; }     //K=J*M^-1*J^T
        Vec2D GetCDot();                //Gibt CDot=J*V zurück (Geschwindigkeit in Richtung jeder J-Zeile)
        Vec2D Bias { get; }
        Vec2D AccumulatedImpulse { get; set; }
    }

    internal interface IAngularConstraint : IConstraint
    {
        float Bias { get; } //Zielvorgabe für die relative Winkelgeschwindigkeit zwischen Body1 und Body2
        float ImpulseMass { get; } //Umrechungsvektor vom Relative-Winkelgeschwindigkeit in ein Drehimpuls
        float AccumulatedImpulse { get; set; }
    }


    //Wenn eine Constraint das ISoftConstraint-Interface implementiert, dann heißt dass, sie kann Soft sein. Muss es aber nicht.
    internal interface ISoftConstraint
    {
        bool IsSoftConstraint { get; } //Nur wenn hier true steht, ist die Constraint wirklich soft. Ansonsten gilt sie als Stiff
        float Gamma { get; }
        float Beta { get; }
    }

    internal interface ISoftConstraint1D : ISoftConstraint, ILinear1DConstraint
    {
        float PositionError { get; }
    }
    internal interface ISoftConstraint2D : ISoftConstraint, ILinear2DConstraint
    {
        Vec2D PositionError { get; }    //Entspricht dem PositionConstraint-Wert
    }
    internal interface ISoftConstraint3D : ISoftConstraint, IConstraint
    {
        Vec3D PositionError { get; }    //Entspricht dem PositionConstraint-Wert
    }
    internal interface ISoftAngular : ISoftConstraint, IAngularConstraint
    {
        float PositionError { get; }
    }


    
}
