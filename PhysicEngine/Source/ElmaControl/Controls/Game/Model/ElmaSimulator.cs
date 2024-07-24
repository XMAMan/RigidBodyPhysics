using GameHelper;
using GameHelper.Simulation;
using GraphicPanels;
using KeyboardRecordAndPlay;
using LevelEditorGlobal;
using System;
using System.Drawing;
using System.Windows.Input;
using WpfControls.Controls.CameraSetting;

namespace ElmaControl.Controls.Game.Model
{
    //Simuliert die Bewegung vom Motorrad und feuert ein Event, wenn die Simulation zu Ende ist
    internal class ElmaSimulator : GameSimulator
    {
        private string DataFolder = null;

        private GraphicPanel2D panel;   //Grafik-Ausgabe

        private Sounds sounds;          //Soundwiedergabe

        //Model
        private Bike bike;
        private Apples apples;
        private Flower flower;
        private bool isFinished = false; //Wird true sobalt der Kopf die Wand berührt oder das Motorrad die Blume
        private VoronoiExploder exploder = null;
        private TickCounterStopwatch tickCounter = null; //Wird gestartet, wenn der Kopf gegen die Wand kommt
        private int timerTickCounter = 1; //Zählt die Anzahl der TimerTicks
        private string elapsedTimeText = "0";
        private bool simulationIsFinished = false; //Wird true wenn die Bike-Explodeanimation oder Flower-Reached-Musik zu Ende ist

        public event Action StopElapsedTimer; //Wird sofort gefeuert, wenn das Motorad die Blume oder der Kopf den Boden berührt -> Ab hier werden keine Tastendrücke mehr aufgezeichnet und der Timer links oben stoppt
        public event Action BikeIsBrokenHandler; //Dieses Event wird 3 Sekunden nach Zusamenstoß vom Kopf mit Wand gefeuert
        public event Action FinishedIsReached; //Dieses Event wird 3 Sekunden nach Zusammenstoß von Bike mit Blume gefeuert

        public bool HeadHasTouchedWall
        {
            get => this.tickCounter.IsRunning;
        }

        //Wird vom Leveleditor genutzt
        public ElmaSimulator(SimulatorInputData data, Size panelSize, Camera2D camera, float timerIntercallInMilliseconds)
            :base(data, panelSize, camera, timerIntercallInMilliseconds)
        {
        }

        //Wird vom GameViewModel genutzt
        public ElmaSimulator(string levelFile, float timerIntercallInMilliseconds, Sounds sounds, string dataFolder, GraphicPanel2D panel)
            :base(levelFile, panel.Size, timerIntercallInMilliseconds)
        {
            Init(sounds, dataFolder, panel);
        }

        public void Init(Sounds sounds, string dataFolder, GraphicPanel2D panel)
        {
            this.DataFolder = dataFolder;
            this.panel = panel;
            this.sounds = sounds;

            this.bike = new Bike(this, this.sounds, dataFolder, panel);
            this.bike.HeadIsTouchingTheGround += Bike_HeadIsTouchingTheGround;
            this.apples = new Apples(this, this.sounds, this.bike, dataFolder);
            this.flower = new Flower(this, this.bike, this.apples, dataFolder);
            this.flower.BikeIsTouching += BikeIsTouchingFlowerHandler;
            this.isFinished = false;
            this.exploder = null;
            this.tickCounter = new TickCounterStopwatch();
        }

        private void Bike_HeadIsTouchingTheGround()
        {
            this.isFinished = true;
            this.sounds.PlayWallCrash();
            this.exploder = new VoronoiExploder(this.bike, this.panel);
            this.bike.RemoveBikeFromSimulation();
            this.tickCounter.StartTimer(() => { BikeIsBrokenHandler?.Invoke(); simulationIsFinished = true; }, 3);
            this.StopElapsedTimer?.Invoke();
        }

        private void BikeIsTouchingFlowerHandler()
        {
            if (this.isFinished == false)
            {
                this.isFinished = true;
                this.sounds.PlayCollectFlower();

                this.tickCounter.StartTimer(() => { this.FinishedIsReached?.Invoke(); simulationIsFinished = true; }, 3);
                this.StopElapsedTimer?.Invoke();
            }
        }

        public override void Draw(GraphicPanel2D panel)
        {
            base.Draw(panel);

            if (this.exploder != null)
            {
                this.exploder.Draw(this.panel);
            }

            //Zeichne die Distanz-Joints als Ketten, welche das Tag 'chain' haben
            foreach (var dis in this.GetJointsByTagName("chain"))
            {
                panel.DrawLineWithTexture(DataFolder + "Chain.png", dis.Anchor1.ToGrx(), dis.Anchor2.ToGrx(), 15, true);
            }

            //Ausgabe der verstrichenen Zeit
            panel.PushMatrix();
            panel.SetTransformationMatrixToIdentity();
            panel.DrawString(0, 0, Color.White, 20, this.elapsedTimeText);
            //panel.DrawString(0, 20, Color.White, 20, this.bike.GetMotorSpeed() + "");
            panel.PopMatrix();

            panel.FlipBuffer();
        }

        public override void MoveOneStep(float dt)
        {
            if (this.simulationIsFinished) return;

            if (this.HeadHasTouchedWall == false)
            {
                base.MoveOneStep(dt);

                this.bike.HandleTimerTick(dt);
                this.apples.HandleTimerTick(dt);
                this.flower.HandleTimerTick(dt);
            }else
            {
                //Wenn das Motorrad kaputt ist, dann bewegt der Exploder noch die Splitter
                if (this.exploder != null)
                {
                    this.exploder.HandleTimerTick(dt);
                }
                this.tickCounter.TimerTickHandler(dt); //Wenn das Motorrad kaputt ist oder das Ziel erreicht ist, dann läuft noch der tickCounter für 3 Sekunden
            }

            if (this.isFinished) return; //Blume oder Wand berührt? -> Zähle die Zeit nicht weiter
            this.timerTickCounter++;
            this.elapsedTimeText = TimerTickConverter.ToString(this.timerTickCounter, dt);
        }

        public override void HandleKeyDown(Key key)
        {
            if (key == System.Windows.Input.Key.Up)
            {
                this.bike.TurnMotorOn();
            }
            if (key == System.Windows.Input.Key.Down)
            {
                this.bike.ActivateBrake();
            }
            if (key == System.Windows.Input.Key.Left)
            {
                this.bike.HandleLeftIsDown();
            }
            if (key == System.Windows.Input.Key.Right)
            {
                this.bike.HandleRightIsDown();
            }
            if (key == System.Windows.Input.Key.Space)
            {
                this.bike.SpinDirection();
            }

            base.HandleKeyDown(key);
        }

        public override void HandleKeyUp(Key key)
        {
            if (key == System.Windows.Input.Key.Up)
            {
                this.bike.TurnMotorOff();
            }
            if (key == System.Windows.Input.Key.Down)
            {
                this.bike.ReleaseBrake();
            }
            if (key == System.Windows.Input.Key.Left)
            {
                this.bike.HandleLeftIsUp();
            }
            if (key == System.Windows.Input.Key.Right)
            {
                this.bike.HandleRightIsUp();
            }

            base.HandleKeyUp(key);
        }
    }
}
