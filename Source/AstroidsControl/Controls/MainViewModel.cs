using AstroidsControl.Model;
using GameHelper;
using GameHelper.Simulation;
using GraphicPanels;
using GraphicPanelWpf;
using KeyboardRecordAndPlay;
using ReactiveUI;
using SoundEngine;
using System;
using System.Drawing;
using System.IO;

namespace AstroidsControl.Controls
{
    //Tasten: Pfeiltasten = Raumschiff steuern
    //        Space = Schießen
    //        Esq = Neustart
    //        R = Neustart mit rand(0)
    //        S = Save last Replay
    //        L = Load last Replay
    internal class MainViewModel : ReactiveObject, ITimerHandler, IGraphicPanelHandler, IPhysicSimulated
    {
        private string DataFolder;

        private GraphicPanel2D panel;                   //Grafik-Ausgabe

        private float timerIntervallInMilliseconds;     //Timer

        private KeyBoardPlayer keyBoardPlayer = null;   //Tastendrücke aufzeichen/wiedergeben
        private KeyboardRecorder keyboardRecorder;

        private GameSimulator simulator;                //Physik-Simulation
        private Random rand = new Random();
        private Rocket rocket;
        private AstroidCreator astroidCreator;
        private AstroidDestroyer collisionHandler;

        //In diesen Bereich wird gespielt. Um so größer, um so kleiner erscheint das Raumschiff
        private readonly RectangleF GameArea = new RectangleF(0, 0, 2000, 1000);

        private Sounds sounds;                          //Soundwiedergabe
        public MainViewModel(GraphicPanel2D panel, ISoundGenerator soundGenerator, float timerIntervallInMilliseconds, string dataFolder)
        {
            this.DataFolder = dataFolder;
            this.panel = panel;
            this.timerIntervallInMilliseconds = timerIntervallInMilliseconds;
            this.sounds = new Sounds(soundGenerator, this.DataFolder);

            Load("Astroids.txt");
        }

        private void Load(string levelFile)
        {
            this.simulator = new GameSimulator(DataFolder + levelFile, panel.Size, timerIntervallInMilliseconds);
            this.simulator.ShowSmallWindow = false;            
            this.rocket = new Rocket(this.DataFolder, this.simulator, this.sounds, this.panel);
            this.astroidCreator = new AstroidCreator(this.DataFolder, this.simulator, this.rand);
            this.collisionHandler = new AstroidDestroyer(this.simulator, this.sounds, this.rand);

            this.simulator.FixCameraArea = this.GameArea;
            this.simulator.CameraModus = Simulator.Simulator.CameraMode.FixArea;

            this.keyboardRecorder = new KeyboardRecorder();
            this.keyBoardPlayer = null;
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
                KeyDownAction = (key) => { this.simulator.HandleKeyDown(key); this.rocket.HandleKeyDown(key); },
                KeyUpAction = (key) => { this.simulator.HandleKeyUp(key); this.rocket.HandleKeyUp(key); },
                IsFinish = () => { }
            });

            return recordData.TimerTicks;
        }

        private void Refresh()
        {
            this.simulator.Draw(this.panel);
            this.rocket.Draw(this.panel);
            this.collisionHandler.Draw(this.panel);

            this.panel.PushMatrix();
            this.panel.SetTransformationMatrixToIdentity();
            panel.DrawString(10, 10, Color.White, 20, "Destroycounter:" + this.collisionHandler.DestroyCounter);
            this.panel.PopMatrix();

            panel.FlipBuffer();
        }


        #region ITimerHandler
        public void HandleTimerTick(float dt)
        {
            if (this.keyBoardPlayer != null) //Modus = Replay
            {
                this.keyBoardPlayer.HandleTimerTick(dt);
            }

            this.keyboardRecorder.HandleTimerTick(dt);

            this.simulator.MoveOneStep(dt);
            this.rocket.HandleTimerTick(dt);
            this.astroidCreator.HandleTimerTick(dt);
            this.collisionHandler.HandleTimerTick(dt);

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
            //Start das Spiel mit Random-Seedwert von 0 -> Wird benötigt, damit bei Replay-Wiedergabe aus den UnitTests herraus die Platform immer gleich ist
            if (e.Key == System.Windows.Input.Key.R)
            {
                this.rand = new Random(0);
                Load("Astroids.txt");
            }

            if (this.keyBoardPlayer == null) //Modus = Play Game
            {
                this.simulator.HandleKeyDown(e.Key);
                this.rocket.HandleKeyDown(e.Key);
                this.keyboardRecorder.AddKeyDownEvent(e.Key);
            }



        }
        public void HandleKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            if (this.keyBoardPlayer == null) //Modus = Play Game
            {
                this.simulator.HandleKeyUp(e.Key);
                this.rocket.HandleKeyUp(e.Key);
                this.keyboardRecorder.AddKeyUpEvent(e.Key);
            }


            if (e.Key == System.Windows.Input.Key.Escape)
            {
                Load("Astroids.txt");
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
                StartReplay("Astroids.txt", "LastReplay.txt");
            }
        }

        #endregion

        #region IPhysicSimulated
        public int LoadSimulation(string levelFile, string keyboardFile)
        {
            this.rand = new Random(0); //Sorge dafür, dass die gleichen Steine an der gleichen Stelle erzeugt werden
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
