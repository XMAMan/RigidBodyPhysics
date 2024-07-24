using GraphicPanels;
using GraphicPanelWpf;
using PhysicEngine.Tools;
using PowerArgs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SoundEngine;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WpfControls.Model;

namespace PhysicEngine
{
    internal class MainWindowViewModel : ReactiveObject, IDisposable
    {
        private GraphicPanel2D panel;
        private ISoundGenerator soundGenerator;
        private System.Windows.Threading.DispatcherTimer timer;

        [Reactive] public string Title { get; set; }
        [Reactive] public System.Windows.Controls.UserControl ContentUserControl { get; set; }

        public MainWindowViewModel()
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

            this.soundGenerator = new SoundGenerator();

            this.timer = new System.Windows.Threading.DispatcherTimer();
            this.timer.Interval = new TimeSpan(0, 0, 0, 0, 30);//30 ms
            this.timer.Tick += Timer_Tick;

            this.timer.Start();

            //-mode LevelEditor -dataFolder ..\..\..\..\..\Data\GameData\
            //-mode SpriteEditor -dataFolder ..\..\..\..\..\Data\GameData\
            //-mode PhysicSceneTestbed -dataFolder ..\..\..\..\..\Data\GameData\

            //-mode Moonlander -dataFolder ..\..\..\..\..\Data\GameData\Moonlander\
            //-mode SkiJumper -dataFolder ..\..\..\..\..\Data\GameData\SkiJumper\
            //-mode Elma -dataFolder ..\..\..\..\..\Data\GameData\Elma\
            //-mode Astroids -dataFolder ..\..\..\..\..\Data\GameData\Astroids\
            //-mode CarDrifter -dataFolder ..\..\..\..\..\Data\GameData\CarDrifter\
            //-mode BridgeBuilder -dataFolder ..\..\..\..\..\Data\GameData\BridgeBuilder\
            //-mode SpiderBox -dataFolder ..\..\..\..\..\Data\GameData\SpiderBox\

            //-mode CreateCleanBat -dataFolder ..\..\..\..\..\Source\ -outputFolder ..\..\..\..\..\Batch-Files\
            //-mode CreateCleanBat -dataFolder "..\..\..\..\..\Dokumentation\Part1 - Iterative Impulse\ExampleCode\Source" -outputFolder "..\..\..\..\..\Dokumentation\Part1 - Iterative Impulse\ExampleCode"
            //-mode CountLineOfCodes -dataFolder ..\..\..\..\..\Source\

            //ShowEditor(EditorType.LevelEditor, @"..\..\..\..\..\Data\GameData\"); return;
            //ShowEditor(EditorType.SpriteEditor, @"..\..\..\..\..\Data\GameData\"); return;

            try
            {
                var args = ParseCommandLineArguments();

                if (EditorFactory.IsHelperTool(args.Mode))
                {
                    RunHelperTool(args.Mode, args.DataFolder, args.OutputFolder);
                    System.Windows.Application.Current.Shutdown();
                }
                else
                {
                    ShowEditor(args.Mode, args.DataFolder);
                }                
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }         

            
        }

        private CommandLineArgs ParseCommandLineArguments()
        {
            
            string[] args = Environment.GetCommandLineArgs();
            //args = new string[] { "-mode", "Moonlander", "-dataFolder", "..\\..\\..\\..\\..\\..\\Data\\GameData\\Moonlander\\" };
            if (args != null)
            {
                if (args.Length > 1)
                {
                    args = args.ToList().GetRange(1, args.Length - 1).ToArray(); //Entferne den Name auf die eigene Exe-Datei
                }
                
                return Args.Parse<CommandLineArgs>(args);
            }
            else
            {
                return new CommandLineArgs()
                {
                    DataFolder = @"..\..\..\..\..\Data\GameData\",
                    Mode = EditorType.LevelEditor,
                };
            }
        }

        private void RunHelperTool(EditorType mode, string dataFolder, string outputFolder)
        {
            switch (mode)
            {
                case EditorType.CreateCleanBat:
                    CleanBatCreator.CreateCleanFile(dataFolder, outputFolder); return;

                case EditorType.CountLineOfCodes:
                    LineOfCodeCounter.CountLineOfCodes(dataFolder); return;
            }

            throw new NotImplementedException();
        }

        private void ShowEditor(EditorType editorType, string dataFolder)
        {
            this.Title = editorType.ToString();
            this.ContentUserControl = EditorFactory.CreateEditorControl(editorType,
                new EditorInputData()
                {
                    ShowSaveLoadButtons = true,
                    ShowGoBackButton = false,
                    Panel = this.panel,
                    SoundGenerator = this.soundGenerator,
                    TimerTickRateInMs = (float)this.timer.Interval.TotalMilliseconds,
                    DataFolder = dataFolder
                });
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (this.ContentUserControl?.DataContext is ITimerHandler)
                    (this.ContentUserControl.DataContext as ITimerHandler).HandleTimerTick((float)timer.Interval.TotalMilliseconds);
            }catch (Exception ex)
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
            }catch (Exception ex)
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

        public void Dispose()
        {
            try
            {
                this.soundGenerator.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }            
        }
    }
}
