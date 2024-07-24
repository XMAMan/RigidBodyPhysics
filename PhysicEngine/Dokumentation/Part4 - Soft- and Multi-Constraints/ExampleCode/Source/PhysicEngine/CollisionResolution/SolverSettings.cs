namespace PhysicEngine.CollisionResolution
{
    internal class SolverSettings
    {
        public bool DoPositionalCorrection = true; //Soll nach jeden TimeStep die Kollision dadurch aufgelößt werden indem die Position laut Kollisionsnormale versetzt wird?
        public float PositionalCorrectionRate = 0.2f; //0 = Keine Korrektur; 1 = Nach ein TimeStep ist die Kollision weg (So viel Prozent wird pro TimeStep die Position korrigiert)
        public float AllowedPenetration = 1.0f; //So viele Pixel dürfen sich zwei Körper überlappen ohne dass ein Korrekturimpuls angewendet wird. Damit werden stabile RestingContacts erzeugt
        public int IterationCount = 10;  //So viele PGS-Iterationen werden verwendet um Lambda zu ermitteln (Stärke des Impulses)
        public bool DoWarmStart = true;  //Soll die aufsummierte Impulsenergie über alle vorherigen TimeSteps am Anfang des TimeSteps angewendet werden?
        public float Gravity = 0.001f; //Schwerkraft in Y-Richtung
    }
}
