using EditorControl.Model.EditorJoint;
using EditorControl.Model.EditorShape;
using EditorControl.Model.Function;
using EditorControl.Model.ShapeExporter;
using GraphicPanels;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reactive;
using System.Windows.Input;
using ControlInterfaces;
using static EditorControl.ViewModel.ActionSelectViewModel;
using System.IO;
using EditorControl.View;
using EditorControl.Model.Function.Joints.DistanceJoint;
using EditorControl.Model.Function.Joints.RevoluteJoint;
using EditorControl.Model.Function.Joints;
using EditorControl.View.Joints;
using EditorControl.Model.Function.Joints.PrismaticJoint;
using EditorControl.Model.Function.Joints.WeldJoint;
using EditorControl.Model.Function.Joints.WheelJoint;

namespace EditorControl.ViewModel
{
    public class EditorViewModel : ReactiveObject, IGraphicPanelHandler, IShapeDataContainer, IActivateable
    {
        public ReactiveCommand<Unit, Unit> SwitchClick { get; private set; }
        [Reactive] public ActionSelectViewModel ActionSelectViewModel { get; set; } = new ActionSelectViewModel();
        [Reactive] public ShapePropertyViewModel ShapePropertyViewModel { get; set; } = new ShapePropertyViewModel();
        [Reactive] public string HelperTextHeadline { get; set; } = "";
        [Reactive] public string HelperTextFunctions { get; set; } = "";
        [Reactive] public System.Windows.Controls.UserControl EditPropertiesControl { get; set; } = null;
        public ReactiveCommand<Unit, Unit> SaveClick { get; private set; }
        public ReactiveCommand<Unit, Unit> LoadClick { get; private set; }
        public ReactiveCommand<Unit, Unit> ChangeBackgroundClick { get; private set; }
        public ReactiveCommand<Unit, Unit> SaveImageClick { get; private set; }

        public bool IsActivated { get; set; } = false;

        private GraphicPanel2D panel;
        private List<IEditorShape> shapes = new List<IEditorShape>();
        private List<IEditorJoint> joints = new List<IEditorJoint>();
        private IFunction activeFunction = null;
        private string backgroundImage = "#FFFFFF";

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
                saveFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    File.WriteAllText(saveFileDialog.FileName, this.GetShapeData());
            });
            this.LoadClick = ReactiveCommand.Create(() =>
            {
                System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    this.LoadShapeData(File.ReadAllText(openFileDialog.FileName));
                    ActionSelectViewModel_SelectedCommandChanged(Commands.Nothing);
                }
                    
            });
            this.ChangeBackgroundClick = ReactiveCommand.Create(() =>
            {
                System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
                openFileDialog.Filter = "png files (*.png)|*.png|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    this.backgroundImage = BackgroundImageHelper.ClampImageSize(openFileDialog.FileName, panel.Width, panel.Height, "EditorBackgroundImage.bmp");
            });
            this.SaveImageClick = ReactiveCommand.Create(() =>
            {
                System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                saveFileDialog.Filter = "bmp files (*.bmp)|*.bmp|All files (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    panel.GetScreenShoot().Save(saveFileDialog.FileName);
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

                case Commands.AddRevoluteJoint:
                    this.activeFunction = new AddRevoluteJointFunction().Init(this.shapes, this.joints);
                    break;

                case Commands.AddPrismaticJoint:
                    this.activeFunction = new AddPrismaticJointFunction().Init(this.shapes, this.joints);
                    break;

                case Commands.AddWeldJoint:
                    this.activeFunction = new AddWeldJointFunction().Init(this.shapes, this.joints);
                    break;

                case Commands.AddWheelJoint:
                    this.activeFunction = new AddWheelJointFunction().Init(this.shapes, this.joints);
                    break;

                case Commands.LimitJoint:
                    this.activeFunction = new SelectJointFunction().Init(this.joints, JointSelectedForEditLimitHandler);
                    break;

                case Commands.EditProperties:
                    this.activeFunction = new EditPropertiesFunction().Init(this.shapes, this.joints, ShapeSelectedForEditHandler, JointSelectedForEditPropertysHandler);
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

        private void JointSelectedForEditPropertysHandler(IEditorJoint joint)
        {
            if (joint != null)
            {
                if (joint is EditorDistanceJoint)
                {
                    this.EditPropertiesControl = new DistanceJointPropertyControl() { DataContext = (joint as EditorDistanceJoint).Properties };
                }

                if (joint is EditorRevoluteJoint)
                {
                    this.EditPropertiesControl = new RevoluteJointPropertyControl() { DataContext = (joint as EditorRevoluteJoint).Properties };
                }

                if (joint is EditorPrismaticJoint)
                {
                    this.EditPropertiesControl = new PrismaticJointPropertyControl() { DataContext = (joint as EditorPrismaticJoint).Properties };
                }

                if (joint is EditorWeldJoint)
                {
                    this.EditPropertiesControl = new WeldJointPropertyControl() { DataContext = (joint as EditorWeldJoint).Properties };
                }

                if (joint is EditorWheelJoint)
                {
                    this.EditPropertiesControl = new WheelJointPropertyControl() { DataContext = (joint as EditorWheelJoint).Properties };
                }
            }
            else
            {
                this.EditPropertiesControl = null;
            }
        }

        private void JointSelectedForEditLimitHandler(IEditorJoint joint)
        {
            if (this.activeFunction != null)
            {
                this.activeFunction.Dispose();
                this.activeFunction = null;
            }

            if (joint != null)
            {
                if (joint is EditorDistanceJoint)
                {
                    this.activeFunction = new LimitDistanceJointFunction().Init(joint as EditorDistanceJoint, SelectLimitJoint);
                }

                if (joint is EditorRevoluteJoint)
                {
                    this.activeFunction = new LimitRevoluteJointFunction().Init(joint as EditorRevoluteJoint, SelectLimitJoint);
                }

                if (joint is EditorPrismaticJoint)
                {
                    this.activeFunction = new LimitPrismaticJointFunction().Init(joint as EditorPrismaticJoint, SelectLimitJoint);
                }

                if (joint is EditorWheelJoint)
                {
                    this.activeFunction = new LimitWheelJointFunction().Init(joint as EditorWheelJoint, SelectLimitJoint);
                }
            }
        }

        private void SelectLimitJoint()
        {
            ActionSelectViewModel_SelectedCommandChanged(Commands.LimitJoint);
        }

        private void UpdateHelperText()
        {
            if (this.activeFunction != null)
            {
                this.HelperTextHeadline = this.activeFunction.GetHelpText().Headline;
                this.HelperTextFunctions = string.Join("\t", this.activeFunction.GetHelpText().Values);
            }
            else
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
            panel.ClearScreen(this.backgroundImage);
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
