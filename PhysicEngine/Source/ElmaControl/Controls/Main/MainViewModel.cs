using ElmaControl.Controls.Game;
using ElmaControl.Controls.Game.Model;
using ElmaControl.Controls.LevelSelect;
using ElmaControl.Controls.MainSelect;
using ElmaControl.Controls.SingleLevel;
using GameHelper;
using GraphicPanelWpf;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Simulator;
using System.Windows.Controls;
using WpfControls.Model;

namespace ElmaControl.Controls.Main
{
    //Legt fest, welches Unter-Control angezeigt wird
    internal class MainViewModel : ReactiveObject, ITimerHandler, IGraphicPanelHandler, IPhysicSimulated
    {
        private SubControlFactory factory;

        private UserControl mainSelect, levelSelect, singleLevel, gameControl, levelEditor, spriteEditor;
        private float timerTickRateInMs;
        private string dataFolder;
        private string emptyLevelFile;
        private string spriteFile;

        [Reactive] public System.Windows.Controls.UserControl ContentUserControl { get; set; }

        public MainViewModel(EditorInputData data)
        {
            //GoBack-Handler vom Level- und SpriteEditor
            data.IsFinished = (sender) =>
            {
                if (this.SelectedControl == ControlType.SpriteEditor)
                {
                    //Aktualisiere die Sprite-Image-Dateien wenn vorher der SpriteEditor genutzt wurde
                    (this.spriteEditor.DataContext as IToTextWriteable).WriteToTextFile(this.spriteFile);
                    UpdateSpriteImages();
                }
                this.SelectedControl = ControlType.MainSelect;
            };

            this.factory = new SubControlFactory(data);
            this.timerTickRateInMs = data.TimerTickRateInMs;
            this.dataFolder = data.DataFolder;
            this.emptyLevelFile = data.DataFolder + "Levels\\EmptyLevel.txt";
            this.spriteFile = data.DataFolder + "Motorbike_Sprite.txt";

            this.mainSelect = this.factory.CreateControl(ControlType.MainSelect);            
            this.levelSelect = this.factory.CreateControl(ControlType.LevelSelect);
            this.singleLevel = this.factory.CreateControl(ControlType.SingleLevel);
            this.gameControl = this.factory.CreateControl(ControlType.Game);
            this.levelEditor = this.factory.CreateControl(ControlType.LevelEditor);
            this.spriteEditor = this.factory.CreateControl(ControlType.SpriteEditor);

            (this.mainSelect.DataContext as MainSelectViewModel).SelectedEntryChanged += MainSelectViewModel_SelectedEntryChanged;

            (this.levelSelect.DataContext as LevelSelectViewModel).SelectLevelHandler += LevelSelect_SelectLevelHandler;
            (this.levelSelect.DataContext as LevelSelectViewModel).GoBackHandler += LevelSelect_GoBackHandler;

            (this.singleLevel.DataContext as SingleLevelViewModel).PlayLevelHandler += SingleLevel_PlayLevelHandler;
            (this.singleLevel.DataContext as SingleLevelViewModel).ReplayLevelHandler += SingleLevel_ReplayLevelHandler;
            (this.singleLevel.DataContext as SingleLevelViewModel).GoBackHandler += SingleLevel_GoBackHandler;

            (this.gameControl.DataContext as GameViewModel).BikeIsBrokenHandler += Game_BikeIsBrokenHandler;
            (this.gameControl.DataContext as GameViewModel).FinishedIsReached += Game_FinishedIsReached;
            (this.gameControl.DataContext as GameViewModel).GoBackHandler += Game_GoBackHandler;

            //Übergebe Builder-Methode für ein ILeveleditorUsedSimulator-Objekt
            (this.levelEditor.DataContext as ISimlatorUser).SetSimulatorBuildMethod(
                (data, panelSize, camera, timerIntercallInMilliseconds) => (this.gameControl.DataContext as GameViewModel).CreateSimulator(data, panelSize, camera, timerIntercallInMilliseconds)
                );

            this.SelectedControl = ControlType.MainSelect;
        }

        private void UpdateSpriteImages()
        {
            var spriteData1 = SpriteEditorControl.SpriteDataCreator.CreateFromSpriteEditorFile(this.spriteFile, 0);
            var spriteData2 = SpriteEditorControl.SpriteDataCreator.CreateFromSpriteEditorFile(this.spriteFile, 1);

            BitmapHelper.BitmapHelp.ScaleImageDownWithoutColorInterpolation(spriteData1.Image, Bike.ScaleFactor).Save(this.dataFolder + "Sprite1.png");
            BitmapHelper.BitmapHelp.ScaleImageDownWithoutColorInterpolation(spriteData2.Image, Bike.ScaleFactor).Save(this.dataFolder + "Sprite2.png");
        }

        private void MainSelectViewModel_SelectedEntryChanged(MainSelectViewModel.SelectedEntry entry)
        {
            switch (entry)
            {
                case MainSelectViewModel.SelectedEntry.LevelSelect:
                    this.SelectedControl = ControlType.LevelSelect;
                    (this.levelSelect.DataContext as LevelSelectViewModel).Update();
                    break;

                case MainSelectViewModel.SelectedEntry.Leveleditor:
                    this.SelectedControl = ControlType.LevelEditor;
                    (this.levelEditor.DataContext as IToTextWriteable).LoadFromTextFile(this.emptyLevelFile);

                    
                    break;

                case MainSelectViewModel.SelectedEntry.SpriteEditor:
                    this.SelectedControl = ControlType.SpriteEditor;                    
                    (this.spriteEditor.DataContext as IToTextWriteable).LoadFromTextFile(this.spriteFile);
                    break;

                case MainSelectViewModel.SelectedEntry.Quit:
                    System.Windows.Application.Current.Shutdown();
                    break;
            }
        }


        private void LevelSelect_SelectLevelHandler(string filePath)
        {
            (this.singleLevel.DataContext as SingleLevelViewModel).Init(SingleLevelViewModel.PreviousState.LevelSelect, filePath, null, this.timerTickRateInMs);
            this.SelectedControl = ControlType.SingleLevel;
        }
        private void LevelSelect_GoBackHandler()
        {
            this.SelectedControl = ControlType.MainSelect;
        }


        private void SingleLevel_PlayLevelHandler(string levelFile)
        {
            (this.gameControl.DataContext as GameViewModel).StartPlay(levelFile);
            this.SelectedControl = ControlType.Game;
        }
        private void SingleLevel_ReplayLevelHandler(string levelFile, string keyboardFile)
        {
            (this.gameControl.DataContext as GameViewModel).StartReplay(levelFile, keyboardFile);
            this.SelectedControl = ControlType.Game;
        }
        private void SingleLevel_GoBackHandler()
        {
            this.SelectedControl = ControlType.LevelSelect;
        }


        private void Game_BikeIsBrokenHandler(string levelFile, string keyboardFile, bool isReplay)
        {
            (this.singleLevel.DataContext as SingleLevelViewModel).Init(isReplay ? SingleLevelViewModel.PreviousState.ReplayIsFinished : SingleLevelViewModel.PreviousState.BikeIsBroken, levelFile, keyboardFile, this.timerTickRateInMs);
            this.SelectedControl = ControlType.SingleLevel;
        }

        private void Game_FinishedIsReached(string levelFile, string keyboardFile, bool isReplay)
        {
            (this.singleLevel.DataContext as SingleLevelViewModel).Init(isReplay ? SingleLevelViewModel.PreviousState.ReplayIsFinished : SingleLevelViewModel.PreviousState.LevelFinished, levelFile, keyboardFile, this.timerTickRateInMs);
            this.SelectedControl = ControlType.SingleLevel;
        }

        private void Game_GoBackHandler(string levelFile, string keyboardFile)
        {
            (this.singleLevel.DataContext as SingleLevelViewModel).Init(SingleLevelViewModel.PreviousState.GoBackFromGame, levelFile, keyboardFile, this.timerTickRateInMs);
            this.SelectedControl = ControlType.SingleLevel;
        }


        private ControlType selectedControl = ControlType.Nothing;
        private ControlType SelectedControl
        {
            get
            {
                return this.selectedControl;
            }
            set
            {
                this.selectedControl = value;
                switch (this.selectedControl)
                {
                    case ControlType.Nothing:
                        this.ContentUserControl = null; break;

                    case ControlType.MainSelect:
                        this.ContentUserControl = this.mainSelect; break;

                    case ControlType.LevelSelect:
                        this.ContentUserControl = this.levelSelect; break;

                    case ControlType.SingleLevel:
                        this.ContentUserControl = this.singleLevel; break;

                    case ControlType.Game:
                        this.ContentUserControl = this.gameControl; break;

                    case ControlType.LevelEditor:
                        this.ContentUserControl = this.levelEditor; break;

                    case ControlType.SpriteEditor:
                        this.ContentUserControl = this.spriteEditor; break;
                }
            }
        }

        #region ITimerHandler
        public void HandleTimerTick(float dt)
        {
            if (this.ContentUserControl?.DataContext is ITimerHandler)
                (this.ContentUserControl.DataContext as ITimerHandler).HandleTimerTick(dt);
        }
        #endregion

        #region IGraphicPanelHandler
        public void HandleSizeChanged(int width, int height)
        {
            if (this.ContentUserControl?.DataContext is ISizeChangeable)
                (this.ContentUserControl.DataContext as ISizeChangeable).HandleSizeChanged(width, height);
        }
        public void HandleMouseClick(System.Windows.Forms.MouseEventArgs e)
        {
            if (this.ContentUserControl?.DataContext is IGraphicPanelHandler)
                (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleMouseClick(e);
        }
        public void HandleMouseWheel(System.Windows.Forms.MouseEventArgs e)
        {
            if (this.ContentUserControl?.DataContext is IGraphicPanelHandler)
                (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleMouseWheel(e);
        }
        public void HandleMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            if (this.ContentUserControl?.DataContext is IGraphicPanelHandler)
                (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleMouseMove(e);
        }
        public void HandleMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            if (this.ContentUserControl?.DataContext is IGraphicPanelHandler)
                (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleMouseDown(e);
        }
        public void HandleMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            if (this.ContentUserControl?.DataContext is IGraphicPanelHandler)
                (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleMouseUp(e);
        }
        public void HandleMouseEnter()
        {
            if (this.ContentUserControl?.DataContext is IGraphicPanelHandler)
                (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleMouseEnter();
        }
        public void HandleMouseLeave()
        {
            if (this.ContentUserControl?.DataContext is IGraphicPanelHandler)
                (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleMouseLeave();
        }

        public void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (this.ContentUserControl?.DataContext is IKeyDownUpHandler)
                (this.ContentUserControl.DataContext as IKeyDownUpHandler).HandleKeyDown(e);
        }
        public void HandleKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            if (this.ContentUserControl?.DataContext is IKeyDownUpHandler)
                (this.ContentUserControl.DataContext as IKeyDownUpHandler).HandleKeyUp(e);
        }
        #endregion

        #region IPhysicSimulated
        public int LoadSimulation(string levelFile, string keyboardFile)
        {
            return (this.gameControl.DataContext as GameViewModel).StartReplay(this.dataFolder + "Levels\\" + levelFile, this.dataFolder + "Recordings\\" + keyboardFile);
        }

        //Return: KeyboardPlayer.IsFinish
        public bool DoTimeStep(float dt)
        {
            return (this.gameControl.DataContext as GameViewModel).DoTimeStep(dt);
        }
        #endregion
    }
}
