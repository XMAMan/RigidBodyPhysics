using PhysicSceneEditorControl;
using PhysicSceneSimulatorControl;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Windows.Controls;
using GraphicPanels;
using WpfControls.Model;
using GraphicPanelWpf;

namespace PhysicSceneTestbedControl
{
    //Speichert das GraphicPanel2D-Objekt; Schaltet zwischen den Simulator und Editor um
    class PhysicSceneTestbedViewModel : ReactiveObject, IGraphicPanelHandler, ITimerHandler, IStringSerializable, IObjectSerializable
    {
        private GraphicPanel2D panel;
        private float timerIntercallInMilliseconds;
        private UserControl simulatorControl;
        private UserControl editorControl;
  
        public enum SubControl { Simulator, Editor }

        [Reactive] public UserControl ContentUserControl { get; set; }

        public PhysicSceneTestbedViewModel(GraphicPanel2D panel, float timerIntercallInMilliseconds)
        {
            this.panel = panel;
            this.timerIntercallInMilliseconds = timerIntercallInMilliseconds;
            this.SelectedControl = SubControl.Editor;
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
            return (this.ContentUserControl.DataContext as IObjectSerializable).GetExportObject();
        }
        public void LoadFromExportObject(object exportObject)
        {
            (this.ContentUserControl.DataContext as IObjectSerializable).LoadFromExportObject(exportObject);
        }
        #endregion

        #region IStringSerializable
        public string GetExportString()
        {
            return (this.ContentUserControl.DataContext as IStringSerializable).GetExportString();
        }
        public void LoadFromExportString(string exportString)
        {
            (this.ContentUserControl.DataContext as IStringSerializable).LoadFromExportString(exportString);
        }
        #endregion

        public SubControl SelectedControl
        {
            set
            {
                switch (value)
                {
                    case SubControl.Simulator:
                        if (this.simulatorControl == null)
                        {
                            this.simulatorControl = new PhysicSceneSimulatorEditorFactory().CreateEditorControl(new WpfControls.Model.EditorInputData()
                            {
                                Panel = this.panel,
                                TimerTickRateInMs = this.timerIntercallInMilliseconds,
                                IsFinished = (sender) => this.SelectedControl = SubControl.Editor,
                            });
                        }


                        this.ContentUserControl = this.simulatorControl;

                        //Kopiere die Daten vom Editor zum Simulator beim Wechsel zum Simulator
                        IStringSerializable editor = (IStringSerializable)this.editorControl.DataContext;
                        IStringSerializable simulator = (IStringSerializable)this.simulatorControl.DataContext;
                        simulator.LoadFromExportString(editor.GetExportString());
                        break;

                    case SubControl.Editor:
                        if (this.editorControl == null)
                        {
                            this.editorControl = new PhysicSceneEditorFactory().CreateEditorControl(new WpfControls.Model.EditorInputData()
                            {
                                Panel = this.panel,
                                IsFinished = (sender) => this.SelectedControl = SubControl.Simulator,
                            });
                        }

                        this.ContentUserControl = this.editorControl;
                        break;
                }
            }
        }
    }
}
