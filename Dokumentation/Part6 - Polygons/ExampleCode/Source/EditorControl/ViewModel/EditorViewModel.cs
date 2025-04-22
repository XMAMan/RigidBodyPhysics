using EditorControl.Model.EditorJoint;
using EditorControl.Model.EditorShape;
using EditorControl.Model.Function;
using EditorControl.Model.ShapeExporter;
using GraphicPanels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
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
using EditorControl.Model.Function.Shapes;
using EditorControl.Model.EditorThruster;
using GraphicPanelWpf;
using WpfControls.Model;
using PhysicEngine.ExportData;
using EditorControl.Model.EditorRotaryMotor;
using System.Windows.Media.Imaging;
using WpfControls.ViewModel;
using WpfControls.View.CollisionMatrix;

namespace EditorControl.ViewModel
{
    public class EditorViewModel : ReactiveObject, IGraphicPanelHandler, IStringSerializable, IObjectSerializable, IPhysicSceneEditor
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
        [Reactive] public bool ShowSaveLoadButtons { get; private set; }

        [Reactive] public System.Windows.Media.ImageSource MouseGridImage { get; set; } = new BitmapImage(new Uri(FilePaths.Grid1, UriKind.Absolute));
        [Reactive] public bool ShowMouseGrid { get; private set; }
        public ReactiveCommand<Unit, Unit> ShowMouseGridClick { get; private set; }
        public uint GridSize { get => this.functionData.MouseGrid.Size; set { this.functionData.MouseGrid.Size = Math.Max(1, value); RefreshPanel(); } }


        private GraphicPanel2D panel;
        private FunctionData functionData = new FunctionData();

        private IFunction activeFunction = null;
        private BackgroundImageViewModel backgroundImageViewModel = new BackgroundImageViewModel();
        private BackgroundImageWindow backgroundImageWindow = null;
        private CollisionMatrixViewModel collisionMatrixViewModel;
        

        public string PhysicSceneJson { get => this.GetExportString(); } //IPhysicSceneEditor

        public EditorViewModel(GraphicPanel2D panel, bool showSaveLoadButtons)
        {
            this.panel = panel;
            this.ShowSaveLoadButtons = showSaveLoadButtons;
            this.collisionMatrixViewModel = new CollisionMatrixViewModel(this.functionData.CollisionMatrix);

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
                    File.WriteAllText(saveFileDialog.FileName, this.GetExportString());
            });
            this.LoadClick = ReactiveCommand.Create(() =>
            {
                System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    this.LoadFromExportString(File.ReadAllText(openFileDialog.FileName));
                    ActionSelectViewModel_SelectedCommandChanged(Commands.Nothing);
                }
                    
            });
            this.ChangeBackgroundClick = ReactiveCommand.Create(() =>
            {
                if (this.backgroundImageWindow != null)
                    this.backgroundImageWindow.Close();

                this.backgroundImageWindow = new BackgroundImageWindow() { DataContext = this.backgroundImageViewModel };
                this.backgroundImageWindow.Show();
            });
            this.backgroundImageViewModel.WhenAnyValue(x => x.Image).Subscribe(vm =>
            {
                if (this.backgroundImageViewModel.Image != null)
                {
                    this.panel.CreateOrUpdateNamedBitmapTexture("BackgroundImage", this.backgroundImageViewModel.Image);
                    RefreshPanel();
                }
            });
            this.backgroundImageViewModel.WhenAnyValue(y => y.XPosition, z => z.YPosition, w => w.Zoom).Subscribe(vm =>
            {
                RefreshPanel();
            });

            this.SaveImageClick = ReactiveCommand.Create(() =>
            {
                System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                saveFileDialog.Filter = "bmp files (*.bmp)|*.bmp|All files (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    panel.GetScreenShoot().Save(saveFileDialog.FileName);
            });

            this.ShowMouseGridClick = ReactiveCommand.Create(() =>
            {
                this.ShowMouseGrid = !ShowMouseGrid;
                this.functionData.MouseGrid.ShowGrid = this.ShowMouseGrid;
                this.MouseGridImage = this.ShowMouseGrid ? new BitmapImage(new Uri(FilePaths.Grid2, UriKind.Absolute)) : new BitmapImage(new Uri(FilePaths.Grid1, UriKind.Absolute));
                RefreshPanel();
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
            this.EditPropertiesControl = null;

            switch (command)
            {
                case Commands.AddRectangle:
                    this.activeFunction = new AddRectangleFunction().Init(this.functionData);
                    break;

                case Commands.AddCircle:
                    this.activeFunction = new AddCircleFunction().Init(this.functionData);
                    break;

                case Commands.AddPolygon:
                    this.activeFunction = new AddPolygonFunction().Init(this.functionData);
                    break;

                case Commands.MoveRotate:
                    this.activeFunction = new MoveRotateFunction().Init(this.functionData);
                    break;

                case Commands.MoveResize:
                    this.activeFunction = new MoveResizeFunction().Init(this.functionData);
                    break;

                case Commands.CloneShape:
                    this.activeFunction = new CloneShapeFunction().Init(this.functionData);
                    break;

                case Commands.AddDistanceJoint:
                    this.activeFunction = new AddDistanceJointFunction().Init(this.functionData);
                    break;

                case Commands.AddRevoluteJoint:
                    this.activeFunction = new AddRevoluteJointFunction().Init(this.functionData);
                    break;

                case Commands.AddPrismaticJoint:
                    this.activeFunction = new AddPrismaticJointFunction().Init(this.functionData);
                    break;

                case Commands.AddWeldJoint:
                    this.activeFunction = new AddWeldJointFunction().Init(this.functionData);
                    break;

                case Commands.AddWheelJoint:
                    this.activeFunction = new AddWheelJointFunction().Init(this.functionData);
                    break;

                case Commands.LimitJoint:
                    this.activeFunction = new SelectJointFunction().Init(this.functionData, JointSelectedForEditLimitHandler);
                    break;

                case Commands.AddThruster:
                    this.activeFunction = new AddThrusterFunction().Init(this.functionData);
                    break;

                case Commands.AddRotaryMotor:
                    this.activeFunction = new AddRotaryMotorFunction().Init(this.functionData);
                    break;

                case Commands.EditProperties:
                    this.activeFunction = new EditPropertiesFunction().Init(this.functionData, ShapeSelectedForEditHandler, JointSelectedForEditPropertysHandler, ThrusterSelectedForEditHandler, RotaryMotorSelectedForEditHandler);
                    break;

                case Commands.EditCollisionMatrix:
                    {
                        this.activeFunction = new DefineCollisionMatrixFunction().Init(this.functionData, this.collisionMatrixViewModel);
                        this.EditPropertiesControl = new CollisionMatrixControl() { DataContext = this.collisionMatrixViewModel };
                    }
                    break;

                case Commands.Delete:
                    this.activeFunction = new DeleteFunction().Init(this.functionData);
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
                if (shape.Properties is PolygonPropertyViewModel)
                {
                    this.EditPropertiesControl = new PolygonPropertyControl() { DataContext = shape.Properties };
                }
                else if (shape.Properties is RectanglePropertyViewModel)
                {
                    this.EditPropertiesControl = new RectanglePropertyControl() { DataContext = shape.Properties };
                }
                else
                {
                    this.EditPropertiesControl = new ShapePropertyControl() { DataContext = shape.Properties };
                }
                
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

        private void ThrusterSelectedForEditHandler(IEditorThruster thruster)
        {
            if (thruster != null)
            {
                this.EditPropertiesControl = new ThrusterPropertyControl() { DataContext = thruster.Properties };
            }
            else
            {
                this.EditPropertiesControl = null;
            }
        }

        private void RotaryMotorSelectedForEditHandler(IEditorRotaryMotor motor)
        {
            if (motor != null)
            {
                this.EditPropertiesControl = new RotaryMotorPropertyControl() { DataContext = motor.Properties };
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
        public void HandleMouseEnter() { }
        public void HandleMouseLeave() { }
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
            panel.ClearScreen(System.Drawing.Color.White);

            if (this.functionData.MouseGrid.ShowGrid)
                this.functionData.MouseGrid.Draw(panel, 1, new GraphicMinimal.Vector2D(0,0));

            if (this.backgroundImageViewModel.Image != null)
            {
                var v = this.backgroundImageViewModel;
                panel.DrawFillRectangle("BackgroundImage", (int)(v.XPosition * panel.Width), (int)(v.YPosition * panel.Height), (int)(v.Image.Width * v.Zoom), (int)(v.Image.Height * v.Zoom), false, Color.White);
            }
            foreach (IEditorShape shape in this.functionData.Shapes) shape.Draw(panel); 
            foreach (IEditorJoint joint in this.functionData.Joints) joint.Draw(panel); 
            foreach (IEditorThruster thruster in this.functionData.Thrusters) thruster.Draw(panel); 
            foreach (IEditorRotaryMotor motor in this.functionData.RotaryMotors) motor.Draw(panel);
            this.activeFunction?.Draw(panel);
            panel.FlipBuffer();
        }

        public object GetExportObject()
        {
            return ExportHelper.ConvertFunctionDataToExportData(this.functionData);
        }
        public void LoadFromExportObject(object exportObject)
        {
            this.functionData = ExportHelper.ConvertExportDataToFunctionData((PhysicSceneExportData)exportObject);
            this.collisionMatrixViewModel.CollideMatrix = this.functionData.CollisionMatrix;
        }

        public string GetExportString()
        {
            return ExportHelper.ToJson(this.functionData);
        }

        public void LoadFromExportString(string json)
        {
            this.functionData = ExportHelper.JsonToEditorData(json);
            this.collisionMatrixViewModel.CollideMatrix = this.functionData.CollisionMatrix;
        }        

        public void HandleSizeChanged(int width, int height)
        {
            RefreshPanel();
        }
    }
}
