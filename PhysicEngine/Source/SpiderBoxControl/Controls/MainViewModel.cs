using GameHelper.Simulation;
using GraphicMinimal;
using GraphicPanels;
using GraphicPanelWpf;
using ReactiveUI;
using SoundEngine;
using SpiderBoxControl.Model;
using System;
using System.Drawing;

namespace SpiderBoxControl.Controls
{
    internal class MainViewModel : ReactiveObject, ITimerHandler, IGraphicPanelHandler
    {
        private string DataFolder = null;

        private GraphicPanel2D panel;                   //Grafik-Ausgabe

        private float timerIntervallInMilliseconds;     //Timer

        private GameSimulator simulator;                //Physik-Simulation

        private Sounds sounds;                          //Soundwiedergabe

        private Player player;                          //Model
        private PongBlock pongBlock;
        private bool showPhysicModel = false;
        private bool showHelpText = false;

        public MainViewModel(GraphicPanel2D panel, ISoundGenerator soundGenerator, float timerIntervallInMilliseconds, string dataFolder)
        {
            this.DataFolder = dataFolder;
            this.panel = panel;
            this.timerIntervallInMilliseconds = timerIntervallInMilliseconds;

            this.sounds = new Sounds(soundGenerator, this.DataFolder);
            Load("Level1.txt");
        }

        private void Load(string file)
        {
            this.simulator = new GameSimulator(DataFolder + file, panel.Size, timerIntervallInMilliseconds);
            this.simulator.ShowSmallWindow = false;
            this.player = new Player(simulator, timerIntervallInMilliseconds, DataFolder, this.sounds);
            this.pongBlock = new PongBlock(simulator, DataFolder, timerIntervallInMilliseconds);
            this.simulator.CameraModus = Simulator.Simulator.CameraMode.CameraTracker;
            this.simulator.ShowSmallWindow = true;
            this.showPhysicModel = false;
        }

        private void Refresh()
        {
            this.simulator.Draw(this.panel);
            if (this.showPhysicModel)
            {
                this.simulator.DrawPhysicItemBorders(panel, Pens.Yellow);
                this.player.DrawPhysicBorderFromArm(panel, Pens.Yellow);
            }            

            this.panel.PushMatrix();
            this.panel.SetTransformationMatrixToIdentity();
            DrawProgressBar(panel, new Vector2D(10, 30), "Rope length", this.player.RopeLength, Rope.MinSegmentLength, Rope.MaxSegmentLength);
            DrawProgressBar(panel, new Vector2D(10, 90), "Throwing distance", this.player.Power, 0, 1);

            if (this.showHelpText)
            {
                panel.DrawString(350, 20, Color.Black, 30, "MouseMove = Set your target direction");
                panel.DrawString(350, 60, Color.Black, 30, "MouseClick = Throw out rope");
                panel.DrawString(350, 100, Color.Black, 30, "Left Down = Shorten rope");
                panel.DrawString(350, 140, Color.Black, 30, "Right Down = Extend rope");
                panel.DrawString(350, 180, Color.Black, 30, "MouseWheel = Change throwing power");
                panel.DrawString(350, 220, Color.Black, 30, "Space = Remove rope");
                panel.DrawString(350, 260, Color.Black, 30, "D = Show physic model");
            }else
            {
                panel.DrawString(10, 0, Color.Black, 30, "F1 = Show Help");
            }


            this.panel.PopMatrix();

            panel.FlipBuffer();
        }

        private static void DrawProgressBar(GraphicPanel2D panel, Vector2D position, string text, float value, float minValue, float maxValue)
        {
            int borderWidth = 3;
            Size size = new Size(300, 40);
            float fontSize = 20;

            float f = Math.Min(1, Math.Max(0, (value - minValue) / (maxValue - minValue)));
            panel.DrawFillRectangle(Color.Black, position.X, position.Y, size.Width, size.Height);
            panel.DrawFillRectangle(Color.Aquamarine, position.X + borderWidth, position.Y + borderWidth, size.Width - borderWidth * 2, size.Height - borderWidth * 2);
            panel.DrawFillRectangle(Color.Green, position.X + borderWidth, position.Y + borderWidth, (size.Width - borderWidth * 2) * f, size.Height - borderWidth * 2);

            var textSize = panel.GetStringSize(fontSize, text);
            panel.DrawString(position.X + borderWidth, position.Y + size.Height / 2 - textSize.Height / 2, Color.Black, fontSize, text);
        }

        #region ITimerHandler
        public void HandleTimerTick(float dt)
        {
            this.simulator.MoveOneStep(dt);
            this.player.HandleTimerTick(dt);
            this.pongBlock.MoveOneStep(dt);

            Refresh();
        }
        #endregion

        #region IGraphicPanelHandler
        public void HandleSizeChanged(int width, int height)
        {
            this.simulator.PanelSizeChangedHandler(width, height);
            Refresh();
        }
        public void HandleMouseClick(System.Windows.Forms.MouseEventArgs e)
        {
            this.player.HandleMouseClick(this.simulator.PointToCamera(new PointF(e.X, e.Y)), e.Button);
        }
        public void HandleMouseWheel(System.Windows.Forms.MouseEventArgs e)
        {
            this.player.HandleMouseWheel(e);
        }
        public void HandleMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            this.player.HandleMouseMove(this.simulator.PointToCamera(new PointF(e.X, e.Y)), e.Button);
        }
        public void HandleMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            this.player.HandleMouseDown(e);
        }
        public void HandleMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            this.player.HandleMouseUp(e);
        }
        public void HandleMouseEnter()
        {

        }
        public void HandleMouseLeave()
        {

        }

        public void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            this.player.HandleKeyDown(e.Key);

            if (e.Key == System.Windows.Input.Key.Escape)
            {
                Load("Level1.txt");
            }

            if (e.Key== System.Windows.Input.Key.D)
            {
                this.showPhysicModel = !this.showPhysicModel;
            }

            if (e.Key == System.Windows.Input.Key.F1)
            {
                this.showHelpText = !this.showHelpText;
            }
        }

        public void HandleKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            this.player.HandleKeyUp(e.Key);
        }
        #endregion
    }
}
