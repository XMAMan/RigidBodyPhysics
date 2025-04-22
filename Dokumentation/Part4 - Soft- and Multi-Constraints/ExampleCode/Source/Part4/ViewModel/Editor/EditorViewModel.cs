using GraphicPanels;
using Part4.Model.Editor.EditorJoint;
using Part4.Model.Editor.EditorShape;
using Part4.Model.Editor.Function;
using Part4.Model.ShapeExporter;
using Part4.View.Editor;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reactive;
using System.Windows.Controls;
using System.Windows.Input;
using static Part4.ViewModel.Editor.ActionSelectViewModel;

namespace Part4.ViewModel.Editor
{
    public class EditorViewModel : ReactiveObject, IGraphicPanelHandler, IShapeDataContainer, IActivateable
    {
        public ReactiveCommand<Unit, Unit> SwitchClick { get; private set; }
        [Reactive] public ActionSelectViewModel ActionSelectViewModel { get; set; } = new ActionSelectViewModel();
        [Reactive] public ShapePropertyViewModel ShapePropertyViewModel { get; set; } = new ShapePropertyViewModel();
        [Reactive] public string HelperTextHeadline { get; set; } = "";
        [Reactive] public string HelperTextFunctions { get; set; } = "";
        [Reactive] public UserControl EditPropertiesControl { get; set; } = null;
        public ReactiveCommand<Unit, Unit> SaveClick { get; private set; }
        public ReactiveCommand<Unit, Unit> LoadClick { get; private set; }

        public bool IsActivated { get; set; } = false;

        private GraphicPanel2D panel;
        private List<IEditorShape> shapes = new List<IEditorShape>();
        private List<IEditorJoint> joints = new List<IEditorJoint>();
        private IFunction activeFunction = null;

        public EditorViewModel(GraphicPanel2D panel)
        {
            this.panel = panel;

            this.SwitchClick = ReactiveCommand.Create(() =>
            {
                //Deaktiviere die ausgewählte Aktion             
                this.ActionSelectViewModel.SelectedCommand = Commands.Nothing;
            });

            this.SaveClick = ReactiveCommand.Create(() =>
            {
                System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    File.WriteAllText(saveFileDialog.FileName, this.GetShapeData());
            });
            this.LoadClick = ReactiveCommand.Create(() =>
            {
                System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    this.LoadShapeData(File.ReadAllText(openFileDialog.FileName));
            });

            this.ActionSelectViewModel.SelectedCommandChanged += ActionSelectViewModel_SelectedCommandChanged;

            RefreshPanel();
        }

        private void ActionSelectViewModel_SelectedCommandChanged(Commands command)
        {
            if (this.activeFunction != null)
            {
                this.activeFunction.Dispose();
                this.activeFunction = null;
            }

            switch (command)
            {
                case Commands.AddRectangle:
                    this.activeFunction = new AddRectangleFunction().Init(this.shapes, this.joints);
                    break;

                case Commands.AddCircle:
                    this.activeFunction = new AddCircleFunction().Init(this.shapes, this.joints);
                    break;

                case Commands.MoveRotate:
                    this.activeFunction = new MoveRotateFunction().Init(this.shapes, this.joints);
                    break;

                case Commands.MoveResize:
                    this.activeFunction = new MoveResizeFunction().Init(this.shapes, this.joints);
                    break;

                case Commands.CloneShape:
                    this.activeFunction = new CloneShapeFunction().Init(this.shapes, this.joints);
                    break;

                case Commands.AddDistanceJoint:
                    this.activeFunction = new AddDistanceJointFunction().Init(this.shapes, this.joints);
                    break;

                case Commands.EditProperties:
                    this.activeFunction = new EditPropertiesFunction().Init(this.shapes, this.joints, ShapeSelectedForEditHandler, JointSelectedForEditHandler);
                    break;

                case Commands.Delete:
                    this.activeFunction = new DeleteFunction().Init(this.shapes, this.joints);
                    break;

                case Commands.Nothing:                    
                    
                    break;
            }

            UpdateHelperText();
        }

        private void ShapeSelectedForEditHandler(IEditorShape shape)
        {
            if (shape != null)
            {
                this.EditPropertiesControl = new ShapePropertyControl() { DataContext = shape.Properties };
            }
            else
            {
                this.EditPropertiesControl = null;
            }
        }

        private void JointSelectedForEditHandler(IEditorJoint joint)
        {
            if (joint != null)
            {
                if (joint is EditorDistanceJoint)
                {
                    this.EditPropertiesControl = new DistanceJointPropertyControl() { DataContext = (joint as EditorDistanceJoint).Properties };
                }
            }else
            {
                this.EditPropertiesControl = null;
            }
        }

        private void UpdateHelperText()
        {
            if (this.activeFunction != null)
            {
                this.HelperTextHeadline = this.activeFunction.GetHelpText().Headline;
                this.HelperTextFunctions = string.Join("\t", this.activeFunction.GetHelpText().Values);
            }else
            {
                this.HelperTextHeadline = "";
                this.HelperTextFunctions = "";
            }
        }

        public void HandleMouseClick(System.Windows.Forms.MouseEventArgs e)
        {
            this.activeFunction?.HandleMouseClick(e);

            UpdateHelperText();
        }

        public void HandleMouseWheel(System.Windows.Forms.MouseEventArgs e)
        {
            this.activeFunction?.HandleMouseWheel(e);

            RefreshPanel();
        }

        public void HandleMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            this.activeFunction?.HandleMouseMove(e);

            RefreshPanel();
        }

        public void HandleMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            this.activeFunction?.HandleMouseDown(e);
        }
        public void HandleMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            this.activeFunction?.HandleMouseUp(e);
        }

        public void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.ActionSelectViewModel.SelectedCommand = Commands.Nothing;
            else
                this.activeFunction?.HandleKeyDown(e);

            RefreshPanel();
            UpdateHelperText();
        }

        public void HandleKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            this.activeFunction?.HandleKeyUp(e);
        }

        public void RefreshPanel()
        {
            panel.ClearScreen(Color.White);
            foreach (IEditorShape shape in this.shapes) { shape.Draw(panel); }
            foreach (IEditorJoint joint in this.joints) { joint.Draw(panel); }
            this.activeFunction?.Draw(panel);
            panel.FlipBuffer();
        }

        public string GetShapeData()
        {
            return ExportHelper.ToJson(this.shapes, this.joints);
        }

        public void LoadShapeData(string json)
        {
            ExportHelper.JsonToEditorData(json, out this.shapes, out this.joints);
        }
    }
}
