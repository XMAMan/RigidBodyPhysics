using PhysicEngine.CollisionResolution.EnterTheMatrix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PhysicEngine.UnitTests.CollisionResolution
{
    //Gegeben sind viele Körper die aufeinander gestapelt sind und die bereits in Ruhe sind
    //Damit sie in Ruhe bleibt ist es wichtig die korrekten Kräfte für die Kollisionspunkte zu bestimmen
    //Hier soll untersucht werden, wie schnell Projected-Gauss-Seidel den richtigen Lambda-Vektor ermittelt
    public class PGSConvergenceTest
    {
        private static string TestData = @"..\..\..\..\..\Data\CollisionResolutionTestcases\5_PGSConvergence\";
        private static float TimeStepTickRate = 50; //[ms]

        //Ich habe die Daten von hier dann mit OpenCalc visualisiert
        [Fact]
        public void GetPGSErrorCurveFromAllTestCases()
        {
            string e1 = DoTest(TestData + "FunnelBalls.txt", 1000);
            string e2 = DoTest(TestData + "CircleStack.txt", 1000);
            string e3 = DoTest(TestData + "CubeStack.txt", 1000);
            string e4 = DoTest(TestData + "Pyramid.txt", 1000);
        }

        //Berechnet über iterations Schritte mit PGS die Lösung der Bewegungsgleichung
        //Alle Zwischenergebnisse werden dann gegen den letzten Schritt verglichen (Abweichung ermittelt)
        private static string DoTest(string fileName, int iterations)
        {
            var bodys = ExportHelper.ReadFromFile(fileName);
            var scene = new PhysicScene(bodys);
            scene.UseGlobalSolver();
            scene.DoPositionalCorrection = true;
            scene.HasGravity = true;

            var steps = scene.GetProjectedGaussSeidelSteps(TimeStepTickRate, iterations);
            float[] errors = steps.Select(x => (x - steps.Last()).GetSqrtLength()).ToArray();
            string e = string.Join("\n", errors.Select(x => x * 1000000000)); //OpenOffice kann keine Zahlen einlesen, wo ein E drin vorkommt
            return e;
        }
    }
}
