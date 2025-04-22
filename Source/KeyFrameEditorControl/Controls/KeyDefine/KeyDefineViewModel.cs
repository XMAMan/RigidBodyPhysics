using GraphicPanels;
using GraphicPanelWpf;
using KeyFrameEditorControl.Controls.ControlListBox;
using KeyFrameEditorControl.Controls.PlayAnimation;
using KeyFrameGlobal;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using static KeyFrameEditorControl.Controls.KeyDefine.FloatListCanvas;
using WpfControls.Controls.CameraSetting;
using KeyFrameEditorControl.Dialogs.LoadSprite;
using DynamicData;
using System.Drawing;

namespace KeyFrameEditorControl.Controls.KeyDefine
{
    //Diese Klasse dient zur grafischen Erzeugung/Definition von ein AnimationOutputData-Objekt
    //Während der Definition befinden sich in den Keys.Tag-Werten die FrameData-Objekte
    //Das KeyDefineControl definiert vom den FrameData-Objekten den Time-Wert und wie viele FrameData-Objekte es überhaupt gibt
    //Das ControlListBoxViewModel definiert für ein aktives FrameData-Objekt die Value-Werte (einzelne Gelenkwerte)
    //Der AnimatorInputData.AnimationModelDrawer dient zur Anzeige.
    //Über AnimatorInputData.Properties wird das anzuzeigende Objekt verändert
    public class KeyDefineViewModel : ReactiveObject, ISizeChangeable, ITimerHandler
    {
        private GraphicPanel2D panel;
        private AnimatorInputData animationData;
        private FrameToTimeConverter frameToTimeConverter;
        private FrameData timeNullData; //Zustand des zu animierenden Objekts zum Zeitpunkt (t=0)
        private FrameInterpolator frameInterpolator = null;
        private LoadSpriteViewModel loadSpriteViewModel = new LoadSpriteViewModel();
        private LoadSpriteWindow loadSpriteWindow = null;


        [Reactive] public ControlListBoxViewModel BoxViewModel { get; set; }
        [Reactive] public PlayAnimationViewModel PlayAnimationViewModel { get; set; }
        [Reactive] public CameraSettingViewModel CameraViewModel { get; set; }
        [Reactive] public float TimePosition { get; set; } = 0; //Wiedergabeposition
        public ReactiveCommand<Unit, Unit> RevertKeysClick { get; private set; }
        public ReactiveCommand<Unit, Unit> ChangeBackgroundImageClick { get; private set; }

        public ObservableCollection<FloatListCanvas.Entry> Keys { get; private set; } = new ObservableCollection<FloatListCanvas.Entry>();

        [Reactive] public FloatListCanvas.Entry SelectedKey { get; set; }

        public ReactiveCommand<FloatListCanvas.Entry, Unit> NewKeyHandler { get; private set; } //Command (Event) wenn ein neuer Key erzeugt wurde

        public KeyDefineViewModel(GraphicPanel2D panel, AnimatorInputData animationData, float timerTickRateInMs, bool showStartTimeTextbox)
        {
            this.panel = panel;
            this.timeNullData = animationData.Properties.GetFrameFromAnimatedObject(0);
            this.animationData = animationData;

            this.PlayAnimationViewModel = new PlayAnimationViewModel(timerTickRateInMs, showStartTimeTextbox);
            this.frameToTimeConverter = new FrameToTimeConverter(this.PlayAnimationViewModel.Frames, this.PlayAnimationViewModel.AnimationType, 0);
            this.BoxViewModel = new ControlListBoxViewModel(animationData);
            this.CameraViewModel = new CameraSettingViewModel(panel.Width, panel.Height, animationData.AnimationModelDrawer.GetBoundingBoxFromScene());

            this.NewKeyHandler = ReactiveCommand.Create<FloatListCanvas.Entry>((entry) =>
            {
                entry.Tag = this.BoxViewModel.GetFrameData(entry.Value); //Weise dem neu angeklickten Key die aktuellen Animationdaten zu

                if (PlayAnimationViewModel.IsRunning)
                    this.frameInterpolator = CreateNewInterpolator();
            });

            //Animation rückwärts laufen lassen indem die Timeposition von jeden Key umgekehrt wird
            this.RevertKeysClick = ReactiveCommand.Create(() =>
            {
                foreach (var key in this.Keys)
                {
                    key.Value = 1 - key.Value; //Time-Position von jeden Key umkehren
                }

                //Trigger das CollectionChanged-Event zur View, so dass sie die Anzeige aktualisiert
                var dummy = new Entry(-1);
                this.Keys.Add(dummy);
                this.Keys.Remove(dummy);
            });


            this.WhenAnyValue(x => x.SelectedKey).Subscribe(x =>
            {
                this.BoxViewModel.SelectedFrame = x != null ? (FrameData)x.Tag : null;
            });

            this.PlayAnimationViewModel.WhenAnyValue(x => x.IsRunning).Subscribe(isRunning =>
            {
                if (isRunning)
                    this.frameInterpolator = CreateNewInterpolator();
                else
                    this.frameInterpolator = null;

            });
            this.PlayAnimationViewModel.RestartClick.Subscribe((x) =>
            {
                this.frameToTimeConverter.Reset();
                this.animationData.AnimationObject.ResetToInitialState();
            });
            this.PlayAnimationViewModel.WhenAnyValue(x => x.DecreasePositionIsPressed).Subscribe((isPressed) =>
            {
                this.frameToTimeConverter.PlayBackwards = isPressed;
            });
            this.PlayAnimationViewModel.WhenAnyValue(x => x.IncreasePositionIsPressed).Subscribe((isPressed) =>
            {
                this.frameToTimeConverter.PlayForward = isPressed;
            });
            this.PlayAnimationViewModel.WhenAnyValue(x => x.Frames).Subscribe(frames =>
            {
                this.frameToTimeConverter.FrameAnimationCount = frames;
            });
            this.PlayAnimationViewModel.WhenAnyValue(x => x.AnimationType).Subscribe(animationType =>
            {
                this.frameToTimeConverter.AnimationType = animationType;
            });


            //Hier ist der Sprite-Block
            //Sprite-Bild als Hintergrundbild festelegen um damit eine Animation nachbauen zu können
            this.ChangeBackgroundImageClick = ReactiveCommand.Create(() =>
            {
                if (this.loadSpriteWindow != null)
                    this.loadSpriteWindow.Close();

                this.loadSpriteWindow = new LoadSpriteWindow() { DataContext = this.loadSpriteViewModel };
                this.loadSpriteWindow.Show();
            });
            this.loadSpriteViewModel.WhenAnyValue(x => x.Image).Subscribe(vm =>
            {
                if (this.loadSpriteViewModel.Image != null)
                {
                    this.panel.CreateOrUpdateNamedBitmapTexture("Sprite", this.loadSpriteViewModel.Image);
                }
            });
        }

        public AnimationOutputData GetAnimationData()
        {
            foreach (var keys in this.Keys)
            {
                var frame = (FrameData)keys.Tag;
                frame.Time = keys.Value;
            }
            var keyFrames = this.Keys.Select(x => (FrameData)x.Tag).OrderBy(x => x.Time).ToList();

            bool[] propertyIsAnimated = this.BoxViewModel.IsPropertyAnimated;
            if (keyFrames.Any() == false)
                propertyIsAnimated = new bool[this.BoxViewModel.IsPropertyAnimated.Length]; //Wenn keine Keys definiert sind soll auch keine Animation erfolgen

            if (keyFrames.Any() == false || keyFrames.First().Time > 0)
            {
                keyFrames.Insert(0, this.timeNullData);
            }

            return new AnimationOutputData(
                keyFrames.ToArray(),
                (float)this.PlayAnimationViewModel.Seconds,
                this.PlayAnimationViewModel.AnimationType,
                propertyIsAnimated,
                this.PlayAnimationViewModel.StartTime
                );
        }

        public void LoadAnimationData(AnimationOutputData data)
        {
            this.SelectedKey = null;
            this.Keys.Clear();
            this.PlayAnimationViewModel.AnimationType = data.Type;
            this.PlayAnimationViewModel.Seconds = data.DurrationInSeconds;
            this.PlayAnimationViewModel.StartTime = data.StartTime;
            this.Keys.AddRange(data.Frames.Select(x => new Entry(x.Time) { Tag = x }));
            this.BoxViewModel.IsPropertyAnimated = data.PropertyIsAnimated;
        }

        private FrameInterpolator CreateNewInterpolator()
        {
            return new FrameInterpolator(this.animationData.Properties, GetAnimationData());
        }

        private void PlayAnimationTimerTick()
        {
            if (this.PlayAnimationViewModel.IsRunning && this.frameInterpolator != null)
            {
                this.frameToTimeConverter.HandleTimerTick(0); //Erhöhe den internen TimerTick-Zähler
                var frame = this.frameInterpolator.GetFrame(this.frameToTimeConverter.Time);
                this.animationData.Properties.WriteFrameToAnimatedObject(frame, this.BoxViewModel.IsPropertyAnimated); //Schreibe den interpolierten Frame auf das Animation-Anzeigeobjekt
            }
        }

        public void HandleTimerTick(float dt)
        {
            PlayAnimationTimerTick();
            this.TimePosition = this.frameToTimeConverter.Time;

            this.animationData.AnimationObject.HandleTimerTick(dt);
            DrawAnimationObject();
        }

        private void DrawAnimationObject()
        {
            panel.ClearScreen(Color.White);

            //Soll im Hintergrund ein Spritebild angezeigt werden?
            if (this.loadSpriteViewModel.Image != null)
            {
                var v = this.loadSpriteViewModel;

                int spriteNr = v.SpriteNr;
                if (v.ShowAnimatedSprite) //Soll es sich von alleine bewegen?
                {
                    spriteNr = (int)v.SpriteAnimaitonPosition;
                    v.SpriteAnimaitonPosition += v.AnimationSpeed;
                    if (v.SpriteAnimaitonPosition > v.ImageCount)
                        v.SpriteAnimaitonPosition = 0;
                }
                panel.DrawSprite("Sprite", v.XCount, v.YCount, spriteNr % v.XCount, spriteNr / v.XCount, (int)(panel.Width * v.XPosition), (int)(panel.Height * v.YPosition), (int)(v.SingleImageWidth * v.Zoom), (int)(v.SingleImageHeight * v.Zoom), 0, true, Color.FromArgb(255, 255, 255, 255));
            }

            this.animationData.AnimationModelDrawer.Draw(this.CameraViewModel.Camera);
            panel.FlipBuffer();
        }

        #region ISizeChangeable
        public void HandleSizeChanged(int width, int height)
        {
            this.CameraViewModel.HandleSizeChanged(width, height);
        }
        #endregion
    }
}
