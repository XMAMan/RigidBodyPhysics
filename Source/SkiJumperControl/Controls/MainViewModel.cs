using GameHelper;
using GameHelper.Simulation;
using GraphicPanels;
using GraphicPanelWpf;
using KeyboardRecordAndPlay;
using ReactiveUI;
using SkiJumperControl.Model;
using SoundEngine;
using System.Drawing;
using System.IO;

namespace SkiJumperControl.Controls
{
    internal class MainViewModel : ReactiveObject, ITimerHandler, IGraphicPanelHandler, IPhysicSimulated
    {
        private string DataFolder = null;

        private GraphicPanel2D panel;                   //Grafik-Ausgabe

        private float timerIntervallInMilliseconds;     //Timer

        private GameSimulator simulator;                //Physik-Simulation
        private KeyBoardPlayer keyBoardPlayer = null;
        private KeyboardRecorder keyboardRecorder;

        private Sounds sounds;                          //Soundwiedergabe

        //Model
        private Skiman skiman;

        enum MainState { ShowIntro, PlayGame, Replay}
        private MainState state = MainState.ShowIntro;

        public MainViewModel(GraphicPanel2D panel, ISoundGenerator soundGenerator, float timerIntervallInMilliseconds, string dataFolder)
        {
            this.DataFolder = dataFolder;
            this.panel = panel;
            this.timerIntervallInMilliseconds = timerIntervallInMilliseconds;

            this.sounds = new Sounds(soundGenerator, this.DataFolder);
            Load("Skijumper.txt");

            this.sounds.PlayIntro();
        }

        private void Load(string file)
        {
            this.simulator = new GameSimulator(DataFolder + file, panel.Size, timerIntervallInMilliseconds);
            this.simulator.ShowSmallWindow = false;
            this.skiman = new Skiman(simulator, sounds);
            this.keyboardRecorder = new KeyboardRecorder();
        }

        private void Refresh()
        {
            if (this.state == MainState.ShowIntro)
            {
                this.panel.ClearScreen(Color.White);
                this.panel.DrawFillRectangle(DataFolder + "SkiJumper_Intro.png", 0, 0, this.panel.Width, this.panel.Height, false, Color.White);
                this.panel.FlipBuffer();
                return;
            }

            this.simulator.Draw(this.panel);

            this.panel.PushMatrix();
            this.panel.SetTransformationMatrixToIdentity();

            //this.panel.DrawString(10, 20, Color.White, 20, "Angle:" + this.Text);

            if (this.skiman.FlagIsReachedWithoutInjury)
            {
                DrawText("Flag has been reached", Color.Green, 0.3f);
                DrawText("Number of somersaults=" + this.skiman.GetSomersaults(), Color.Green, 0.5f);
                DrawText("Points=" + GetPoints(this.skiman.GetSomersaults()), Color.Green, 0.7f);
            }
            else if (this.skiman.SkimanWasInjured)
            {
                DrawText("Skieman was injured", Color.Red);
            }
            else if (this.skiman.FlagIsTouched && this.skiman.ManIsTouchingTheGround)
            {
                DrawText("Skiman is touching the ground", Color.Red);
            }
            else if (this.skiman.SkiBoardIsBroken)
            {
                DrawText("Ski board was broken", Color.Red);
            }

            this.panel.PopMatrix();
            panel.FlipBuffer();
        }

        private int GetPoints(Skiman.Somersault somersault)
        {
            switch (somersault)
            {
                case Skiman.Somersault.Nothing: return 1;
                case Skiman.Somersault.Backflip: return 5;
                case Skiman.Somersault.DoubleBackflip: return 10;
                case Skiman.Somersault.TrippleBackflip: return 100;
                case Skiman.Somersault.Frontflip: return 15;
                case Skiman.Somersault.DoubleFrontflip: return 50;
                case Skiman.Somersault.TrippleFrontflip: return 200;
            }

            return -1;
        }

        private void DrawText(string text, Color color, float height = 0.5f)
        {
            float textSize = 50;
            var size = this.panel.GetStringSize(textSize, text);
            this.panel.DrawString(this.panel.Width / 2 - size.Width / 2, this.panel.Height * height - size.Height / 2, color, textSize, text);
        }

        #region ITimerHandler
        public void HandleTimerTick(float dt)
        {
            if (this.state == MainState.Replay)
            {
                this.keyBoardPlayer.HandleTimerTick(dt);
            }

            if (this.state != MainState.ShowIntro && this.skiman.FlagIsReachedWithoutInjury == false)
            {
                this.keyboardRecorder.HandleTimerTick(dt);
                this.simulator.MoveOneStep(dt);
            }

            

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
            this.keyboardRecorder.AddKeyDownEvent(e.Key);

            if (this.state == MainState.PlayGame)
            {                
                this.simulator.HandleKeyDown(e.Key);
            }            
        }
        public void HandleKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            if (this.state == MainState.ShowIntro)
            {
                if (e.Key == System.Windows.Input.Key.I)
                {
                    this.sounds.PlayIntro();
                    return;
                }
                else
                {
                    this.state = MainState.PlayGame;
                    this.sounds.StopIntro();
                }
                
               
            }


            this.keyboardRecorder.AddKeyUpEvent(e.Key);
            this.simulator.HandleKeyUp(e.Key);

            //Testfälle:
            //1 = Flagge wurde ohne Salte erreicht                                  -> Zeige Gewinntext mit Salto=No
            //2 = Flagge wurde ohne Verletzung erreicht                             -> Es soll der Gewinntext angezeigt werden mit Salto=Backflip
            //3 = Flagge wurde mit Double-Backflip erreicht                         -> Zeige Gewinntext mit Salto=DoubleBackflip
            //4 = Flagge wurde mit Frontflip erreicht                               -> Zeige Gewinntext mit Salto=Frontflip
            //5 = Flagge wurde mit Double-Frontflip erreicht                        -> Zeige Gewinntext mit Salto=Double-Frontflip
            //6 = Flagge wurde erreicht aber Hintern berührt den Boden              -> Es soll IsTouching angezeigt werden
            //7 = Skifahrer fliegt hin und Kopf fliegt gegen Flagge                 -> Es soll "was injured" angezeigt werden
            //8 = Es bricht vom Ski ein Holzstück ab und fliegt gegen die Flagge    -> Es darf kein Gewinntext angezeigt werden
            //9 = Flagge wurde erreicht aber der Ski ist kaputt                     -> Zeige "Board is broken"-Text

            if (e.Key == System.Windows.Input.Key.D1) StartReplay("FlagIsReachedWithoutSomersault.txt");
            if (e.Key == System.Windows.Input.Key.D2) StartReplay("Backflip.txt");
            if (e.Key == System.Windows.Input.Key.D3) StartReplay("Doublebackflip.txt");
            if (e.Key == System.Windows.Input.Key.D4) StartReplay("Frontflip.txt");
            if (e.Key == System.Windows.Input.Key.D5) StartReplay("Doublefrontflip.txt");
            if (e.Key == System.Windows.Input.Key.D6) StartReplay("IsTouchingGround.txt");
            if (e.Key == System.Windows.Input.Key.D7) StartReplay("DoubleFrontWithCrash.txt");
            if (e.Key == System.Windows.Input.Key.D8) StartReplay("SkiBoardTouchTheFlag.txt");
            if (e.Key == System.Windows.Input.Key.D9) StartReplay("FlagIsReachedWithBoardDamage.txt");
            if (e.Key == System.Windows.Input.Key.D0) StartReplay("SkiDoublefrontflip_Crash.txt");

            //Save last replay
            if (e.Key == System.Windows.Input.Key.S)
            {
                var data = JsonHelper.Helper.ToJson(this.keyboardRecorder.GetRecordedData());
                File.WriteAllText(DataFolder + "SkiJumperRecordings\\LastReplay.txt", data);
            }

            //Load last Replay
            if (e.Key == System.Windows.Input.Key.L)
            {
                StartReplay("LastReplay.txt");
            }

            if (e.Key == System.Windows.Input.Key.Escape)
            {
                Load("Skijumper.txt");
                this.state = MainState.PlayGame;
            }
        }

        //Return: Anzahl der Timerticks von keyboardFile
        private int StartReplay(string file)
        {
            Load("Skijumper.txt");

            var recordData = JsonHelper.Helper.CreateFromJson<KeyBoardRecordData>(File.ReadAllText(DataFolder + "SkiJumperRecordings\\" + file));

            this.keyBoardPlayer = new KeyBoardPlayer(new KeyBoardPlayerConstructorData()
            {
                RecordData = recordData,
                KeyDownAction = (key) => this.simulator.HandleKeyDown(key),
                KeyUpAction = (key) => this.simulator.HandleKeyUp(key),
                IsFinish = () => { }
            });

            this.state = MainState.Replay;

            return recordData.TimerTicks;
        }
        #endregion

        #region IPhysicSimulated
        public int LoadSimulation(string levelFile, string keyboardFile)
        {
            this.state = MainState.PlayGame;
            this.sounds.StopIntro();
            return StartReplay(keyboardFile);
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
