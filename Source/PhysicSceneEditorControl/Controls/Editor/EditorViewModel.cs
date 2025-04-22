using GraphicPanels;
using GraphicPanelWpf;
using PhysicSceneEditorControl.Controls.ActionSelect;
using PhysicSceneEditorControl.Controls.PolygonProperty;
using PhysicSceneEditorControl.Controls.RectangleProperty;
using PhysicSceneEditorControl.Controls.RotaryMotorProperty;
using PhysicSceneEditorControl.Controls.ShapeProperty;
using PhysicSceneEditorControl.Controls.ThrusterProperty;
using PhysicSceneEditorControl.Dialogs.BackgroundImage;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using RigidBodyPhysics.ExportData;
using System.Reactive;
using static PhysicSceneEditorControl.Controls.ActionSelect.ActionSelectViewModel;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WpfControls.Controls.CollisionMatrix;
using WpfControls.Model;
using PhysicSceneEditorControl.Controls.Editor.Model.Function;
using System.IO;
using PhysicSceneEditorControl.Controls.Editor.Model.EditorShape;
using PhysicSceneEditorControl.Controls.Editor.Model.EditorJoint;
using PhysicSceneEditorControl.Controls.Editor.Model.EditorThruster;
using PhysicSceneEditorControl.Controls.Editor.Model.EditorRotaryMotor;
using PhysicSceneEditorControl.Controls.Editor.Model.ShapeExporter;
using PhysicSceneEditorControl.Controls.Editor.Model.Function.Shapes;
using PhysicSceneEditorControl.Controls.Editor.Model.Function.Joints.DistanceJoint;
using PhysicSceneEditorControl.Controls.Editor.Model.Function.Joints.RevoluteJoint;
using PhysicSceneEditorControl.Controls.Editor.Model.Function.Joints.PrismaticJoint;
using PhysicSceneEditorControl.Controls.Editor.Model.Function.Joints.WeldJoint;
using PhysicSceneEditorControl.Controls.Editor.Model.Function.Joints.WheelJoint;
using PhysicSceneEditorControl.Controls.JointPropertys.DistanceJoint;
using PhysicSceneEditorControl.Controls.JointPropertys.RevoluteJoint;
using PhysicSceneEditorControl.Controls.JointPropertys.PrismaticJoint;
using PhysicSceneEditorControl.Controls.JointPropertys.WeldJoint;
using PhysicSceneEditorControl.Controls.JointPropertys.WheelJoint;
using PhysicSceneEditorControl.Controls.Editor.Model.Function.Joints;
using PhysicSceneEditorControl.Controls.Editor.Model.EditorAxialFriction;
using PhysicSceneEditorControl.Controls.AxialFriction;
using PhysicSceneEditorControl.Controls.Editor.Model;
using JsonHelper;

namespace PhysicSceneEditorControl.Controls.Editor
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
        [Reactive] public bool ShowSwitchButton { get; private set; }

        [Reactive] public System.Windows.Media.ImageSource MouseGridImage { get; set; } = new BitmapImage(new Uri(FilePaths.Grid1, UriKind.Absolute));
        [Reactive] public bool ShowMouseGrid { get; private set; }
        public ReactiveCommand<Unit, Unit> ShowMouseGridClick { get; private set; }
        public uint GridSize { get => this.functionData.MouseGrid.Size; set { this.functionData.MouseGrid.Size = Math.Max(1, value); RefreshPanel(); } }
        public ReactiveCommand<Unit, Unit> CopyToClipboardClick { get; private set; }
        public ReactiveCommand<Unit, Unit> PasteFromClipboardClick { get; private set; }

        private GraphicPanel2D panel;
        private FunctionData functionData = new FunctionData();

        private IFunction activeFunction = null;
        private BackgroundImageViewModel backgroundImageViewModel = new BackgroundImageViewModel();
        private BackgroundImageWindow backgroundImageWindow = null;
        private CollisionMatrixViewModel collisionMatrixViewModel;


        public string PhysicSceneJson { get => this.GetExportString(); } //IPhysicSceneEditor

        public EditorViewModel(EditorInputData data)
        {
            this.panel = data.Panel;
            this.ShowSaveLoadButtons = data.ShowSaveLoadButtons;
            this.ShowSwitchButton = data.ShowGoBackButton;
            this.collisionMatrixViewModel = new CollisionMatrixViewModel(this.functionData.CollisionMatrix);

            this.SwitchClick = ReactiveCommand.Create(() =>
            {
                //Deaktiviere die ausgewählte Aktion             
                this.ActionSelectViewModel.SelectedCommand = Commands.Nothing;
                data.IsFinished?.Invoke(this);
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

            this.CopyToClipboardClick = ReactiveCommand.Create(() =>
            {
                string physicSceneExport = Helper.ToCompactJson(ExportHelper.ConvertFunctionDataToExportData(this.functionData));
                System.Windows.Clipboard.SetText(physicSceneExport);
            });

            this.PasteFromClipboardClick = ReactiveCommand.Create(() =>
            {
                string physicSceneExport = System.Windows.Clipboard.GetText();
                try
                {
                    this.functionData = ExportHelper.ConvertExportDataToFunctionData(Helper.FromCompactJson<PhysicSceneExportData>(physicSceneExport));
                    this.collisionMatrixViewModel.CollideMatrix = this.functionData.CollisionMatrix;
                }
                catch (Exception)
                {
                }
            });

            this.ActionSelectViewModel.SelectedCommandChanged += ActionSelectViewModel_SelectedCommandChanged;

            //Da das EditPropertiesControl so groß ist, werden alle Buttons ausgeblendet, wenn es aktiv ist, damit es in Fenster passt
            this.WhenAnyValue(x => x.EditPropertiesControl).Subscribe(vm => 
            {
                this.ActionSelectViewModel.VisibilityAll = this.EditPropertiesControl == null ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            });

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

                case Commands.AddAxialFriction:
                    this.activeFunction = new AddAxialFrictionFunction().Init(this.functionData);
                    break;

                case Commands.EditProperties:
                    this.activeFunction = new EditPropertiesFunction().Init(this.functionData, EditorObjectSelectedForEditPropertysHandler);
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

        private void EditorObjectSelectedForEditPropertysHandler(ISelectable obj)
        {
            if (obj != null)
            {
                if (obj is EditorCircle)
                {
                    this.EditPropertiesControl = new ShapePropertyControl() { DataContext = (obj as EditorCircle).Properties };
                }

                if (obj is EditorPolygon)
                {
                    this.EditPropertiesControl = new PolygonPropertyControl() { DataContext = (obj as EditorPolygon).Properties };
                }

                if (obj is EditorRectangle)
                {
                    this.EditPropertiesControl = new RectanglePropertyControl() { DataContext = (obj as EditorRectangle).Properties };
                }

                if (obj is EditorDistanceJoint)
                {
                    this.EditPropertiesControl = new DistanceJointPropertyControl() { DataContext = (obj as EditorDistanceJoint).Properties };
                }

                if (obj is EditorRevoluteJoint)
                {
                    this.EditPropertiesControl = new RevoluteJointPropertyControl() { DataContext = (obj as EditorRevoluteJoint).Properties };
                }

                if (obj is EditorPrismaticJoint)
                {
                    this.EditPropertiesControl = new PrismaticJointPropertyControl() { DataContext = (obj as EditorPrismaticJoint).Properties };
                }

                if (obj is EditorWeldJoint)
                {
                    this.EditPropertiesControl = new WeldJointPropertyControl() { DataContext = (obj as EditorWeldJoint).Properties };
                }

                if (obj is EditorWheelJoint)
                {
                    this.EditPropertiesControl = new WheelJointPropertyControl() { DataContext = (obj as EditorWheelJoint).Properties };
                }

                if (obj is EditorThruster)
                {
                    this.EditPropertiesControl = new ThrusterPropertyControl() { DataContext = (obj as EditorThruster).Properties };
                }

                if (obj is EditorRotaryMotor)
                {
                    this.EditPropertiesControl = new RotaryMotorPropertyControl() { DataContext = (obj as EditorRotaryMotor).Properties };
                }

                if (obj is EditorAxialFriction)
                {
                    this.EditPropertiesControl = new AxialFrictionPropertyControl() { DataContext = (obj as EditorAxialFriction).Properties };
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
                this.functionData.MouseGrid.Draw(panel, 1, new GraphicMinimal.Vector2D(0, 0));

            if (this.backgroundImageViewModel.Image != null)
            {
                var v = this.backgroundImageViewModel;
                panel.DrawFillRectangle("BackgroundImage", (int)(v.XPosition * panel.Width), (int)(v.YPosition * panel.Height), (int)(v.Image.Width * v.Zoom), (int)(v.Image.Height * v.Zoom), false, Color.White);
            }
            foreach (IEditorShape shape in this.functionData.Shapes) shape.Draw(panel);
            foreach (IEditorJoint joint in this.functionData.Joints) joint.Draw(panel);
            foreach (IEditorThruster thruster in this.functionData.Thrusters) thruster.Draw(panel);
            foreach (IEditorRotaryMotor motor in this.functionData.RotaryMotors) motor.Draw(panel);
            foreach (IEditorAxialFriction axialFriction in this.functionData.AxialFrictions) axialFriction.Draw(panel);
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
