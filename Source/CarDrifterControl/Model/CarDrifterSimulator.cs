using GameHelper.Simulation;
using GraphicPanels;
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

        private bool showHelpText = true;

        //Wird vom Leveleditor genutzt
        public CarDrifterSimulator(SimulatorInputData data, Size panelSize, Camera2D camera, float timerIntervalInMilliseconds)
            : base(data, panelSize, camera, timerIntervalInMilliseconds)
        {
        }

        //Wird vom GameViewModel genutzt
        public CarDrifterSimulator(string levelFile, float timerIntervalInMilliseconds, Sounds sounds, string dataFolder, GraphicPanel2D panel)
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
        }

        public override void Draw(GraphicPanel2D panel)
        {
            base.Draw(panel);

            this.car.Draw(panel);   

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
