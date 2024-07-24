namespace PhysicEngine.CollisionResolution
{
    internal class SolverSettings
    {
        internal bool DoPositionalCorrection = true; //Soll nach jeden TimeStep die Kollision dadurch aufgelößt werden indem die Position laut Kollisionsnormale versetzt wird?
        internal float PositionalCorrectionRate = 0.2f; //0 = Keine Korrektur; 1 = Nach ein TimeStep ist die Kollision weg (So viel Prozent wird pro TimeStep die Position korrigiert)
        internal float AllowedPenetration = 1.0f; //So viele Pixel dürfen sich zwei Körper überlappen ohne dass ein Korrekturimpuls angewendet wird. Damit werden stabile RestingContacts erzeugt
        internal int IterationCount = 50;  //So viele PGS-Iterationen werden verwendet um Lambda zu ermitteln (Stärke des Impulses)
        internal bool DoWarmStart = true;  //Soll die aufsummierte Impulsenergie über alle vorherigen TimeSteps am Anfang des TimeSteps angewendet werden?
        internal float Gravity = 0.001f; //Schwerkraft in Y-Richtung
    }
}
