using DynamicObjCreation;
using GameHelper.Simulation;
using GraphicPanelWpf;
using LevelEditorGlobal;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using System;
using System.Drawing;
using System.Linq;

namespace AstroidsControl.Model
{
    //Erzeugt zufällige Astroide und Satelliten
    class AstroidCreator : ITimerHandler
    {
        private int MaxAstroidCount = 5;
        private int MaxSatellitCount = 1;
        private float MinSize = 40;
        private float MaxSize = 200;

        private string dataFolder;
        private GameSimulator simulator;
        private Random rand;
        private BodyIsInsideScreenTester astroidInsideTester;
        private BodyIsInsideScreenTester satellitInsideTester;

        private PhysikLevelItemExportData satellitExport;

        public AstroidCreator(string dataFolder, GameSimulator simulator, Random rand)
        {
            this.dataFolder = dataFolder;
            this.simulator = simulator;
            this.rand = rand;
            this.astroidInsideTester = new BodyIsInsideScreenTester(simulator);
            
            this.satellitInsideTester = new BodyIsInsideScreenTester(simulator, 100);
            this.satellitInsideTester.BodyWasDeletedHandler += SatellitInsideTester_BodyWasDeletedHandler;

            //Export-Daten vom Satelliten sich holen und dann den Satallit entfernen
            int satellitId = this.simulator.GetTagDataFromBody(this.simulator.GetBodiesByTagName("satellit").First()).LevelItemId;
            this.satellitExport = this.simulator.GetExportDataFromLevelItem(satellitId);
            this.simulator.RemoveLevelItem(satellitId);
        }

        private void SatellitInsideTester_BodyWasDeletedHandler(IPublicRigidBody obj)
        {
            int levelItemId = this.simulator.GetTagDataFromBody(obj).LevelItemId;
            this.simulator.RemoveLevelItem(levelItemId);
        }

        public int AstroidCount { get { return astroidInsideTester.Count; } }
        public int SatellitCount { get { return satellitInsideTester.Count; } }

        public void HandleTimerTick(float dt)
        {
            //Erzeuge neue Astroide
            if (this.AstroidCount < this.MaxAstroidCount && this.rand.Next(100) == 0)
            {
                var astroidData = TryToCreateAstroid();
                if (astroidData != null)
                {
                    var body = this.simulator.AddRigidBody(astroidData);
                    astroidInsideTester.AddBody(body);
                }                
            }

            //Erzeuge Satellit
            if (this.SatellitCount < this.MaxSatellitCount && this.rand.Next(500) == 0)
            {
                var satellitData = TryToCreateSatellit();
                if (satellitData != null)
                {
                    int newId = simulator.AddLevelItem(satellitData);
                    var mainBody = simulator.GetBodyByTagName(newId, "satellitMain");
                    satellitInsideTester.AddBody(mainBody);
                }
            }

            //Entferne all die Objekte, die außerhalb des Sichtbereichs sind
            this.astroidInsideTester.HandleTimerTick(dt);
            this.satellitInsideTester.HandleTimerTick(dt);
        }

        //Beispiel 1 wie ein Objekt während der Simulation erzeugt werden kann: Einzelnes Polygon
        private BodyWithTexture TryToCreateAstroid()
        {
            int maxTrys = 10;
            for (int i = 0; i < maxTrys; i++)
            {
                var box = this.simulator.GetScreenBox();
                var borderPosition = GetPointOnRectangleBorder(box, (float)rand.NextDouble());
                float size = (MaxSize - MinSize) * (float)rand.NextDouble() + MinSize;
                var boxPoint = GetPointInRectangle(box, (float)rand.NextDouble(), (float)rand.NextDouble());
                var velocity = (boxPoint - borderPosition).Normalize() * 0.1f;
                var exportData = DynamicObjCreation.AstroidCreator.CreateAstroid(borderPosition + velocity, rand, size, 25, dataFolder + "Stone.JPG", velocity, ((float)rand.NextDouble() - 0.5f) * 0.01f);
                exportData.Body.CollisionCategory = 2; //0=Ship;1=Bullet;2=Astroid
                exportData.TagColor = 2;
                if (this.simulator.GetCollisionPointsFromExternBodyWithScene(exportData.Body).Length == 0)
                {
                    return exportData;
                }
            }

            return null;
        }

        //Beispiel 2 zur Objekterzeugung: Komplexes Objekt, was per Kopie von ein im LevelEditor erstellten LevelItem erzeugt wird 
        private PhysikLevelItemExportData TryToCreateSatellit()
        {
            var box = this.simulator.GetScreenBox();
            var borderPosition = GetPointOnRectangleBorder(box, (float)rand.NextDouble());
            var boxPoint = GetPointInRectangle(box, (float)rand.NextDouble(), (float)rand.NextDouble());
            var velocity = (boxPoint - borderPosition).Normalize() * 0.1f;
            var copyData = new PhysikLevelItemExportData(this.satellitExport); //Kopie vom Original-Export erstellen
            float angleInDegree = Vec2D.Angle360(new Vec2D(-1, 0), velocity);
            LevelItemExportHelper.MoveToPivotPoint(copyData, (borderPosition + velocity).ToGrx(), LevelItemExportHelper.PivotOriantation.Center, 1, angleInDegree); //Kopie bearbeiten
            LevelItemExportHelper.SetVelocityFromAllBodies(copyData, velocity);
            if (this.simulator.GetCollisionPointsFromExternLevelItemWithScene(copyData).Length > 0) return null;
            return copyData;
        }

        //f = 0..1 (0..0.25 = Top; 0.25..0.5=Right; 0.5..0.75=Bottom; 0.75..1=Left)
        private Vec2D GetPointOnRectangleBorder(RectangleF box, float f)
        {
            if (f < 0 || f > 1) throw new ArgumentException();

            if (f >= 0 && f < 0.25f) return new Vec2D(box.X + box.Width * (f / 0.25f), box.Y);                  //Top
            if (f >= 0.25f && f < 0.5f) return new Vec2D(box.Right, box.Y + box.Height * (f - 0.25f) / 0.25f);  //Right
            if (f >= 0.5f && f < 0.75f) return new Vec2D(box.Right - box.Width * (f-0.5f) / 0.25f, box.Bottom); //Bottom
            if (f >= 0.75f && f < 1f) return new Vec2D(box.Left, box.Bottom - box.Height * (f - 0.75f) / 0.25f);//Left

            throw new NotImplementedException();
        }

        //f1/f2 = 0..1
        private Vec2D GetPointInRectangle(RectangleF box, float f1, float f2)
        {
            return new Vec2D(box.X + box.Width * f1, box.Y + box.Height * f2);
        }
    }
}
