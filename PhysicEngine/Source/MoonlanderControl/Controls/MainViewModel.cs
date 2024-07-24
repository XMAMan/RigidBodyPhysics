using GraphicPanels;
using GraphicPanelWpf;
using MoonlanderControl.Model;
using ReactiveUI;
using System;
using System.Drawing;
using KeyboardRecordAndPlay;
using System.IO;
using GameHelper;
using SoundEngine;
using GameHelper.Simulation;

namespace MoonlanderControl.Controls
{
    //Tasten: Up = Fliege hoch
    //        Left/Richt = Drehe Raumschiff
    //        Space = Explodiere Raumschiff
    //        I während das Intro gezeigt wird = Spiele Intro-Musik erneut ab
    //        R während das Intro gezeigt wird = Start das Spiel mit ein Random-Seedwert von 0
    //        S = Speichere Keyboard-Datei nach DataFolder\LastReplay.txt
    //        L = Lade DataFolder\LastReplay.txt

    internal class MainViewModel : ReactiveObject, ITimerHandler, IGraphicPanelHandler, IPhysicSimulated
    {
        private string DataFolder;

        private bool showIntroScreen = true;

        private GraphicPanel2D panel;                   //Grafik-Ausgabe

        private float timerIntervallInMilliseconds;     //Timer

        private KeyBoardPlayer keyBoardPlayer = null;   //Tastendrücke aufzeichen/wiedergeben
        private KeyboardRecorder keyboardRecorder;

        private GameSimulator simulator;          //Physik-Simulation
        private MainThruster mainThruster;
        private Ground ground;
        private Ship ship;
        private ShipExploder shipExploder;
        private FuelGauge fuelGauge;
        private float shipDistanceToGround = float.PositiveInfinity;
        private Random rand = new Random();

        private Sounds sounds;                          //Soundwiedergabe

        public MainViewModel(GraphicPanel2D panel, ISoundGenerator soundGenerator, float timerIntervallInMilliseconds, string dataFolder) 
        {
            this.DataFolder = dataFolder;
            this.panel = panel;
            this.timerIntervallInMilliseconds = timerIntervallInMilliseconds;

            this.sounds = new Sounds(soundGenerator, this.DataFolder);

            this.sounds.PlayIntroSound();
        }

        private void Load(string levelFile)
        {
            this.simulator = new GameSimulator(DataFolder + levelFile, panel.Size, timerIntervallInMilliseconds);
            this.fuelGauge = new FuelGauge(this.simulator);
            this.mainThruster = new MainThruster(this.simulator, this.sounds);
            this.ground = new Ground(this.simulator, this.rand);
            this.ship = new Ship(this.simulator);
            this.ship.IsBrokenChanged += Ship_IsBrokenChanged;
            this.shipExploder = new ShipExploder(this.simulator, this.sounds, this.rand);

            this.keyboardRecorder = new KeyboardRecorder();
            this.keyBoardPlayer = null;

            this.simulator.CameraModus = Simulator.Simulator.CameraMode.SceneBoundingBox;
        }

        //Return: Anzahl der Timerticks von keyboardFile 
        private int StartReplay(string levelFile, string keyboardFile)
        {
            this.rand = new Random(0);
            Load(levelFile);

            var recordData = JsonHelper.Helper.CreateFromJson<KeyBoardRecordData>(File.ReadAllText(DataFolder + keyboardFile));

            this.keyBoardPlayer = new KeyBoardPlayer(new KeyBoardPlayerConstructorData()
            {
                RecordData = recordData,
                KeyDownAction = (key) => this.simulator.HandleKeyDown(key),
                KeyUpAction = (key) => this.simulator.HandleKeyUp(key),
                IsFinish = () => { }
            });

            return recordData.TimerTicks;
        }

        private void Ship_IsBrokenChanged(bool isBroken)
        {
            if (isBroken)
            {
                this.shipExploder.StartTimer(3);
            }
        }

        private void Refresh()
        {
            if (this.showIntroScreen)
            {
                this.panel.ClearScreen(Color.White);
                this.panel.DrawFillRectangle(DataFolder + "Moonlander_Intro.png", 0, 0, this.panel.Width, this.panel.Height, false, Color.White);
                this.panel.FlipBuffer();
                return;
            }

            this.simulator.Draw(this.panel);
            this.mainThruster.Draw(this.panel);
            this.shipExploder.DrawParticles(this.panel);
            this.ground.Draw(this.panel);

            this.panel.PushMatrix();
            this.panel.SetTransformationMatrixToIdentity();

            this.fuelGauge.Draw(this.panel);

            if (this.ship.IsShipInMinMaxRangeFromLandingArea(this.ground.LandingArea))
            {
                this.panel.DrawString(10,20, Color.White, 20, "Altitude:" + (int)this.shipDistanceToGround);
            }
            

            if (this.ship.IsBroken)
            {
                DrawText("Ship was destroyed!", Color.Red);
            }

            if (this.ship.IsStandingOnLandingArea)
            {
                DrawText("Landing successful", Color.Green);
            }

            this.panel.PopMatrix();
            panel.FlipBuffer();
        }

        private void DrawText(string text, Color color)
        {
            float textSize = 50;
            var size = this.panel.GetStringSize(textSize, text);
            this.panel.DrawString(this.panel.Width / 2 - size.Width / 2, this.panel.Height / 2 - size.Height / 2, color, textSize, text);
        }

        #region ITimerHandler
        public void HandleTimerTick(float dt)
        {
            if (this.showIntroScreen == false)
            {
                if (this.keyBoardPlayer != null) //Modus = Replay
                {
                    this.keyBoardPlayer.HandleTimerTick(dt);
                }

                this.keyboardRecorder.HandleTimerTick(dt);

                this.simulator.MoveOneStep(dt);

                this.ground.MoveOnStep(dt);

                this.mainThruster.MoveOnStep(dt);
                this.shipDistanceToGround = this.ground.GetDistanceFromShipToGround(this.ship);

                float shipDistanceToLandingArea = this.ship.GetDistanceToLandingArea(this.ground.LandingArea);
                this.simulator.CameraModus = shipDistanceToLandingArea < 400 ? Simulator.Simulator.CameraMode.CameraTracker : Simulator.Simulator.CameraMode.SceneBoundingBox;

                this.ship.TimerTickHandler(this.ground);
                this.shipExploder.TimerTickHandler(dt);
                this.fuelGauge.TimerTickHandler(dt);
            }


           

            Refresh();

        }
        #endregion

        #region IGraphicPanelHandler
        public void HandleSizeChanged(int width, int height)
        {
            this.simulator?.PanelSizeChangedHandler(width, height);
            Refresh();
        }
        public void HandleMouseClick(System.Windows.Forms.MouseEventArgs e)
        {

        }
        public void HandleMouseWheel(System.Windows.Forms.MouseEventArgs e)
        {

        }
        public void HandleMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            //var point = this.camera.PointToCamera(new PointF(e.X, e.Y)).ToGrx();
        }
        public void HandleMouseDown(System.Windows.Forms.MouseEventArgs e)
        {

        }
        public void HandleMouseUp(System.Windows.Forms.MouseEventArgs e)
        {

        }
        public void HandleMouseEnter()
        {

        }
        public void HandleMouseLeave()
        {

        }
        public void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (this.showIntroScreen)
            {
                if (e.Key == System.Windows.Input.Key.I)
                {
                    this.sounds.PlayIntroSound();
                }else
                {
                    this.showIntroScreen = false;
                    this.sounds.StopIntroSound();

                    //Start das Spiel mit Random-Seedwert von 0 -> Wird benötigt, damit bei Replay-Wiedergabe aus den UnitTests herraus die Platform immer gleich ist
                    if (e.Key == System.Windows.Input.Key.R)
                    {
                        this.rand = new Random(0);
                    }

                    Load("Moonlander.txt");
                }                
            }

            if (this.keyBoardPlayer == null) //Modus = Play Game
            {
                if (this.shipExploder.IsExploded == false && this.ship.IsStandingOnLandingArea == false && this.fuelGauge.IsFuelAvailable())
                {
                    this.simulator.HandleKeyDown(e.Key);
                }
                this.keyboardRecorder.AddKeyDownEvent(e.Key);
            }
               
            
            
        }
        public void HandleKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            if (this.keyBoardPlayer == null) //Modus = Play Game
            {
                this.simulator.HandleKeyUp(e.Key);
                this.keyboardRecorder.AddKeyUpEvent(e.Key);
            }
                

            if (e.Key == System.Windows.Input.Key.Escape)
            {
                Load("Moonlander.txt");
            }

            if (e.Key == System.Windows.Input.Key.Space)
            {
                this.shipExploder.ExplodeShip();
            }

            //Save last replay
            if (e.Key == System.Windows.Input.Key.S)
            {
                var data = JsonHelper.Helper.ToJson(this.keyboardRecorder.GetRecordedData());
                File.WriteAllText(DataFolder + "LastReplay.txt", data);
            }

            //Load last Replay
            if (e.Key == System.Windows.Input.Key.L)
            {
                StartReplay("Moonlander.txt", "LastReplay.txt");
            }
        }

        #endregion

        #region IPhysicSimulated
        public int LoadSimulation(string levelFile, string keyboardFile)
        {
            this.showIntroScreen = false;
            this.sounds.StopIntroSound();
            this.rand = new Random(0); //Sorge dafür, dass die Landeplattform immer an der gleichen Stelle erscheint (Intro muss mit R übersprugen werden, wenn man ein KeyboardFile erzeugen will)
            return StartReplay(levelFile, keyboardFile);
        }

        //Return: KeyboardPlayer.IsFinish
        public bool DoTimeStep(float dt)
        {
            this.HandleTimerTick(dt);
            return this.keyBoardPlayer.IsFinish;
        }
        #endregion
    }
}
