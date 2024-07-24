using BridgeBuilderControl.Controls.Helper;
using BridgeBuilderControl.Controls.LevelEditor.Controls;
using BridgeBuilderControl.Controls.LevelEditor.Functions;
using GraphicPanels;
using GraphicPanelWpf;
using ReactiveUI;
using System;
using System.Drawing;
using System.IO;
using System.Reactive;
using System.Windows.Forms;

namespace BridgeBuilderControl.Controls.LevelEditor
{

    internal class LevelEditorViewModel : ITimerHandler, IGraphicPanelHandler
    {
        public class InputData
        {
            public Action GoBack;
        }

        private string DataFolder = null;

        private EditorState state;
        private IEditorFunction function;

        public SettingsViewModel Settings { get; private set; }
        public ReactiveCommand<Unit, Unit> SaveClick { get; private set; }
        public ReactiveCommand<Unit, Unit> LoadClick { get; private set; }
        public ReactiveCommand<Unit, Unit> GoBackClick { get; private set; }
        public ReactiveCommand<Unit, Unit> DefineGroundClick { get; private set; }
        public ReactiveCommand<Unit, Unit> DefineAnchorPointsClick { get; private set; }
        public ReactiveCommand<Unit, Unit> PasteImageFromClipboardClick { get; private set; }

        public ReactiveCommand<Unit, Unit> MoveLeft { get; private set; }
        public ReactiveCommand<Unit, Unit> MoveRight { get; private set; }
        public ReactiveCommand<Unit, Unit> MoveUp { get; private set; }
        public ReactiveCommand<Unit, Unit> MoveDown { get; private set; }

        public LevelEditorViewModel Init(InputData data)
        {
            this.GoBackClick = ReactiveCommand.Create(() =>
            {
                ClearLevel();
                data.GoBack();
            });
            return this;
        }

        public LevelEditorViewModel(GraphicPanel2D panel, string dataFolder)
        {
            this.DataFolder = dataFolder;

            this.state = new EditorState() { Panel = panel, MouseGrid = new MouseGrid(panel, 0) };            
            this.Settings = new SettingsViewModel(state);

            //Wenn XCount im SettingsViewModel verändert wird, dann wird MouseGrid aktualisiert
            this.Settings.WhenAnyValue(x => x.XCount).Subscribe(x => this.state.MouseGrid.XCount = x);

            this.SaveClick = ReactiveCommand.Create(() =>
            {
                System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                saveFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog.InitialDirectory = new DirectoryInfo(this.DataFolder + "Levels").FullName;
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    File.WriteAllText(saveFileDialog.FileName, this.GetExportString());
            });
            this.LoadClick = ReactiveCommand.Create(() =>
            {
                System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.InitialDirectory = new DirectoryInfo(this.DataFolder + "Levels").FullName;
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    this.LoadFromExportString(File.ReadAllText(openFileDialog.FileName));
                }
            });
            this.DefineGroundClick = ReactiveCommand.Create(() =>
            {
                UseFunction(FunctionType.DefineGround);
            });
            this.DefineAnchorPointsClick = ReactiveCommand.Create(() =>
            {
                UseFunction(FunctionType.DefineAnchorPoints);
            });
            this.PasteImageFromClipboardClick = ReactiveCommand.Create(() =>
            {
                if (Clipboard.ContainsImage())
                {
                    var clipboardImage = Clipboard.GetImage() as Bitmap;
                    if (clipboardImage != null)
                    {
                        panel.CreateOrUpdateNamedBitmapTexture("BackgroundEditorImage", clipboardImage);                        
                    }
                }
            });
            this.MoveLeft = ReactiveCommand.Create(() =>
            {
                Move(-1, 0);
            });
            this.MoveRight = ReactiveCommand.Create(() =>
            {
                Move(+1, 0);
            });
            this.MoveUp = ReactiveCommand.Create(() =>
            {
                Move(0, -1);
            });
            this.MoveDown = ReactiveCommand.Create(() =>
            {
                Move(0, +1);
            });

            UseFunction(FunctionType.Default);
        }

        private void UseFunction(FunctionType functionType)
        {
            switch (functionType)
            {
                case FunctionType.Default:
                    this.function = new DefaultFunction().Init(state);
                    break;
                case FunctionType.DefineGround:
                    this.function = new DefineGround().Init(state);
                    break;

                case FunctionType.DefineAnchorPoints:
                    this.function = new DefineAnchorPoints().Init(state);
                    break;
            }
        }

        private void Move(int x, int y)
        {
            UseFunction(FunctionType.Default);
            for (int i=0;i< this.state.AnchorPoints.Length;i++)
            {
                var p = this.state.AnchorPoints[i];
                this.state.AnchorPoints[i] = new Point(p.X + x, p.Y + y);
            }
            for (int i = 0; i < this.state.GroundPolygon.Length; i++)
            {
                var p = this.state.GroundPolygon[i];
                this.state.GroundPolygon[i] = new Point(p.X + x, p.Y);
            }
            this.state.GroundHeight = (uint)(this.state.GroundHeight + y);
        }

        private string GetExportString()
        {
            var data = new LevelExport();

            data.GroundPolygon = this.state.GroundPolygon;
            data.AnchorPoints = this.state.AnchorPoints;
            data.XCount = this.state.XCount;
            data.YCount = this.state.YCount;
            data.GroundHeight = this.state.GroundHeight;
            data.WaterHeight = this.state.WaterHeight;
            data.Budget = this.state.Budget;
            data.TrainExtraSpeed = this.state.TrainExtraSpeed;

            return JsonHelper.Helper.ToJson(data);
        }

        private void LoadFromExportString(string jsonExport)
        {
            var data = JsonHelper.Helper.CreateFromJson<LevelExport>(jsonExport);

            this.Settings.XCount = data.XCount;
            this.Settings.YCount = data.YCount;
            this.Settings.GroundHeight = data.GroundHeight;
            this.Settings.WaterHeight = data.WaterHeight;
            this.Settings.Budget = data.Budget;
            this.Settings.TrainExtraSpeed = data.TrainExtraSpeed;

            this.state.GroundPolygon = data.GroundPolygon;
            this.state.AnchorPoints = data.AnchorPoints;
            this.function.Init(this.state);
        }

        private void ClearLevel()
        {
            var panel = this.state.Panel;
            this.state = new EditorState() { Panel = panel, MouseGrid = new MouseGrid(panel, this.Settings.XCount) };
            UseFunction(FunctionType.Default);
        }

        #region ITimerHandler
        public void HandleTimerTick(float dt)
        {
            this.function.HandleTimerTick(dt);
        }

        #endregion

        #region IGraphicPanelHandler
        public void HandleSizeChanged(int width, int height)
        {
        }
        public void HandleMouseClick(System.Windows.Forms.MouseEventArgs e)
        {
            this.function.HandleMouseClick(e);
        }
        public void HandleMouseWheel(System.Windows.Forms.MouseEventArgs e)
        {

        }
        public void HandleMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            this.function.HandleMouseMove(e);
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
                if (this.function.Type != FunctionType.Default)
                {
                    UseFunction(FunctionType.Default);
                }else
                {
                    this.GoBackClick.Execute().Subscribe();
                }                
            }
        }
        public void HandleKeyUp(System.Windows.Input.KeyEventArgs e)
        {
        }


        #endregion
    }
}
