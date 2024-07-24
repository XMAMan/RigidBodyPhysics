using CarDrifterControl.Model;
using GameHelper;
using GraphicPanels;
using GraphicPanelWpf;
using KeyboardRecordAndPlay;
using LevelEditorGlobal;
using Simulator;
using SoundEngine;
using System.Drawing;
using System.IO;
using WpfControls.Controls.CameraSetting;

namespace CarDrifterControl.Controls
{
    internal class MainViewModel : ITimerHandler, IGraphicPanelHandler, IPhysicSimulated
    {
        private string DataFolder;

        private GraphicPanel2D panel;                   //Grafik-Ausgabe

        private float timerIntervallInMilliseconds;     //Timer

        private CarDrifterSimulator simulator;          //Physik-Simulation

        private KeyBoardPlayer keyBoardPlayer = null;   //Tastendrücke aufzeichen/wiedergeben
        private KeyboardRecorder keyboardRecorder;

        private Sounds sounds;                          //Soundwiedergabe

        public MainViewModel(GraphicPanel2D panel, ISoundGenerator soundGenerator, float timerIntervallInMilliseconds, string dataFolder)
        {
            this.DataFolder = dataFolder;
            this.panel = panel;
            this.timerIntervallInMilliseconds = timerIntervallInMilliseconds;
            this.sounds = new Sounds(soundGenerator, this.DataFolder);

            Load("CarDrifter.txt");
        }

        public ILeveleditorUsedSimulator CreateSimulator(SimulatorInputData data, Size panelSize, Camera2D camera, float timerIntercallInMilliseconds)
        {
            var sim = new CarDrifterSimulator(data, panelSize, camera, timerIntercallInMilliseconds);
            sim.Init(this.sounds, this.DataFolder);
            return sim;
        }

        private void Load(string levelFile)
        {
            this.simulator = new CarDrifterSimulator(DataFolder + levelFile, timerIntervallInMilliseconds, sounds, DataFolder, panel);
            this.simulator.ShowSmallWindow = false;

            this.keyboardRecorder = new KeyboardRecorder();
            this.keyBoardPlayer = null;
        }

        //Return: Anzahl der Timerticks von keyboardFile 
        private int StartReplay(string levelFile, string keyboardFile)
        {
            Load(levelFile);

            var recordData = JsonHelper.Helper.CreateFromJson<KeyBoardRecordData>(File.ReadAllText(DataFolder + keyboardFile));

            this.keyBoardPlayer = new KeyBoardPlayer(new KeyBoardPlayerConstructorData()
            {
                RecordData = recordData,
                KeyDownAction = (key) => { this.simulator.HandleKeyDown(key); },
                KeyUpAction = (key) => { this.simulator.HandleKeyUp(key); },
                IsFinish = () => { }
            });

            return recordData.TimerTicks;
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

            this.simulator.Draw(this.panel);

        }
        #endregion

        #region IGraphicPanelHandler
        public void HandleSizeChanged(int width, int height)
        {
            this.simulator.PanelSizeChangedHandler(width, height);
            this.simulator.Draw(this.panel);
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
            if (this.keyBoardPlayer == null) //Modus = Play Game
            {
                this.simulator.HandleKeyDown(e.Key);                
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
                Load("CarDrifter.txt");
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
                StartReplay("CarDrifter.txt", "LastReplay.txt");
            }
        }

        #endregion

        #region IPhysicSimulated
        public int LoadSimulation(string levelFile, string keyboardFile)
        {
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
