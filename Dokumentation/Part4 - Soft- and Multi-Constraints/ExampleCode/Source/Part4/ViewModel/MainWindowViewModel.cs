using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Controls;
using Part4.View.Simulator;
using GraphicPanels;
using Part4.View.Editor;
using Part4.ViewModel.Simulator;
using Part4.ViewModel.Editor;
using static Part4.ViewModel.Editor.ActionSelectViewModel;
using System.Windows.Input;
using System.IO;

namespace Part4.ViewModel
{
    //Speichert das GraphicPanel2D-Objekt; Schaltet zwischen den Simulator und Editor um
    class MainWindowViewModel : ReactiveObject
    {
        private GraphicPanel2D panel;
        private UserControl simulatorControl;
        private UserControl editorControl;

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

        public SubControl SelectedControl
        {
            set
            {
                if (this.ContentUserControl != null)
                {
                    IActivateable bevore = (IActivateable)this.ContentUserControl.DataContext;
                    bevore.IsActivated = false;

                }

                switch (value)
                {
                    case SubControl.Simulator:
                        if (this.simulatorControl == null)
                        {
                            var vm = new SimulatorViewModel(this.panel);
                            vm.SwitchClick.Subscribe(value => { this.SelectedControl = SubControl.Editor; });
                            this.simulatorControl = new SimulatorControl(vm, this.panel);
                        }
                            

                        this.ContentUserControl = this.simulatorControl;

                        //Kopiere die Daten vom Editor zum Simulator beim Wechsel zum Simulator
                        IShapeDataContainer editor = (IShapeDataContainer)this.editorControl.DataContext;
                        IShapeDataContainer simulator = (IShapeDataContainer)this.simulatorControl.DataContext;
                        simulator.LoadShapeData(editor.GetShapeData());
                        break;

                    case SubControl.Editor:
                        if (this.editorControl == null)
                        {
                            var vm = new EditorViewModel(this.panel);
                            vm.SwitchClick.Subscribe(value => { this.SelectedControl = SubControl.Simulator; });
                            this.editorControl = new EditorControl(vm, this.panel);
                        }                            

                        this.ContentUserControl = this.editorControl;
                        break;
                }

                IActivateable after = (IActivateable)this.ContentUserControl.DataContext;
                after.IsActivated = true;
            }
        }
    }
}
