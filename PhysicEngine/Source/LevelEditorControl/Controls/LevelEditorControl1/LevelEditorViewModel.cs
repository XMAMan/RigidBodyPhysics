using GraphicPanels;
using GraphicPanelWpf;
using LevelEditorGlobal;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using WpfControls.Model;
using LevelEditorControl.Controls.EditorControl;
using PhysicItemEditorControl.Model;
using System.Windows.Controls;
using Simulator;

namespace LevelEditorControl.Controls.LevelEditorControl1
{
    //Die Aufgabe von dieser Klasse ist es das ContentUserControl anzuzeigen und das GraphicPanel2D-Objekt und Timer anzulegen
    //Er bekommt von den Control, was er gerade anzeigt dann ein Event, wenn er das Control wechseln soll
    internal class LevelEditorViewModel : ReactiveObject, IGraphicPanelHandler, ITimerHandler, IToTextWriteable, IObjectSerializable, ISimlatorUser
    {
        private GraphicPanel2D panel;
        private float timerIntercallInMilliseconds;

        public enum SubControl { MainControl, AddNewPhysicItem }
        [Reactive] public System.Windows.Controls.UserControl ContentUserControl { get; set; }

        public EditorViewModel MainViewModel { get; set; }

        public LevelEditorViewModel(EditorInputData data)
        {
            this.panel = data.Panel;
            this.timerIntercallInMilliseconds = data.TimerTickRateInMs;

            this.MainViewModel = new EditorViewModel(data, new EditorViewModelActions()
            {
                AddNewPhysicItem = () => ShowPhysicEditorControl(null),    //Add Item to PrototypControl
                EditPhysicItem = ShowPhysicEditorControl                   //Edit-Item from PrototypControl
            });

            ShowMainControl();
        }

        #region ITimerHandler
        public void HandleTimerTick(float dt)
        {
            if (this.ContentUserControl?.DataContext is ITimerHandler)
                (this.ContentUserControl.DataContext as ITimerHandler).HandleTimerTick(this.timerIntercallInMilliseconds);
        }
        #endregion

        #region IGraphicPanelHandler
        public void HandleSizeChanged(int width, int height)
        {
            if (this.ContentUserControl?.DataContext is IGraphicPanelHandler)
                (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleSizeChanged(panel.Width, panel.Height);
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
            if (this.ContentUserControl?.DataContext is IGraphicPanelHandler)
                (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleKeyDown(e);
        }
        public void HandleKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            if (this.ContentUserControl?.DataContext is IGraphicPanelHandler)
                (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleKeyUp(e);
        }
        #endregion

        #region IObjectSerializable
        public object GetExportObject()
        {
            return this.MainViewModel.GetExportObject();
        }
        public void LoadFromExportObject(object exportObject)
        {
            this.MainViewModel.LoadFromExportObject(exportObject);
        }
        #endregion

        #region IToTextWriteable
        public void WriteToTextFile(string filePath)
        {
            this.MainViewModel.WriteToTextFile(filePath);
        }
        public void LoadFromTextFile(string filePath)
        {
            this.MainViewModel.LoadFromTextFile(filePath);
        }
        #endregion

        #region ISimlatorUser
        public void SetSimulatorBuildMethod(CreateSimulatorFunction createSimulatorFunction)
        {
            this.MainViewModel.SetSimulatorBuildMethod(createSimulatorFunction);
        }
        #endregion

        private void ShowMainControl()
        {
            this.ContentUserControl = new EditorControl.EditorControl(this.MainViewModel, this.panel);
        }

        private void ShowPhysicEditorControl(IPrototypItem item)
        {
            this.ContentUserControl = CreatePhysicEditorControl(item);
        }

        //item = Dieses Item soll bearbeitet werden. Wenn hier null steht, dann wird ein neues Item erzeugt
        private UserControl CreatePhysicEditorControl(IPrototypItem item)
        {
            return new PhysicEditorFactory(() => this.MainViewModel.PrototypViewModel.CreateNewId(), true, true).CreateEditorControl(new EditorInputData()
            {
                Panel = this.panel,
                TimerTickRateInMs = this.timerIntercallInMilliseconds,
                ShowSaveLoadButtons = false,
                InputData = item?.EditorExportData,
                IsFinished = (sender) =>
                {
                    var createdLevelItem = (this.ContentUserControl.DataContext as IPrototypItemFactory).CreatePrototypItem();
                    if (createdLevelItem != null)
                    {
                        if (item == null)
                            this.MainViewModel.PrototypViewModel.AddItem(createdLevelItem); //Erzeuge neues Item
                        else
                        {
                            this.MainViewModel.UpdateAfterPrototypWasChanged(item, createdLevelItem);//Aktualisiere die Item-Daten
                        }

                    }
                    ShowMainControl();
                }
            });
        }
    }
}
