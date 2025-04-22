using FluentAssertions;
using System.Text;
using Xunit;

namespace PhysicEngine.UnitTests.CollisionResolution
{
    //Vergleicht wie gut die verschiedenen Constraint-Solver sind
    public class SolverCompareTest
    {
        private static string TestData = @"..\..\..\..\..\Data\CollisionResolutionTestcases\3_PositionCorrection\";
        private static string SourceCode = @"..\..\..\..\..\Source\PhysicEngine\CollisionResolution\";
        private static float TimeStepTickRate = 50; //[ms]

        //Hiermit vergleich ich die verschiednen Solver wie schnell sie sind
        [Fact(Skip = "Der Test dauert zu lange (13 Sekunden)")]
        public void CompareExecutionTime()
        {
            float timeMatrix1 = GetExecutionTimt(PhysicScene.SolverType.Matrix, 10, 100);
            float timeSI1 = GetExecutionTimt(PhysicScene.SolverType.SequentiellImpulse, 10, 100);
            float timeJRow1 = GetExecutionTimt(PhysicScene.SolverType.JRowAsObject, 10, 100);

            float timeMatrix2 = GetExecutionTimt(PhysicScene.SolverType.Matrix, 20, 20);
            float timeSI2 = GetExecutionTimt(PhysicScene.SolverType.SequentiellImpulse, 20, 20);
            float timeJRow2 = GetExecutionTimt(PhysicScene.SolverType.JRowAsObject, 20, 20);

            float timeMatrix3 = GetExecutionTimt(PhysicScene.SolverType.Matrix, 30, 10);
            float timeSI3 = GetExecutionTimt(PhysicScene.SolverType.SequentiellImpulse, 30, 10);
            float timeJRow3 = GetExecutionTimt(PhysicScene.SolverType.JRowAsObject, 30, 10);

            float minTime1 = Math.Min(Math.Min(timeMatrix1, timeSI1), timeJRow1);
            float minTime2 = Math.Min(Math.Min(timeMatrix2, timeSI2), timeJRow2);
            float minTime3 = Math.Min(Math.Min(timeMatrix3, timeSI3), timeJRow3);

            StringBuilder str = new StringBuilder();
            str.AppendLine("Matrix\t" + timeMatrix1.ToString("0.00") + "s (" + (timeMatrix1 / minTime1).ToString("0.00") + ")\t" + timeMatrix2.ToString("0.00") + "s (" + (timeMatrix2 / minTime2).ToString("0.00") + ")\t" + timeMatrix3.ToString("0.00") + "s (" + (timeMatrix3 / minTime3).ToString("0.00") + ")");
            str.AppendLine("Sequentielle Impulse\t" + timeSI1.ToString("0.00") + "s (" + (timeSI1 / minTime1).ToString("0.00") + ")\t" + timeSI2.ToString("0.00") + "s (" + (timeSI2 / minTime2).ToString("0.00") + ")\t" + timeSI3.ToString("0.00") + "s (" + (timeSI3 / minTime3).ToString("0.00") + ")");
            str.AppendLine("JRow\t" + timeJRow1.ToString("0.00") + "s (" + (timeJRow1 / minTime1).ToString("0.00") + ")\t" + timeJRow2.ToString("0.00") + "s (" + (timeJRow2 / minTime2).ToString("0.00") + ")\t" + timeJRow3.ToString("0.00") + "s (" + (timeJRow3 / minTime3).ToString("0.00") + ")");
            string result = str.ToString();
        }

        [Fact]
        public void CompareLinesOfCode()
        {
            int linesMatrix = GetLineOfCodes(SourceCode + "EnterTheMatrix");
            int linesJRow = GetLineOfCodes(SourceCode + "JRowAsObject");
            int linesSI = GetLineOfCodes(SourceCode + "SequentiellImpulse");
            int linesII = GetLineOfCodes(SourceCode + "IterativeImpulse");

            int max = Math.Max(Math.Max(Math.Max(linesMatrix, linesJRow), linesSI), linesII);

            StringBuilder str = new StringBuilder();
            str.AppendLine("Matrix: " + linesMatrix + " (" + (linesMatrix * 100 / max)+"%)");
            str.AppendLine("JRow: " + linesJRow + " (" + (linesJRow * 100 / max) + "%)");
            str.AppendLine("SI: " + linesSI + " (" + (linesSI * 100 / max) + "%)");
            str.AppendLine("IterativeImpulse: " + linesII + " (" + (linesII * 100 / max) + "%)");
            string result = str.ToString();
        }

        private static float GetExecutionTimt(PhysicScene.SolverType solver, int testRuns, int pgsIterations)
        {
            DateTime start = DateTime.Now;
            for (int i = 0; i < 20; i++)
            {
                var result = SimulateSeveralTimeSteps.DoTest(TestData + "LongCubeStack.txt", TimeStepTickRate, true, 20, 500, new SimulateSeveralTimeSteps.ExtraSettings() { DoPositionCorrection = true, Solver = solver });

                //Gibt zurück, wie viele TimeSteps der letzte zur Ruhe kommende Körper braucht, um die Ruhelage zu erreichen
                int maxCalm = SimulateSeveralTimeSteps.GetTimeStepCountForGettingCalmForEachBody(result.TimeSteps).Max(); //PGS-ItertionCount=100 -> [0]=0 [1]=23 [2]=29 [3]=29 [4]=30 [5]=49

                //maxCalm.Should().BeLessThanOrEqualTo(60); //Spätestens nach 60 TimeSteps soll jeder Körper in Ruhelage sein

                result.TimeSteps.Last().Bodies.Select(x => x.Position.Y).ToArray().Should().BeInDescendingOrder(); //Prüfe das der Turm nicht umgefallen ist
            }
            return (float)(DateTime.Now - start).TotalSeconds;
        }

        private static int GetLineOfCodes(string folder)
        {
            var files = Directory.GetFiles(folder, "*.cs", SearchOption.AllDirectories).ToArray();
            return files.Where(x => x.Contains("Examples") == false).Sum(x => File.ReadAllLines(x).Length);
        }
    }
}
