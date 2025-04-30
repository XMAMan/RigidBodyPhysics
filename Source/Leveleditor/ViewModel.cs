using GraphicPanels;
using GraphicPanelWpf;
using LevelEditorControl;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Windows;
using System.Windows.Input;
using WpfControls.Model;

namespace Leveleditor
{
    class ViewModel : ReactiveObject
    {
        private GraphicPanel2D panel;
        private System.Windows.Threading.DispatcherTimer timer;

        [Reactive] public System.Windows.Controls.UserControl ContentUserControl { get; set; }

        public ViewModel()
        {
            this.panel = new GraphicPanel2D() { Width = 100, Height = 100, Mode = Mode2D.OpenGL_Version_3_0 };

            this.panel.MouseClick += Panel_MouseClick;
            this.panel.MouseWheel += Panel_MouseWheel;
            this.panel.MouseMove += Panel_MouseMove;
            this.panel.MouseDown += Panel_MouseDown;
            this.panel.MouseUp += Panel_MouseUp;
            this.panel.SizeChanged += Panel_SizeChanged;
            this.panel.MouseEnter += Panel_MouseEnter;
            this.panel.MouseLeave += Panel_MouseLeave;

            this.timer = new System.Windows.Threading.DispatcherTimer();
            this.timer.Interval = new TimeSpan(0, 0, 0, 0, 30);//30 ms
            this.timer.Tick += Timer_Tick;
            this.timer.Start();

            try
            {
                this.ContentUserControl = new LevelEditorFactory().CreateEditorControl(new EditorInputData()
                {
                    ShowSaveLoadButtons = true,
                    ShowGoBackButton = false,
                    Panel = this.panel,
                    TimerTickRateInMs = (float)this.timer.Interval.TotalMilliseconds,
                    DataFolder = ""
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (this.ContentUserControl?.DataContext is ITimerHandler)
                    (this.ContentUserControl.DataContext as ITimerHandler).HandleTimerTick((float)timer.Interval.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                this.timer.Stop();
                MessageBox.Show(ex.ToString());
            }

        }
        private void Panel_SizeChanged(object? sender, EventArgs e)
        {
            try
            {
                if (this.ContentUserControl?.DataContext is ISizeChangeable)
                    (this.ContentUserControl.DataContext as ISizeChangeable).HandleSizeChanged(panel.Width, panel.Height);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void Panel_MouseClick(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            try
            {
                if (this.ContentUserControl?.DataContext is IGraphicPanelHandler)
                    (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleMouseClick(e);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Panel_MouseWheel(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            try
            {
                if (this.ContentUserControl?.DataContext is IGraphicPanelHandler)
                    (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleMouseWheel(e);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Panel_MouseMove(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            try
            {
                if (this.ContentUserControl?.DataContext is IGraphicPanelHandler)
                    (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleMouseMove(e);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Panel_MouseDown(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            try
            {
                if (this.ContentUserControl?.DataContext is IGraphicPanelHandler)
                    (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleMouseDown(e);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void Panel_MouseUp(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            try
            {
                if (this.ContentUserControl?.DataContext is IGraphicPanelHandler)
                    (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleMouseUp(e);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Panel_MouseEnter(object? sender, EventArgs e)
        {
            try
            {
                if (this.ContentUserControl?.DataContext is IGraphicPanelHandler)
                    (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleMouseEnter();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void Panel_MouseLeave(object? sender, EventArgs e)
        {
            try
            {
                if (this.ContentUserControl?.DataContext is IGraphicPanelHandler)
                    (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleMouseLeave();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void HandleKeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsRepeat) return; //So verhindere ich, dass bei gedrückter Taste der Handler mehrmals aufgerufen wird

            try
            {
                if (this.ContentUserControl?.DataContext is IKeyDownUpHandler)
                    (this.ContentUserControl.DataContext as IKeyDownUpHandler).HandleKeyDown(e);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void HandleKeyUp(object sender, KeyEventArgs e)
        {
            if (e.IsRepeat) return;

            try
            {
                if (this.ContentUserControl?.DataContext is IKeyDownUpHandler)
                    (this.ContentUserControl.DataContext as IKeyDownUpHandler).HandleKeyUp(e);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
