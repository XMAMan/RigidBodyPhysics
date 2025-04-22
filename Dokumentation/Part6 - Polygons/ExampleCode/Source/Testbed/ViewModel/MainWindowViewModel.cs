using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Windows.Controls;
using GraphicPanels;
using System.Windows.Input;
using GraphicPanelWpf;
using SimulatorControl;
using WpfControls.Model;
using EditorControl;

namespace Testbed.ViewModel
{
    //Speichert das GraphicPanel2D-Objekt; Schaltet zwischen den Simulator und Editor um
    class MainWindowViewModel : ReactiveObject
    {
        private GraphicPanel2D panel;
        private UserControl simulatorControl;
        private UserControl editorControl;
        private System.Windows.Threading.DispatcherTimer timer;

        public enum SubControl { Simulator, Editor}

        [Reactive] public UserControl ContentUserControl { get; set; }

        public MainWindowViewModel()
        {
            this.panel = new GraphicPanel2D() { Width = 100, Height = 100, Mode = Mode2D.OpenGL_Version_3_0 };

            this.SelectedControl = SubControl.Editor;

            this.panel.MouseClick += Panel_MouseClick;
            this.panel.MouseWheel += Panel_MouseWheel;
            this.panel.MouseMove += Panel_MouseMove;
            this.panel.MouseDown += Panel_MouseDown;
            this.panel.MouseUp += Panel_MouseUp;
            this.panel.SizeChanged += Panel_SizeChanged;

            this.timer = new System.Windows.Threading.DispatcherTimer();
            this.timer.Interval = new TimeSpan(0, 0, 0, 0, 50);//50 ms
            this.timer.Tick += Timer_Tick;
            this.timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (this.ContentUserControl.DataContext is ITimerHandler)
                (this.ContentUserControl.DataContext as ITimerHandler).HandleTimerTick((float)timer.Interval.TotalMilliseconds);
        }

        private void Panel_MouseClick(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleMouseClick(e);
        }

        private void Panel_MouseWheel(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleMouseWheel(e);
        }

        private void Panel_MouseMove(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleMouseMove(e);
        }

        private void Panel_MouseDown(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleMouseDown(e);
        }
        private void Panel_MouseUp(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleMouseUp(e);
        }

        public void HandleKeyDown(object sender, KeyEventArgs e)
        {
            (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleKeyDown(e);
        }

        public void HandleKeyUp(object sender, KeyEventArgs e)
        {
            (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleKeyUp(e);
        }

        private void Panel_SizeChanged(object? sender, EventArgs e)
        {
            (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleSizeChanged(panel.Width, panel.Height);
        }

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
                                TimerTickRateInMs = (float)timer.Interval.TotalMilliseconds,
                                IsFinished = () => this.SelectedControl = SubControl.Editor,
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
                                IsFinished = () => this.SelectedControl = SubControl.Simulator,
                            });
                        }                            

                        this.ContentUserControl = this.editorControl;
                        break;
                }
            }
        }
    }
}
