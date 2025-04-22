using BridgeBuilderControl.Controls.BridgeEditor;
using BridgeBuilderControl.Controls.Helper;
using BridgeBuilderControl.Controls.LevelEditor;
using BridgeBuilderControl.Controls.LevelSelect;
using BridgeBuilderControl.Controls.MainSelect;
using BridgeBuilderControl.Controls.OpenDialog;
using BridgeBuilderControl.Controls.Readme;
using BridgeBuilderControl.Controls.SaveDialog;
using BridgeBuilderControl.Controls.Simulator;
using GameHelper;
using GraphicPanelWpf;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using WpfControls.Model;

namespace BridgeBuilderControl.Controls.Main
{
    //Legt fest, welches UserControl angezeigt wird
    internal class MainViewModel : ReactiveObject, ITimerHandler, IGraphicPanelHandler, IPhysicSimulated
    {
        private SubControlFactory factory;

        private UserControl mainSelect, levelSelect, levelEditor, bridgeEditor, simulator, saveDialog, openDialog, readme;
        private string dataFolder;
        private Stack<ControlType> visitedControls = new Stack<ControlType>();
        private bool useStack = true; //Soll bei Nutzung der SelectedControl-Property der Stack 'visitedControls' genutzt werden?

        [Reactive] public System.Windows.Controls.UserControl ContentUserControl { get; set; }

        public MainViewModel(EditorInputData data)
        {
            this.factory = new SubControlFactory(data);
            this.dataFolder = data.DataFolder;

            this.mainSelect = this.factory.CreateControl(ControlType.MainSelect);
            this.levelSelect = this.factory.CreateControl(ControlType.LevelSelect);
            this.levelEditor = this.factory.CreateControl(ControlType.LevelEditor);
            this.bridgeEditor = this.factory.CreateControl(ControlType.BridgeEditor);
            this.simulator = this.factory.CreateControl(ControlType.Simulator);
            this.saveDialog = this.factory.CreateControl(ControlType.SaveDialog);
            this.openDialog = this.factory.CreateControl(ControlType.OpenDialog);
            this.readme = this.factory.CreateControl(ControlType.Readme);

            (this.mainSelect.DataContext as MainSelectViewModel).Init(new MainSelectViewModel.InputData()
            {
                StartGame = () =>
                {
                    (this.levelSelect.DataContext as LevelSelectViewModel).Update();
                    this.SelectedControl = ControlType.LevelSelect;
                },
                ShowLevelEditor = () => { this.SelectedControl = ControlType.LevelEditor; },
                LoadBridgeDialog = new ContentControlTextDialog(new ContentControlTextDialog.InputData()
                {
                    ShowDialogAction = (inputText, cancelAction, answerAction) =>
                    {
                        var openDialogVm = (this.openDialog.DataContext as OpenDialogViewModel);

                        openDialogVm.Init(new OpenDialogViewModel.InputData()
                        {
                            InitialDirectory = new DirectoryInfo(this.dataFolder).FullName + "\\UserBridges\\",
                            Cancel = cancelAction,
                            OpenFileAction = answerAction
                        }) ;

                        //Zeige den OpenDialog
                        this.SelectedControl = ControlType.OpenDialog;
                    },
                    CloseDialogAction = () =>
                    {
                        //Aktiviere das Fenster, aus dem der OpenDialog gestartet wurde
                        GoBack();
                    }
                }),
                ShowBridgeFile = (bridgeFile) => 
                {
                    (this.bridgeEditor.DataContext as BridgeEditorViewModel).LoadUserBridgeFile(bridgeFile);
                    this.SelectedControl = ControlType.BridgeEditor;
                },
                ShowReadme = () =>
                {
                    this.SelectedControl = ControlType.Readme;
                }
            });
            (this.levelSelect.DataContext as LevelSelectViewModel).Init(new LevelSelectViewModel.InputData()
            {
                GoBack = () => { GoBack(); },
                SelectLevel = (fileName)=> 
                {
                    (this.bridgeEditor.DataContext as BridgeEditorViewModel).LoadLevel(fileName);
                    this.SelectedControl = ControlType.BridgeEditor;
                }
            });
            (this.levelEditor.DataContext as LevelEditorViewModel).Init(new LevelEditorViewModel.InputData()
            {
                GoBack = () => { GoBack(); }
            });
            (this.bridgeEditor.DataContext as BridgeEditorViewModel).Init(new BridgeEditorViewModel.InputData()
            {
                GoBack = () => { GoBack(); },
                Test = (simulatorData) => 
                {
                    (this.simulator.DataContext as SimulatorViewModel).SimulateLevel(simulatorData);
                    this.SelectedControl = ControlType.Simulator; 
                },
                SaveDialog = new ContentControlTextDialog(new ContentControlTextDialog.InputData()
                {
                    ShowDialogAction = (inputText, cancelAction, answerAction) =>
                    {
                        var saveDialogVm = (this.saveDialog.DataContext as SaveDialogViewModel);

                        saveDialogVm.Init(new SaveDialogViewModel.InputData()
                        {
                            FileName = inputText,   //Zeige den Inputtext
                            Cancel = cancelAction,  //
                            Save = answerAction
                        });

                        //Zeige den SaveDialog
                        this.SelectedControl = ControlType.SaveDialog;
                    },
                    CloseDialogAction = ()=>
                    {
                        //Aktiviere das Fenster, aus dem der Savedialog gestartet wurde
                        GoBack();
                    }                    
                })
            });
            (this.simulator.DataContext as SimulatorViewModel).Init(new SimulatorViewModel.InputData()
            {
                GoBack = () => { GoBack(); }
            });
            (this.readme.DataContext as ReadmeViewModel).Init(new ReadmeViewModel.InputData()
            {
                GoBack = () => { GoBack(); }
            });


            this.SelectedControl = ControlType.MainSelect;
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
                if (this.useStack)
                {
                    this.visitedControls.Push(value);
                }                
                switch (this.selectedControl)
                {
                    case ControlType.Nothing:
                        this.ContentUserControl = null; break;

                    case ControlType.MainSelect:
                        this.ContentUserControl = this.mainSelect; break;

                    case ControlType.LevelSelect:
                        this.ContentUserControl = this.levelSelect; break;                    

                    case ControlType.LevelEditor:
                        this.ContentUserControl = this.levelEditor; break;

                    case ControlType.BridgeEditor:
                        this.ContentUserControl = this.bridgeEditor; break;

                    case ControlType.Simulator:
                        this.ContentUserControl = this.simulator; break;

                    case ControlType.SaveDialog:
                        this.ContentUserControl = this.saveDialog; break;

                    case ControlType.OpenDialog:
                        this.ContentUserControl = this.openDialog; break;

                    case ControlType.Readme:
                        this.ContentUserControl = this.readme; break;
                }
            }
        }

        //Aktiviere das vorherige Fenster
        private void GoBack()
        {
            if (this.visitedControls.Any())
            {
                this.visitedControls.Pop(); //Verwerfe das Fenster wo du gerade bist

                this.useStack = false; //Verhindere, dass das Fenster, auf das zurück gegangen wird zwei mal auf den Stack kommt
                this.SelectedControl = this.visitedControls.Peek(); //Aktivere das vorherige Fenster
                this.useStack = true;
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
            throw new NotImplementedException();
            //return (this.gameControl.DataContext as GameViewModel).StartReplay(this.dataFolder + "Levels\\" + levelFile, this.dataFolder + "Recordings\\" + keyboardFile);
        }

        //Return: KeyboardPlayer.IsFinish
        public bool DoTimeStep(float dt)
        {
            throw new NotImplementedException();
            //return (this.gameControl.DataContext as GameViewModel).DoTimeStep(dt);
        }
        #endregion
    }
}
