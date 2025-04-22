using KeyFrameGlobal;
using RigidBodyPhysics.ExportData;
using RigidBodyPhysics;

namespace KeyFramePhysicImporter.Model
{
    //Wenn man eine Animation für eine PhysicScene erstellt, dann definiert man im Importer welcehs Objekt fix ist und 
    //kann dann jede Play-Position anspringen ohne dass das PhysicScene-Objekt wegspringt.
    //Wenn man dann diese Animation nutzen will, dann springt die PhysicScene vom Initialzustand, wie es der PhysicEditor
    //erzeugt hat zur Time=0-Position. Um zu verhindern, dass im ersten TimeStep in der Simulation ein Sprung entsteht
    //erzeugt diese Klasse hier ein PhysicScene-Objekt, wie es entsteht wenn man an Play-Posiiton=0 steht aber dabei
    //bleibt das Objekt, was im Importer als fix markiert wurde unverändert an der Stelle, wie es der PhysicEditor vorgegeben hat.
    public static class PhysicSceneStartValueCreator
    {
        //isFix = Diese Information kommt vom Importer aus ImporterControlExportData.IsFix
        //scene = So wurde die Scene dem KeyFramePhysicImporter als Input gegeben
        //initialTimeForManually = 0..1 Von dieser Play-Position soll eine PhysicScene erstellt werden
        public static PhysicSceneExportData CreateSceneAfterPlayingNTimeSteps(bool[] isFix, PhysicSceneExportData scene, AnimationOutputData[] animations, float timerIntercallInMilliseconds, int timeStepCount, int iterationCount)
        {
            if (isFix.Length != scene.Bodies.Length)
                throw new ArgumentException("Body.Count must be isFix.Length");

            //Schritt 1: Merke, welchen MassType-Zustand die Objekte vorher haben
            var exportInput = new PhysicScene(scene).GetExportData(); //Erzeuge eine Kopie vom Input um ihn nicht zu verändern
            var massDataInput = exportInput.Bodies.Select(x => x.MassData.Type).ToArray();

            //Schritt 2: Wandle all die Bodys in Fix um, welche laut isFix so sein sollen
            for (int i = 0; i < isFix.Length; i++)
            {
                if (isFix[i])
                    exportInput.Bodies[i].MassData.Type = RigidBodyPhysics.ExportData.RigidBody.MassData.MassType.Infinity;
            }

            //Schritt 3: Erzeuge ein neues PhysicScene-Objekt und springe an die initialTimeForManually damit
            var fixPhysicScene = new PhysicScene(exportInput);
            fixPhysicScene.IterationCount = iterationCount;
            var animatedScene = new AnimatedPhysicObjects(fixPhysicScene.GetAllPublicData(), animations, timerIntercallInMilliseconds);
            for (int i = 0; i < timeStepCount; i++)
            {
                animatedScene.HandleTimerTick(timerIntercallInMilliseconds);
                fixPhysicScene.TimeStep(timerIntercallInMilliseconds);
            }
            var fixSceneExport = fixPhysicScene.GetExportData();

            //Schritt 4: Erstelle eine neue Scene aber mit korrigierten Körperpositionen
            var correctedScene = new PhysicScene(scene).GetExportData();
            for (int i = 0; i < correctedScene.Bodies.Length; i++)
            {
                var b1 = correctedScene.Bodies[i];
                var b2 = fixSceneExport.Bodies[i];

                b1.MassData.Type = massDataInput[i];
                b1.Center = b2.Center;
                b1.AngleInDegree = b2.AngleInDegree;
                b1.Velocity = new RigidBodyPhysics.MathHelper.Vec2D(0, 0);
                b1.AngularVelocity = 0;
            }

            return correctedScene;
        }
    }
}
