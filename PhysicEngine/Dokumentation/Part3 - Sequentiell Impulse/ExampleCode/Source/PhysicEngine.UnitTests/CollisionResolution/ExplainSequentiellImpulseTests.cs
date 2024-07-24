using FluentAssertions;
using Xunit;

namespace PhysicEngine.UnitTests.CollisionResolution
{
    //Hier wird erklärt, wie Sequentielle Impulse funktioniert
    public class ExplainSequentiellImpulseTests
    {
        private static string TestData = @"..\..\..\..\..\Data\CollisionResolutionTestcases\6_ExplainSequentiellImpulse\";
        private static float TimeStepTickRate = 50; //[ms]

        //Hier wird ein vereinfachter SI-Algorithmus gezeigt, der nur eine Normalconstraint hat, kein WarmStart, 
        //keine Schwerkraft, kein AccumulatedImpulse.
        //Durch diese ganze Vereinfachungen sieht das Verfahren nun aus wie Iterative Impulse aber der Würfel
        //prallt rotationsfrei von der Wand ab. Der Grund dafür ist das SI die Bias-Werte einmal am Anfang 
        //berechnet und II pro ApplyImpulse-Step
        [Fact]
        public void Example1_DoItLikeIterativeImpulse()
        {
            var result = SingleTimeStepCubeAgainstWall(PhysicScene.SolverType.SI_Easy1);

            //Prüfe dass der Würfel rotationsfrei an der Wand abprallt
            result.CubeVelocity.Should().Be(-result.CubeVelocityBefore);
            result.CubeAngularVelocity.Should().Be(0);
  
            result.ImpulseSI[0].Should().Be(result.ImpulseMatrix[0]); //Normal-Impuls für den oberen Kontaktpunkt
            result.ImpulseSI[1].Should().Be(result.ImpulseMatrix[1]); //Normal-Impuls für den unteren Kontaktpunkt
        }


        //Das hier macht das gleiche wie SI_Easy1 nur dass zusätzlich noch die FrictionConstraint genutzt wird.
        //Hier sieht man, dass der Würfel nun wegen falschen Impulsclamping falsche Impulse berechnet so dass er 
        //dann mit Drehung abprallt 
        [Fact] //-> Es ist beabsichtigt dass der Test rot wird da hier gezeigt wird, wie es nicht geht
        public void Example2_ExpandWithFriction()
        {
            var result = SingleTimeStepCubeAgainstWall(PhysicScene.SolverType.SI_Easy2);

            //Prüfe dass der Würfel rotationsfrei an der Wand abprallt
            result.CubeVelocity.Should().Be(-result.CubeVelocityBefore);
            result.CubeAngularVelocity.Should().Be(0);

            result.ImpulseSI[0].Should().Be(result.ImpulseMatrix[0]); //Normal-Impuls für den oberen Kontaktpunkt
            result.ImpulseSI[1].Should().Be(result.ImpulseMatrix[1]); //Normal-Impuls für den unteren Kontaktpunkt
            result.ImpulseSI[2].Should().Be(result.ImpulseMatrix[2]); //Friction-Impuls für den oberen Kontaktpunkt
            result.ImpulseSI[3].Should().Be(result.ImpulseMatrix[3]); //Friction-Impuls für den unteren Kontaktpunkt
        }

        //Erweitert SI_Easy2 um die Nutzung von AccumulatedImpulse. Diesmal erfolgt das Clamping nicht mit den
        //Zwischenimpuls sondern mit der Impulse-Summe. Somit kommt jetzt der richtige Impuls herraus. 
        [Fact]
        public void Example3_ExpandWithAccumulatedImpulse()
        {
            var result = SingleTimeStepCubeAgainstWall(PhysicScene.SolverType.SI_Easy3);

            //Prüfe dass der Würfel rotationsfrei an der Wand abprallt
            result.CubeVelocity.Should().Be(-result.CubeVelocityBefore);
            result.CubeAngularVelocity.Should().Be(0);

            result.ImpulseSI[0].Should().Be(result.ImpulseMatrix[0]); //Normal-Impuls für den oberen Kontaktpunkt
            result.ImpulseSI[1].Should().Be(result.ImpulseMatrix[1]); //Normal-Impuls für den unteren Kontaktpunkt
            result.ImpulseSI[2].Should().Be(result.ImpulseMatrix[2]); //Friction-Impuls für den oberen Kontaktpunkt
            result.ImpulseSI[3].Should().Be(result.ImpulseMatrix[3]); //Friction-Impuls für den unteren Kontaktpunkt
        }

        //Erweitert SI_Easy3 um die Anwendung der externen Kraft (=Schwerkraft). Hier wird zuerst die externe Kraft 
        //angewendet und danach dann die Constraint-Objekte erstellt. 
        //Hier fällt ein Würfel auf ein Tisch und er springt ein paar mal hoch und runter. Erwartung: Die maximal erreichte 
        //Höhe ist kleiner als die Start-Höhe. Weil aber die externe Kraft die Bias-Werte verändert, springt der Würfel zu hoch
        [Fact]   //-> Es ist beabsichtigt dass der Test rot wird da hier gezeigt wird, wie es nicht geht
        public void Example4_ExternForceBeforeConstraintCreation()
        {
            float[] cubeHeights = MultipleTimeStepsCubeFallsOnTable(PhysicScene.SolverType.SI_Easy4).TakeLast(10).ToArray();

            //Prüfe das der Würfel ruhig auf dem Tisch liegt
            (cubeHeights.Max() - cubeHeights.Min()).Should().Be(0);
        }

        //Ähnlich wie Example4 nur diesmal erfolgt die externe Kraft nach der Constraint-Erstellung und vor der Constraint-Kraft-Anwendung
        //Ein Würfel fällt aus geringer Höhe auf ein Tisch und kommt dort zur Ruhe
        [Fact]
        public void Example5_ExternForceAfterConstraintCreation()
        {
            float[] cubeHeights = MultipleTimeStepsCubeFallsOnTable(PhysicScene.SolverType.SI_Easy5).TakeLast(10).ToArray();

            //Prüfe das der Würfel ruhig auf dem Tisch liegt
            (cubeHeights.Max() - cubeHeights.Min()).Should().Be(0);
        }

        //Ähnlich wie Example4 nur diesmal erfolgt die externe Kraft nach der Constraint-Kraft
        //Ein Würfel fällt aus geringer Höhe auf ein Tisch und fällt durch den Tisch hindurch
        [Fact] //-> Es ist beabsichtigt dass der Test rot wird da hier gezeigt wird, wie es nicht geht
        public void Example6_ExternForceAfterConstraintForce()
        {
            float[] cubeHeights = MultipleTimeStepsCubeFallsOnTable(PhysicScene.SolverType.SI_Easy6).TakeLast(10).ToArray();

            //Prüfe das der Würfel ruhig auf dem Tisch liegt
            (cubeHeights.Max() - cubeHeights.Min()).Should().Be(0);
        }

        #region Testhelper for Example1 .. Example3
        class CubeAgainstWallResult
        {
            public float CubeVelocityBefore;
            public float CubeVelocity;
            public float CubeAngularVelocity;
            public float[] ImpulseMatrix; //Diese Impulse hat die Matrix pro Constraint berechnet
            public float[] ImpulseSI; //Diese Impulse hat Sequentielle Impulse pro Constraint berechnet
        }

        private static CubeAgainstWallResult SingleTimeStepCubeAgainstWall(PhysicScene.SolverType solver)
        {
            int cube = 0;
            var bodys = ExportHelper.ReadFromFile(TestData + "CubeAgainstWall.txt");

            float cubeVelocityBefore = bodys[cube].Velocity.X;

            var scene = new PhysicScene(bodys);
            scene.Solver = solver;
            scene.DoPositionalCorrection = scene.DoWarmStart = scene.HasGravity = false;
            scene.IterationCount = 100;

            var lambda = scene.GetProjectedGaussSeidelSteps(TimeStepTickRate, scene.IterationCount).Last();
            float[] impulseMatrix = (lambda * TimeStepTickRate).GetColum(0); //Sollwert

            scene.TimeStep(TimeStepTickRate);

            float[] impulseSI = scene.GetLastAppliedImpulsePerConstraint();

            int roundDecimalPlaces = 7;

            return new CubeAgainstWallResult()
            {
                CubeVelocityBefore = cubeVelocityBefore,
                CubeVelocity = bodys[cube].Velocity.X,
                CubeAngularVelocity = MathHelp.Round(bodys[cube].AngularVelocity, roundDecimalPlaces),
                ImpulseMatrix = MathHelp.Round(impulseMatrix, roundDecimalPlaces),
                ImpulseSI = MathHelp.Round(impulseSI, roundDecimalPlaces),
            };
        }
        #endregion

        #region Testhelper for Example4..Example6

        private static float[] MultipleTimeStepsCubeFallsOnTable(PhysicScene.SolverType solver)
        {
            int cube = 1;
            var bodys = ExportHelper.ReadFromFile(TestData + "CubeFallsOnTable.txt");

            var scene = new PhysicScene(bodys);
            scene.Solver = solver;
            scene.DoPositionalCorrection = scene.DoWarmStart = false;
            scene.HasGravity = true;
            scene.IterationCount = 100;

            List<float> cubeHeights = new List<float>();

            int timeStepCount = 200;
            for (int i=0;i<timeStepCount;i++)
            {
                scene.TimeStep(TimeStepTickRate);
                cubeHeights.Add(bodys[cube].Center.Y);
            }

            return cubeHeights.ToArray();
        }

        #endregion
    }
}
