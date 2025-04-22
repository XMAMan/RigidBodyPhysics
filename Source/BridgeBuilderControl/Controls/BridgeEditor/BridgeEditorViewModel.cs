using BridgeBuilderControl.Controls.BridgeEditor.Model;
using BridgeBuilderControl.Controls.Helper;
using BridgeBuilderControl.Controls.LevelEditor;
using BridgeBuilderControl.Controls.Simulator.Model;
using GraphicPanels;
using GraphicPanelWpf;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.IO;
using System.Reactive;
using System.Windows;

namespace BridgeBuilderControl.Controls.BridgeEditor
{
    internal class BridgeEditorViewModel : ReactiveObject, ITimerHandler, IGraphicPanelHandler
    {
        public class InputData
        {
            public Action GoBack;
            public Action<SimulatorInput> Test;
            public ITextDialog SaveDialog;
        }

        private GraphicPanel2D panel;
        private string dataFolder;
        private string levelFile;
        private string bridgeFile;
        private DefineBridgeFunction model;

        public ReactiveCommand<Unit, Unit> GoBackClick { get; private set; }
        public ReactiveCommand<Unit, Unit> TestClick { get; private set; }
        public ReactiveCommand<Unit, Unit> ClearClick { get; private set; }
        public ReactiveCommand<Unit, Unit> SaveClick { get; private set; }        
        public ReactiveCommand<Unit, Unit> ZoomInMouseDown { get; private set; }
        public ReactiveCommand<Unit, Unit> ZoomInMouseUp { get; private set; }
        public ReactiveCommand<Unit, Unit> ZoomOutMouseDown { get; private set; }
        public ReactiveCommand<Unit, Unit> ZoomOutMouseUp { get; private set; }

        private bool zoomInIsPressed = false;
        private bool zoomOutIsPressed = false;

        [Reactive] public int Cost { get; set; }
        [Reactive] public int Budget { get; set; }
        [Reactive] public System.Windows.Media.SolidColorBrush CostColor { get; set; } = System.Windows.Media.Brushes.Gray;

        public BridgeEditorViewModel Init(InputData data)
        {
            this.GoBackClick = ReactiveCommand.Create(() =>
            {
                data.GoBack();
            });
            this.TestClick = ReactiveCommand.Create(() =>
            {
                SaveBridge();
                data.Test(model.GetSimulatorInputData(this.levelFile));
            });
            this.SaveClick = ReactiveCommand.CreateFromTask(async () =>
            {
                bool isUserBridge = new FileInfo(this.bridgeFile).Directory.Name == "UserBridges";
                string inputText = isUserBridge ? Path.GetFileNameWithoutExtension(this.bridgeFile) : "";
                string newUserBridgeFileName = await data.SaveDialog.ShowDialogWithTextAnswer(inputText);
                if (string.IsNullOrEmpty(newUserBridgeFileName) == false)
                {
                    if (newUserBridgeFileName.EndsWith(".txt") == false)
                    {
                        newUserBridgeFileName += ".txt";
                    }
                    this.bridgeFile = new DirectoryInfo(this.dataFolder).FullName + "\\UserBridges\\" + newUserBridgeFileName;
                    SaveBridge();
                }
            });           
            this.ClearClick = ReactiveCommand.Create(() =>
            {
                this.model.Clear();
            });
            this.ZoomInMouseDown = ReactiveCommand.Create(() =>
            {
                this.zoomInIsPressed = true;
            });

            this.ZoomInMouseUp = ReactiveCommand.Create(() =>
            {
                this.zoomInIsPressed = false;
            });

            this.ZoomOutMouseDown = ReactiveCommand.Create(() =>
            {
                this.zoomOutIsPressed = true;
            });

            this.ZoomOutMouseUp = ReactiveCommand.Create(() =>
            {
                this.zoomOutIsPressed = false;
            });

            return this;
        }

        public BridgeEditorViewModel(GraphicPanel2D panel, string dataFolder) 
        {
            this.panel = panel;
            this.dataFolder = dataFolder;
        }

        public void LoadUserBridgeFile(string userBridgeFile)
        {
            var bridgeData = JsonHelper.Helper.CreateFromJson<BridgeExport>(File.ReadAllText(userBridgeFile));
            string levelFile = new DirectoryInfo(this.dataFolder).FullName + "\\Levels\\" + bridgeData.AssociatedLevel;
            LoadLevelAndBridge(levelFile, userBridgeFile);
        }

        //levelFile = Datei welche vom LevelEditor erstellt wurde
        public void LoadLevel(string levelFile)
        {
            this.bridgeFile = new DirectoryInfo(this.dataFolder).FullName + "\\Bridges\\" + new FileInfo(levelFile).Name;
            LoadLevelAndBridge(levelFile, this.bridgeFile);
        }

        //levelFile = Datei welche vom LevelEditor erstellt wurde; bridgeFile = Wurde über BridgeEditor.Save erstellt
        private void LoadLevelAndBridge(string levelFile, string bridgeFile)
        {
            var levelExport = JsonHelper.Helper.CreateFromJson<LevelExport>(File.ReadAllText(levelFile));
            this.model = new DefineBridgeFunction(this.panel, levelExport, () => 
            {
                UpdateCost();
            });
            this.Budget = (int)levelExport.Budget;
            this.levelFile = new FileInfo(levelFile).Name;
            this.bridgeFile = bridgeFile;

            if (File.Exists(bridgeFile))
            {
                var bridgeData = JsonHelper.Helper.CreateFromJson<BridgeExport>(File.ReadAllText(bridgeFile));
                this.model.LoadFromExport(bridgeData);
            }

            UpdateCost();
        }

        private void UpdateCost()
        {
            this.Cost = this.model.BarCount * 100;
            this.CostColor = Cost < this.Budget ? System.Windows.Media.Brushes.Gray : System.Windows.Media.Brushes.Red;

        }

        private void SaveBridge()
        {
            try
            {
                var bridgeData = this.model.GetExport(this.levelFile);
                string json = JsonHelper.Helper.ToJson(bridgeData);
                File.WriteAllText(this.bridgeFile, json);
            }catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            
        }

        #region ITimerHandler
        public void HandleTimerTick(float dt)
        {
            this.model.HandleTimerTick(dt, this.zoomInIsPressed, this.zoomOutIsPressed);
        }

        #endregion

        #region IGraphicPanelHandler
        public void HandleSizeChanged(int width, int height)
        {
            this.model.HandleSizeChanged(width, height);
        }
        public void HandleMouseClick(System.Windows.Forms.MouseEventArgs e)
        {
            this.model.HandleMouseClick(e);
        }
        public void HandleMouseWheel(System.Windows.Forms.MouseEventArgs e)
        {

        }
        public void HandleMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            this.model.HandleMouseMove(e);
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
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                this.GoBackClick.Execute().Subscribe();
            }

            if (e.Key == System.Windows.Input.Key.T)
            {
                this.TestClick.Execute().Subscribe();
            }
        }
        public void HandleKeyUp(System.Windows.Input.KeyEventArgs e)
        {
        }


        #endregion
    }
}
