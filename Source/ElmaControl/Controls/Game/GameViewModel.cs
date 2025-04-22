using ElmaControl.Controls.Game.Model;
using GameHelper;
using GraphicPanels;
using GraphicPanelWpf;
using KeyboardRecordAndPlay;
using LevelEditorGlobal;
using Simulator;
using SoundEngine;
using System;
using System.Drawing;
using System.IO;
using WpfControls.Controls.CameraSetting;

namespace ElmaControl.Controls.Game
{
    //Erweitert die Simulation um KeyBoardRecord/Play und die Simulation-Finish-Events um LevelFile-Parameter
    internal class GameViewModel : ITimerHandler, IGraphicPanelHandler, IPhysicSimulated
    {
        private string DataFolder = null;

        private GraphicPanel2D panel;                   //Grafik-Ausgabe

        private float timerIntervallInMilliseconds;     //Timer

        private ElmaSimulator simulator;                //Physik-Simulation
        private KeyBoardPlayer keyBoardPlayer = null;
        private KeyboardRecorder keyboardRecorder;

        private Sounds sounds;                          //Soundwiedergabe

        //Model
        private string lastLoadedLevel = string.Empty;
        private string keyboardFile = string.Empty;

        enum MainState { PlayGame, Replay }
        private MainState state = MainState.PlayGame;

        //Diese 3 Events werden gefeuert, wenn die Simulation zu Ende ist
        public event Action<string, string, bool> BikeIsBrokenHandler;//Kopf ist gegen die Wand gekommen (Parameter: levelFile, keyboardFile, isReplay)
        public event Action<string, string, bool> FinishedIsReached;  //Motorrad berührt die Blume (Parameter: levelFile, keyboardFile, isReplay)
        public event Action<string, string> GoBackHandler;      //Esc wurde Esc gedrückt (Parameter: levelFile, keyboardFile)


        public GameViewModel(GraphicPanel2D panel, ISoundGenerator soundGenerator, float timerIntervallInMilliseconds, string dataFolder)
        {
            this.DataFolder = dataFolder;
            this.panel = panel;
            this.timerIntervallInMilliseconds = timerIntervallInMilliseconds;

            this.sounds = new Sounds(soundGenerator, this.DataFolder);         
        }

        public ILeveleditorUsedSimulator CreateSimulator(SimulatorInputData data, Size panelSize, Camera2D camera, float timerIntercallInMilliseconds)
        {
            var sim = new ElmaSimulator(data, panelSize, camera, timerIntercallInMilliseconds);
            sim.Init(this.sounds, this.DataFolder, this.panel);
            return sim;
        }

        public void StartPlay(string levelFile)
        {
            this.lastLoadedLevel = levelFile;
            this.keyboardFile = DataFolder + "Recordings\\LastReplay.txt";

            this.simulator = new ElmaSimulator(levelFile, timerIntervallInMilliseconds, this.sounds, this.DataFolder, this.panel);
            this.simulator.StopElapsedTimer += () => SaveKeyboardData();
            this.simulator.BikeIsBrokenHandler += () => BikeIsBrokenHandler.Invoke(this.lastLoadedLevel, this.keyboardFile, this.state == MainState.Replay);
            this.simulator.FinishedIsReached += () => this.FinishedIsReached?.Invoke(this.lastLoadedLevel, this.keyboardFile, this.state == MainState.Replay);
            this.keyboardRecorder = new KeyboardRecorder();

            this.state = MainState.PlayGame;
        }

        public int StartReplay(string levelFile, string keyboardFile)
        {
            StartPlay(levelFile);
            this.keyboardFile = keyboardFile;

            var recordData = JsonHelper.Helper.CreateFromJson<KeyBoardRecordData>(File.ReadAllText(keyboardFile));

            this.keyBoardPlayer = new KeyBoardPlayer(new KeyBoardPlayerConstructorData()
            {
                RecordData = recordData,
                KeyDownAction = (key) => this.HandleKeyDown(key),
                KeyUpAction = (key) => this.HandleKeyUp(key),
                IsFinish = () => { }
            });

            this.state = MainState.Replay;

            return recordData.TimerTicks;
        }

        //Wird aufgerufen, wenn das Motorrad die Wand oder die Blume berührt
        private void SaveKeyboardData()
        {
            if (this.state == MainState.PlayGame)
            {
                var data = JsonHelper.Helper.ToJson(this.keyboardRecorder.GetRecordedData());
                File.WriteAllText(this.keyboardFile, data);
            }            
        }

        #region ITimerHandler
        public void HandleTimerTick(float dt)
        {
            if (this.simulator.HeadHasTouchedWall == false)
            {
                if (this.state == MainState.Replay)
                {
                    this.keyBoardPlayer.HandleTimerTick(dt);
                }
                else
                {
                    this.keyboardRecorder.HandleTimerTick(dt);
                }
            }

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
            if (this.state == MainState.PlayGame)
            {
                HandleKeyDown(e.Key);
            }            
        }        
        public void HandleKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            if (this.state == MainState.PlayGame)
            {
                HandleKeyUp(e.Key);
            }else
            {
                if (e.Key == System.Windows.Input.Key.Escape)
                {
                    this.GoBackHandler?.Invoke(this.lastLoadedLevel, this.keyboardFile);
                }
            }
        }

        private void HandleKeyDown(System.Windows.Input.Key key)
        {
            this.keyboardRecorder.AddKeyDownEvent(key);

            this.simulator.HandleKeyDown(key);
        }
        private void HandleKeyUp(System.Windows.Input.Key key)
        {
            this.keyboardRecorder.AddKeyUpEvent(key);

            this.simulator.HandleKeyUp(key);

            if (key == System.Windows.Input.Key.Escape)
            {
                SaveKeyboardData();
                this.GoBackHandler?.Invoke(this.lastLoadedLevel, this.keyboardFile);
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
            HandleTimerTick(dt);
            return this.keyBoardPlayer.IsFinish;
        }
        #endregion
    }
}
