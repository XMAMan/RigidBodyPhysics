using GraphicMinimal;
using GraphicPanels;
using GraphicPanelWpf;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Windows.Forms;
using TextureEditorControl.Controls.DrawingSettings;
using TextureEditorControl.Controls.Editor.Model.Shape;
using TextureEditorControl.Controls.Editor.Model;
using TextureEditorControl.Controls.TextureData;
using TextureEditorGlobal;
using WpfControls.Controls.CameraSetting;
using WpfControls.Model;
using System.IO;
using DynamicData.Binding;
using DynamicData;
using System.Drawing;
using Local = TextureEditorControl;

namespace TextureEditorControl.Controls.Editor
{
    public class TextureEditorViewModel : ReactiveObject, IGraphicPanelHandler, IStringSerializable, IObjectSerializable, IPhysicSceneEditor
    {
        private GraphicPanel2D panel;
        private VisualisizerInputData inputData = null;
        private ShapeContainer shapeContainer;
        private Drawer drawer = null;
        private TextureClickPoint selectedTexturePoint = null;
        private TextureClickPoint mouseOverPoint = null;
        private Vector2D mousePosition;
        private bool shiftIsPressed = false;

        public ReactiveCommand<Unit, Unit> ImportPhysicSceneClick { get; private set; }
        public ReactiveCommand<Unit, Unit> SaveClick { get; private set; } //Texturdatei speichern
        public ReactiveCommand<Unit, Unit> LoadClick { get; private set; } //Texturdatei laden

        [Reactive] public bool IsLoaded { get; private set; } = false;
        [Reactive] public CameraSettingViewModel CameraViewModel { get; private set; }
        [Reactive] public TextureDataViewModel ShapeViewModel { get; private set; } = null;
        [Reactive] public DrawingSettingsViewModel DrawingSettings { get; private set; } = new DrawingSettingsViewModel();
        [Reactive] public System.Windows.Input.Cursor DrawingPanelCursor { get; private set; }
        [Reactive] public bool ShowSaveLoadButtons { get; private set; }

        public ObservableCollection<string> AreaShapeNames { get; private set; } = new ObservableCollection<string>();
        [Reactive] public string SelectedAreaShapeName { get; set; }

        public string PhysicSceneJson { get; private set; } //Wird für den PhysicEngine-Editor benötigt damit er sieht, ob es Änderungen in der PhysicScene gab

        public TextureEditorViewModel(GraphicPanel2D panel, bool showSaveLoadButtons, string physicSceneJson)
        {
            this.panel = panel;
            this.ShowSaveLoadButtons = showSaveLoadButtons;

            this.ImportPhysicSceneClick = ReactiveCommand.Create(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ImportPhysicSceneFromJson(File.ReadAllText(openFileDialog.FileName));
                }
            });
            this.SaveClick = ReactiveCommand.Create(() =>
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    File.WriteAllText(saveFileDialog.FileName, GetExportData());

            },
            this.WhenAnyValue(x => x.IsLoaded) //CanExecute: Zeige den Savebutton nur, wenn IsLoaded true ist
            );
            this.LoadClick = ReactiveCommand.Create(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                    LoadExportData(File.ReadAllText(openFileDialog.FileName));
            },
            this.WhenAnyValue(x => x.IsLoaded) //CanExecute: Zeige den Loadbutton nur, wenn IsLoaded true ist
            );

            this.DrawingSettings.WhenAnyPropertyChanged().Subscribe(x =>
            {
                Refresh();
            });

            this.panel.CreateOrUpdateNamedBitmapTexture("Cursor", Local.Properties.Resources.Cursor);
            this.WhenAnyValue(x => x.SelectedAreaShapeName).Subscribe(x =>
            {
                int index = this.AreaShapeNames.IndexOf(x);
                if (index != -1 && index < this.shapeContainer.Shapes.Length)
                {
                    var shape = this.shapeContainer.Shapes[index];
                    this.ShapeViewModel = shape.Propertys;
                    this.shapeContainer.SelectShape(shape);
                    Refresh();
                }
            });

            Refresh();

            if (string.IsNullOrEmpty(physicSceneJson) == false)
                ImportPhysicSceneFromJson(physicSceneJson);
        }

        private string GetExportData()
        {
            return JsonHelper.Helper.ToJson(this.shapeContainer.GetExportData());
        }

        private void LoadExportData(string visualisizerOutputDataJson)
        {
            var data = JsonHelper.Helper.CreateFromJson<VisualisizerOutputData>(visualisizerOutputDataJson);
            this.shapeContainer.LoadExportData(data);
        }

        private void ImportPhysicSceneFromJson(string physicSceneJson)
        {
            this.PhysicSceneJson = physicSceneJson;
            var importer = ImporterBuilder.BuildPhysicImporter(physicSceneJson);
            this.inputData = importer.Import();
            this.shapeContainer = new ShapeContainer(this.inputData);
            this.drawer = new Drawer(this.shapeContainer.Shapes);
            this.CameraViewModel = new CameraSettingViewModel(panel.Width, panel.Height, this.shapeContainer.BoundingBox);
            this.CameraViewModel.CameraPositionChanged += () => { Refresh(); };
            foreach (var shape in this.shapeContainer.Shapes)
            {
                shape.Propertys.WhenAnyPropertyChanged().Subscribe(x =>
                {
                    Refresh();
                });
            }
            this.AreaShapeNames.Clear();
            this.AreaShapeNames.AddRange(this.shapeContainer.GetAllShapeNames());
            this.AreaShapeNames.Add("");
            this.SelectedAreaShapeName = "";
            Refresh();
            this.IsLoaded = true;
        }

        private void Refresh()
        {
            if (this.drawer != null)
            {
                this.drawer.Draw(this.panel, this.CameraViewModel.Camera, this.DrawingSettings);

                var clickPoint = this.mouseOverPoint != null ? this.mouseOverPoint : this.selectedTexturePoint;
                if (clickPoint != null && this.mousePosition != null)
                {
                    clickPoint.DrawClickPoint(panel, this.mousePosition, this.shiftIsPressed == false);
                }
                else if (this.mousePosition != null)
                {
                    panel.DrawFillRectangle("Cursor", mousePosition.Xi, mousePosition.Yi, 20, 30, true, Color.White);
                }
            }
            else
            {
                panel.ClearScreen(Color.White);

                if (this.mousePosition != null)
                    panel.DrawFillRectangle("Cursor", mousePosition.Xi, mousePosition.Yi, 20, 30, true, Color.White);

                panel.FlipBuffer();
            }

        }

        #region IGraphicPanelHandler
        public void HandleMouseClick(System.Windows.Forms.MouseEventArgs e)
        {
        }
        public void HandleMouseWheel(System.Windows.Forms.MouseEventArgs e)
        {
        }
        public void HandleMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            this.mousePosition = new Vector2D(e.X, e.Y);
            Refresh();

            if (this.CameraViewModel == null) return;

            var localPoint = this.CameraViewModel.Camera.PointToCamera(new System.Drawing.PointF(e.X, e.Y)).ToGrx();

            //Schritt 1: Mauszeiger verändern, wenn er über ein Click-Point von ein TexturBorder ist
            this.mouseOverPoint = this.shapeContainer.GetClickPointFromSelectedShape(localPoint);
            if (this.mouseOverPoint != null)
                this.DrawingPanelCursor = System.Windows.Input.Cursors.None;
            else
                this.DrawingPanelCursor = System.Windows.Input.Cursors.Arrow;


            //Schritt 2: TexturBorder verändern, wenn ein TexturClick-Point festgehalten wird
            if (this.selectedTexturePoint != null)
            {
                this.selectedTexturePoint.MoveClickPointToPosition(localPoint, this.shiftIsPressed == false);
            }
        }

        public void HandleMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            if (this.CameraViewModel == null) return;

            var localPoint = this.CameraViewModel.Camera.PointToCamera(new System.Drawing.PointF(e.X, e.Y)).ToGrx();

            if (e.Button == MouseButtons.Left)
            {
                //Schritt 1: Prüfe, ob ein TextureBorder-Clickpoint angeklickt wurde
                this.selectedTexturePoint = this.shapeContainer.GetClickPointFromSelectedShape(localPoint);

                //Es wurde vom bereits aktiven Shape auf ein ClickPoint geklickt. Versuche keine neue Shape zu selektiren
                if (this.selectedTexturePoint != null)
                    return;

                //Schritt 2: Prüfe ob ein PhysikModel angeklickt wurde
                var shape = this.shapeContainer.GetShapeFromClickPosition(localPoint, this.DrawingSettings);
                this.shapeContainer.SelectShape(shape);
                if (shape != null)
                {
                    this.ShapeViewModel = shape.Propertys;
                    this.SelectedAreaShapeName = this.shapeContainer.GetShapeName(shape);
                }
                else
                {
                    this.ShapeViewModel = null;
                    this.SelectedAreaShapeName = "";
                }

                Refresh();
            }
        }



        public void HandleMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            this.selectedTexturePoint = null;
        }
        public void HandleMouseEnter()
        {
            System.Windows.Forms.Cursor.Hide();
        }
        public void HandleMouseLeave()
        {
            System.Windows.Forms.Cursor.Show();
            this.mousePosition = null;
            Refresh();
        }
        public void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.LeftShift)
                this.shiftIsPressed = true;

            Refresh();
        }
        public void HandleKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.LeftShift)
                this.shiftIsPressed = false;

            Refresh();
        }
        public void HandleSizeChanged(int width, int height)
        {
            Refresh();
        }
        #endregion

        #region IStringSerializable
        public object GetExportObject()
        {
            return this.shapeContainer.GetExportData();
        }
        public void LoadFromExportObject(object exportObject)
        {
            this.shapeContainer.LoadExportData((VisualisizerOutputData)exportObject);
        }

        public string GetExportString()
        {
            return GetExportData();
        }
        public void LoadFromExportString(string exportString)
        {
            if (string.IsNullOrEmpty(exportString)) return; //Ohne ein String kann hier nichts geladen werden

            LoadExportData(exportString);
        }
        #endregion
    }
}
