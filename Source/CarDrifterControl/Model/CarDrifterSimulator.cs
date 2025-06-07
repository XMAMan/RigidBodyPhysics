using GameHelper.Simulation;
using LevelEditorExports.Simulator;
using PhysicGlobal;
using System.Drawing;
using System.Linq;
using System.Windows.Input;

namespace CarDrifterControl.Model
{
    internal class CarDrifterSimulator : GameSimulator
    {
        private Sounds sounds;                          //Soundwiedergabe

        private Car car;
        private int carLevelId;
        private int ballLevelId;

        private bool showHelpText = true;

        //Wird vom Leveleditor genutzt
        public CarDrifterSimulator(SimulatorInputData data, Size panelSize, Camera2D camera, float timerIntervalInMilliseconds)
            : base(data, panelSize, camera, timerIntervalInMilliseconds)
        {
        }

        //Wird vom GameViewModel genutzt
        public CarDrifterSimulator(string levelFile, float timerIntervalInMilliseconds, Sounds sounds, string dataFolder, IDrawingPanel panel)
            : base(levelFile, panel.Size, timerIntervalInMilliseconds)
        {
            Init(sounds, dataFolder);
        }

        public void Init(Sounds sounds, string dataFolder)
        {
            this.sounds = sounds;
            
            bool useEmptyMap = false;

            //leere Karte nutzen
            if (useEmptyMap)
            {
                var boxes = this.GetBodiesByTagName("box").ToList();
                foreach (var box in boxes)
                {
                    this.RemoveRigidBody(box);
                }
                this.BackgroundImage.FileName = dataFolder + "MapEmpty.png";
            }
            

            this.car = new Car(this, this.sounds);

            this.carLevelId = GetTagDataFromBody(GetBodiesByTagName("car").First()).LevelItemId;
            this.ballLevelId = GetTagDataFromBody(GetBodyByTagName("ball")).LevelItemId;
        }

        public override void Draw(IDrawingPanel panel)
        {
            base.Draw(panel);

            this.car.Draw(panel);   

            //Hiermit teste ich, dass Kollisionspuntke auch zwischen zwei Objekten ermittelt werden können, welche laut 
            //CollisionMatrix nicht kollidieren
            panel.DisableDepthTesting();
            var collisonPoints = GetCollisionPointsBetweenTwoLevelItems(this.carLevelId, this.ballLevelId);
            foreach (var point in collisonPoints)
            {
                panel.DrawFillCircle(Color.Red, point.Start, 5);
            }

            if (showHelpText)
            {
                panel.PushMatrix();
                panel.SetTransformationMatrixToIdentity();
                panel.DrawString(10, 20, Color.Black, 30, "Left/Right = steer left/right");
                panel.DrawString(10, 60, Color.Black, 30, "Up = drive forward");
                panel.DrawString(10, 100, Color.Black, 30, "Down = drive backward");
                panel.DrawString(10, 140, Color.Black, 30, "Strg = brake");
                panel.PopMatrix();
            }
            

            panel.FlipBuffer();
        }

        public override void HandleKeyDown(Key key)
        {
            base.HandleKeyDown(key);
            this.car.HandleKeyDown(key);

            this.showHelpText = false;
        }

        public override void HandleKeyUp(Key key)
        {
            base.HandleKeyUp(key);
            this.car.HandleKeyUp(key);
        }

        public override void MoveOneStep(float dt)
        {
            base.MoveOneStep(dt);
            this.car.MoveOneStep(dt);
        }
    }
}
