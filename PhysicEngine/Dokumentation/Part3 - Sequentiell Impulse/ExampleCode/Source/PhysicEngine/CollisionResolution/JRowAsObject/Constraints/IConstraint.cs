using GraphicMinimal;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.JRowAsObject.Constraints
{
    //Érzeugt eine Constraint-Kraft zwischen zwei Körpern. 
    internal interface IConstraint
    {
        //Schritt 1: Erzeuge über den Konstruktor folgende Propertys
        Vector3D J1 { get; } //Die 3 J-Werte von Körper 1
        Vector3D J2 { get; } //Die 3 J-Werte von Körper 2
        Vector3D[] JRow { get; } //Array der Länge Bodies.Length wo nur 2 Einträge ein Wert haben
        IRigidBody B1 { get; }
        IRigidBody B2 { get; }
        int Body1Index { get; }
        int Body2Index { get; }
        float Bias { get; }
        float MinLambda { get; }
        float MaxLambda { get; }


        //Schritt 2: Erzeuge durch Aufruf von CreateARowAndBValue die A-Zeile und den B-Spaltenwert
        float[] ARow { get; set; } //Zeile aus der A-Matrix
        float B { get; set; } //Einzelner Wert aus dem B-Spaltenvektor


        //Schritt 3: Berechne per PGS Lambda
        float Lambda {get; set;}
        void SaveLambda(); //Speichert das Lambda im CollisionPointWithLambda
    }
}
