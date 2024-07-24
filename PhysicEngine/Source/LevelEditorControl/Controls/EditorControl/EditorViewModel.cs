using GraphicPanelWpf;
using LevelEditorControl.Controls.ForcePlotterControl;
using LevelEditorControl.Controls.PolygonControl;
using LevelEditorControl.Controls.PrototypControl;
using LevelEditorControl.Controls.SelectedItemControl;
using LevelEditorControl.EditorFunctions;
using LevelEditorControl.LevelItems.BackgroundItem;
using LevelEditorControl.LevelItems.GroupedItems;
using LevelEditorControl.LevelItems.LawnEdge;
using LevelEditorControl.LevelItems.PhysicItem;
using LevelEditorControl.LevelItems.Polygon;
using LevelEditorControl.LevelItems;
using LevelEditorGlobal;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reactive;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using WpfControls.Controls.CameraSetting;
using WpfControls.Controls.CollisionMatrix;
using WpfControls.Model;
using LevelEditorControl.Controls.SimulatorControl;
using LevelEditorControl.Controls.BackgroundItemControl;
using System.Reactive.Linq;
using DynamicData;
using System.Windows.Forms;
using System;
using System.Linq;
using System.Collections.Generic;
using LevelEditorControl.Controls.TreeControl;
using LevelEditorControl.Controls.TagItemControl;
using System.IO;
using Simulator;

namespace LevelEditorControl.Controls.EditorControl
{
    internal class EditorViewModelActions
    {
        public Action AddNewPhysicItem;
        public Action<IPrototypItem> EditPhysicItem;
    }

    //Erzeugt aus all den Sub-Viewmodels ein EditorState-Objekt, was er dann der jeweils aktiven IEditorFunction bereit stellt
    internal class EditorViewModel : ReactiveObject, ITimerHandler, IGraphicPanelHandler, IObjectSerializable, IToTextWriteable, ISimlatorUser
    {
        public ReactiveCommand<Unit, Unit> SaveClick { get; private set; } //Level speichern
        public ReactiveCommand<Unit, Unit> LoadClick { get; private set; } //Level laden
        public ReactiveCommand<Unit, Unit> GoBackClick { get; private set; } //Zurück zum Spiel

        public ReactiveCommand<Unit, Unit> GridModeClick { get; private set; } //Gitter im Hintergrund anzeigen
        public ReactiveCommand<Unit, Unit> SmallWindowClick { get; private set; } //Kleines Vorschaubild rechts unten anzeigen

        [Reactive] public FunctionType CurrentState { get; private set; } = FunctionType.Nothing;

        public PrototypViewModel PrototypViewModel { get; }
        public SelectedItemViewModel SelectedItemViewModel { get; }
        public CameraSettingViewModel CameraSettingViewModel { get; private set; }
        public SimulatorViewModel SimulatorViewModel { get; private set; }
        public ForcePlotterViewModel ForcePlotterViewModel { get; private set; }
        public PolygonControlViewModel PolygonControlViewModel { get; private set; }
        [Reactive] public System.Windows.Controls.UserControl PropertyControl { get; set; } //Hier wird das hier angezeigt: IEditorFunction.GetPropertyControl()

        [Reactive] public ImageSource ShowSideBarImage { get; set; } = new BitmapImage(new Uri(FilePaths.DoubleDown, UriKind.Absolute));
        public ReactiveCommand<Unit, Unit> ShowSideBarClick { get; private set; }
        [Reactive] public bool ShowSideBar { get; private set; } = false;

        [Reactive] public bool ShowSaveLoadButtons { get; private set; }
        [Reactive] public bool ShowGoBackButton { get; private set; }

        public CollisionMatrixViewModel CollisionMatrixViewModel { get; private set; }
        public TreeViewModel LevelTreeViewModel { get; }
        public TagItemViewModel TagItemViewModel { get; }
        
        public ReactiveCommand<Unit, Unit> DefineCollisionMatrixClick { get; private set; }


        private EditorState editorData = new EditorState(); //Diese Variable wird der jeweils aktiven IEditorFunction-Funktion reingegeben 
        private IEditorFunction function = null; //State-Designpattern für das GraphicPanel2D
        private string dataFolder;


        public EditorViewModel(EditorInputData inputData, EditorViewModelActions actions)
        {
            this.editorData.Panel = inputData.Panel;
            this.editorData.TimerIntervallInMilliseconds = inputData.TimerTickRateInMs;
            this.dataFolder = inputData.DataFolder;
            this.ShowSaveLoadButtons = inputData.ShowSaveLoadButtons;
            this.ShowGoBackButton = inputData.ShowGoBackButton;

            this.PrototypViewModel = new PrototypViewModel(this.editorData,
                new PrototypControlActions()
                {
                    AddPhysicItem = actions.AddNewPhysicItem,
                    EditItemAction = (item) =>
                    {
                        switch (item.ProtoType)
                        {
                            case IPrototypItem.Type.PhysicItem:
                                UseFunction(FunctionType.MoveSelect); //Diese Funktion wird angezeigt, wenn das Editionsfenster fertig ist
                                actions.EditPhysicItem(item);
                                break;

                            case IPrototypItem.Type.BackgroundItem:
                                UseFunction(FunctionType.Nothing);
                                var vm = new BackgroundItemViewModel(item as BackgroundPrototypItem);
                                this.PropertyControl = new BackgroundItemControl.BackgroundItemControl() { DataContext = vm };
                                break;

                            case IPrototypItem.Type.GroupedItem:
                                UseFunction(FunctionType.MoveSelect); //Das GroupedItem kann man nicht editieren. Also gehe in die Default-Funktion
                                break;
                        }
                    },
                    MouseDownAction = (item) =>
                    {
                        //Das MouseDown-Event kommt vor dem Wechsel des aktiven ListBox-Items. Deswegen muss ich hier den Wechsel selbst antriggern
                        this.PrototypViewModel.SelectedItem = this.PrototypViewModel.Items.First(x => x.Item == item);

                        UseFunction(FunctionType.AddItem);
                    },
                    DeleteItemAction = (item) => this.editorData.RemoveLevelItemsAssociatedToPrototyp(item),
                    AddBackgroundItem = (fileName) =>
                    {
                        var proto = new BackgroundPrototypItem(fileName, this.PrototypViewModel.CreateNewId(), new InitialRotatedRectangleValues());
                        this.PrototypViewModel.AddItem(proto);
                        var vm = new BackgroundItemViewModel(proto);
                        this.PropertyControl = new BackgroundItemControl.BackgroundItemControl() { DataContext = vm };
                    }
                });

            this.SelectedItemViewModel = new SelectedItemViewModel(this.editorData);
            this.SelectedItemViewModel.RotateResizeButtonClick.Subscribe(x => UseFunction(FunctionType.RotateResize));
            this.SelectedItemViewModel.KeyboardmappingClick.Subscribe(x => UseFunction(FunctionType.KeyboardMapping));
            this.SelectedItemViewModel.IsCameraTrackedClick.Subscribe(x => UseFunction(FunctionType.EditCameraTracker));
            this.SelectedItemViewModel.GroupItemsClick.Subscribe(x =>
            {
                this.PrototypViewModel.AddItem(GroupedItemBuilder.BuildPrototypFromSelectedItems(this.editorData, PrototypViewModel.CreateNewId()));
            });

            this.LevelTreeViewModel = new TreeViewModel(this.editorData);
            this.editorData.TagDataStorrage = new TagDataStorrage(this.LevelTreeViewModel.GetNodeNameByTagObject);
            this.TagItemViewModel = new TagItemViewModel(this.LevelTreeViewModel, this.editorData);
            this.TagItemViewModel.DefineTagColorClick = ReactiveCommand.Create(() =>
            {
                UseFunction(FunctionType.DefineTagColor);
            });
            this.TagItemViewModel.DefineAnchorPointClick = ReactiveCommand.Create(() =>
            {
                UseFunction(FunctionType.DefineAnchorPoint);
            });

            this.CameraSettingViewModel = new CameraSettingViewModel(inputData.Panel.Width, inputData.Panel.Height, new System.Drawing.RectangleF(0, 0, inputData.Panel.Width, inputData.Panel.Height));
            this.CameraSettingViewModel.Camera.ShowOriginalPosition = true;
            this.CameraSettingViewModel.WhenAnyValue(x => x.UseAutoZoom).Subscribe(x =>
            {
                if (editorData.LevelItems.Any())
                {
                    this.CameraSettingViewModel.Camera.UpdateSceneBoundingBox(LevelItemsHelper.GetBoundingBox(editorData.LevelItems.ToList()));
                    this.CameraSettingViewModel.Camera.SetInitialCameraPosition();
                }
            });
            this.editorData.Camera = this.CameraSettingViewModel.Camera;

            this.SaveClick = ReactiveCommand.Create(() =>
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog.InitialDirectory = new DirectoryInfo(this.dataFolder).FullName;
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    WriteToTextFile(saveFileDialog.FileName);
                }

            });
            this.LoadClick = ReactiveCommand.Create(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.InitialDirectory = new DirectoryInfo(this.dataFolder).FullName;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    LoadFromTextFile(openFileDialog.FileName);
                }
            });
            this.GoBackClick = ReactiveCommand.Create(() =>
            {
                inputData.IsFinished?.Invoke(this);
            });

            this.GridModeClick = ReactiveCommand.Create(() =>
            {
                UseFunction(FunctionType.EditGrid);
            });
            this.SmallWindowClick = ReactiveCommand.Create(() =>
            {
                this.editorData.ShowSmallWindow = !this.editorData.ShowSmallWindow;
                if (this.CurrentState != FunctionType.Simulator)
                    UseFunction(FunctionType.MoveSelect);
            });
            this.ShowSideBarClick = ReactiveCommand.Create(() =>
            {
                this.ShowSideBar = !this.ShowSideBar;
                if (this.ShowSideBar)
                {
                    this.ShowSideBarImage = new BitmapImage(new Uri(FilePaths.DoubleLeft, UriKind.Absolute));
                }
                else
                {
                    this.ShowSideBarImage = new BitmapImage(new Uri(FilePaths.DoubleDown, UriKind.Absolute));
                }
            });
            this.DefineCollisionMatrixClick = ReactiveCommand.Create(() =>
            {
                UseFunction(FunctionType.DefineCollisionMatrix);
            });
            this.CollisionMatrixViewModel = this.editorData.CollisionMatrixViewModel;

            this.PolygonControlViewModel = new PolygonControlViewModel(
                () => UseFunction(FunctionType.AddPolygon),
                () => UseFunction(FunctionType.AddLawnEdge),
                editorData.PolygonImages, editorData.Camera, this.editorData.SelectedItems);

            this.SimulatorViewModel = new SimulatorViewModel(this.editorData,
                () =>
                {
                    //ButtonWasPressed-Action
                    if (this.CurrentState != FunctionType.Simulator)
                    {
                        UseFunction(FunctionType.Simulator);
                        this.SimulatorViewModel.SetModel(this.function as SimulatorFunction);
                        this.ForcePlotterViewModel.SetModel(this.function as SimulatorFunction);
                    }
                });
            this.SimulatorViewModel.WhenAnyValue(x => x.HasGravity).Subscribe(x => { this.editorData.HasGravity = x; });
            this.SimulatorViewModel.WhenAnyValue(x => x.Gravity).Subscribe(x => { this.editorData.Gravity = x; });
            this.SimulatorViewModel.WhenAnyValue(x => x.IterationCount).Subscribe(x => { this.editorData.SimulatorIterationCount = x; });

            this.ForcePlotterViewModel = new ForcePlotterViewModel(this.editorData);

            //Es wurde ein einzelnes Objekt selektiert -> Editiere es
            this.editorData.SelectedItems.Connect().Where(x => this.editorData.SelectedItems.Count == 1).Subscribe(x =>
            {
                ILevelItem item = this.editorData.SelectedItems.Items.First();

                if (item is LawnEdgeLevelItem)
                    UseFunction(FunctionType.EditLawnEdge);
                else if (item is IRotateableLevelItem)
                    UseFunction(FunctionType.RotateResize);
                else if (item is IEditablePolygon)
                    UseFunction(FunctionType.EditPolygon);
            });

            //Es wurden alle Objekte deselektiert -> Gehe in MoveSelect
            this.editorData.SelectedItems.Connect().Where(x => this.editorData.SelectedItems.Count == 0).Subscribe(x =>
            {
                if (this.CurrentState != FunctionType.MoveSelect)
                    UseFunction(FunctionType.MoveSelect);
            });

            //Es wurden mehrere Objekte selektiert -> Gehe in MoveSelect
            this.editorData.SelectedItems.Connect().Where(x => this.editorData.SelectedItems.Count > 1).Subscribe(x =>
            {
                if (this.CurrentState != FunctionType.MoveSelect)
                {
                    var selectedItems = this.editorData.SelectedItems.Items.ToList();
                    UseFunction(FunctionType.MoveSelect); //MoveSelect ruft beim Init UnselectAllItems auf
                    EditorStateExtensions.SelectItems(this.editorData, selectedItems); //Selektiere die Items erneut
                }
                    
            });

            UseFunction(FunctionType.MoveSelect);
        }

        public SimulatorInputData GetSimulatorExport()
        {
            //Nur die SimulatorFunction hat den Exporter
            var stateBevore = this.CurrentState;
            if (this.CurrentState != FunctionType.Simulator)
            {
                UseFunction(FunctionType.Simulator);

            }
            var data = (this.function as SimulatorFunction).GetSimulatorExportData();

            UseFunction(stateBevore);

            return data;
        }

        private void UseFunction(FunctionType functionType)
        {
            if (functionType != FunctionType.Simulator)
                this.SimulatorViewModel.SetIsRunning(false); //Stoppe die Simulation, sobalt eine andere Funktion genutzt wird

            //Beende die vorherige Funktion
            if (this.function?.Type != functionType)
            {
                if (this.function != null) this.function.Dispose();
                this.PropertyControl = null;
            }

            switch (functionType)
            {
                case FunctionType.Nothing:
                    this.function = null;
                    break;

                case FunctionType.AddItem:
                    this.function = new AddItemFunction().Init(this.editorData,
                        () => UseFunction(FunctionType.MoveSelect));
                    break;

                case FunctionType.MoveSelect:
                    this.function = new MoveSelectFunction().Init(this.editorData);
                    break;

                case FunctionType.Simulator:
                    this.function = new SimulatorFunction().Init(this.editorData, this.SimulatorViewModel.ForceTrackerDrawingSettings);
                    break;

                case FunctionType.AddPolygon:
                    this.function = new AddPolygonFunction().Init(this.editorData,
                        () => UseFunction(FunctionType.MoveSelect));
                    break;

                case FunctionType.EditPolygon:
                    this.function = new EditPolygonFunction().Init(this.editorData,
                        UseMoveSelect);
                    break;

                case FunctionType.AddLawnEdge:
                    this.function = new AddAndEditLawnEdgeFunction().InitForAddNewItem(this.editorData, this.dataFolder,
                        UseMoveSelect);
                    break;

                case FunctionType.EditLawnEdge:
                    this.function = new AddAndEditLawnEdgeFunction().InitForEditItem(this.editorData,
                        UseMoveSelect);
                    break;

                case FunctionType.KeyboardMapping:
                    this.function = new KeyboardMappingFunction().Init(this.editorData);
                    break;

                case FunctionType.RotateResize:
                    this.function = new RotateResizeFunction().Init(this.editorData,
                        UseMoveSelect);
                    break;

                case FunctionType.EditGrid:
                    this.function = new EditGridFunction().Init(this.editorData,
                        UseMoveSelect);
                    break;

                case FunctionType.DefineCollisionMatrix:
                    this.function = new DefineCollisionMatrixFunction().Init(this.editorData);
                    break;

                case FunctionType.EditCameraTracker:
                    this.function = new EditCameraTrackerFunction().Init(this.editorData);
                    break;

                case FunctionType.DefineTagColor:
                    this.function = new DefineTagColorFunction().Init(this.editorData, this.TagItemViewModel);
                    break;

                case FunctionType.DefineAnchorPoint:
                    this.function = new DefineAnchorPointFunction().Init(this.editorData);
                    break;
            }

            if (this.function != null && this.function.HasPropertyControl)
            {
                this.PropertyControl = this.function.GetPropertyControl();
            }

            if (functionType != FunctionType.Simulator)
            {
                this.SimulatorViewModel.SetModel(null);
                this.ForcePlotterViewModel.SetModel(null);
            }


            this.CurrentState = functionType;
            UpdateButtonColors();
        }

        private void UseMoveSelect(MouseEventArgs? e)
        {
            UseFunction(FunctionType.MoveSelect);

            //Nutze den Mausklick, der zum Beenden von der alten Funktion geführt hat, um damit bei MoveSelect das erste Item zu selektieren
            if (e != null)
            {
                var movSelectFunction = (MoveSelectFunction)this.function;
                movSelectFunction.HandleMouseDown(e);
                movSelectFunction.HandleMouseUp(e);
            }
        }

        private void UpdateButtonColors()
        {
            this.PolygonControlViewModel.UpdateButtonColors(this.CurrentState);
            this.SelectedItemViewModel.UpdateButtonColors(this.CurrentState);
        }

        #region IGraphicPanelHandler, ITimerHandler
        public void HandleTimerTick(float dt)
        {
            this.function?.HandleTimerTick(dt);
        }
        public void HandleMouseClick(System.Windows.Forms.MouseEventArgs e)
        {
            this.function?.HandleMouseClick(e);
        }
        public void HandleMouseWheel(System.Windows.Forms.MouseEventArgs e)
        {
            this.function?.HandleMouseWheel(e);
        }
        public void HandleMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            this.function?.HandleMouseMove(e);
        }
        public void HandleMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            this.function?.HandleMouseDown(e);
        }
        public void HandleMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            this.function?.HandleMouseUp(e);
        }
        public void HandleMouseEnter()
        {
            this.function?.HandleMouseEnter();
        }
        public void HandleMouseLeave()
        {
            this.function?.HandleMouseLeave();
        }
        public void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                UseFunction(FunctionType.MoveSelect);
                this.SimulatorViewModel.SetIsRunning(false);
                this.editorData.UnselectAllItems();
                return;
            }
            this.function?.HandleKeyDown(e);

            //Ich muss das auf Handelt setzen damit ich während der Simulation die Pfeiltasten drücken kann ohne dass das Fenster sein Focus verliert
            //Wenn aber ein PropertyControl angezeigt wird, darf ich Handled nicht auf true setzen, da sonst keine Texteingaben im Control möglich sind
            //if (this.PropertyControl == null)
            //    e.Handled = true; 
        }
        public void HandleKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            this.function?.HandleKeyUp(e);
        }
        public void HandleSizeChanged(int width, int height)
        {
            this.CameraSettingViewModel.HandleSizeChanged(width, height);
            this.function?.HandleSizeChanged(width, height);
        }
        #endregion

        #region IObjectSerializable
        public object GetExportObject()
        {
            return new LevelEditorExportData()
            {
                Prototyps = PrototypViewModel.GetExportData(),
                LevelItems = this.editorData.LevelItems.Select(x => x.GetExportData()).ToArray(),
                BackgroundImage = this.editorData.PolygonImages.BackgroundImage,
                ForegroundImage = this.editorData.PolygonImages.ForegroundImage,
                BackgroundImageMode= this.editorData.PolygonImages.BackgroundImageMode,
                UseCameraAutoZoom = this.CameraSettingViewModel.UseAutoZoom,
                InitialCameraPosition = this.editorData.Camera.InitialPosition,
                HasGravity = this.editorData.HasGravity,
                SimulatorIterationCount = this.editorData.SimulatorIterationCount,
                Gravity = this.editorData.Gravity,
                KeyboardMappingTables = this.editorData.KeyboradMappings.ToArray(),
                ShowGrid = this.editorData.Grid.ShowGrid,
                GridSize = this.editorData.Grid.Size,
                CollisionMatrix = this.editorData.CollisionMatrixViewModel.CollideMatrix,
                CameraTrackedLevelItemId = this.editorData.CameraTrackedItem != null ? this.editorData.CameraTrackedItem.Id : -1,
                CameraTrackerData = this.editorData.CameraTrackerData,
                TagData = this.TagItemViewModel.GetExportData()
            };
        }
        
        public void LoadFromExportObject(object exportObject)
        {
            var data = (LevelEditorExportData)exportObject;

            this.PrototypViewModel.LoadExportData(data.Prototyps);

            var prototyps = this.PrototypViewModel.Items.Select(x => x.Item).ToList();

            this.editorData.PolygonImages.BackgroundImage = data.BackgroundImage;
            this.editorData.PolygonImages.ForegroundImage = data.ForegroundImage;
            this.PolygonControlViewModel.BackgroundImageMode = this.editorData.PolygonImages.BackgroundImageMode = data.BackgroundImageMode;
            if (string.IsNullOrEmpty(this.editorData.PolygonImages.BackgroundImage) == false) this.editorData.Camera.UpdateBackgroundImage(this.editorData.PolygonImages.Background.Size);
            this.SimulatorViewModel.HasGravity = this.editorData.HasGravity = data.HasGravity;
            this.SimulatorViewModel.IterationCount = this.editorData.SimulatorIterationCount = data.SimulatorIterationCount;
            this.SimulatorViewModel.Gravity = this.editorData.Gravity = data.Gravity;
            this.editorData.KeyboradMappings = data.KeyboardMappingTables != null ? data.KeyboardMappingTables.ToList() : new List<KeyboardMappingTable>();
            this.editorData.Grid.ShowGrid = data.ShowGrid;
            this.editorData.Grid.Size = data.GridSize;

            if (data.CollisionMatrix == null) data.CollisionMatrix = new bool[5, 5];
            this.editorData.CollisionMatrixViewModel.CollideMatrix = data.CollisionMatrix;

            this.editorData.LevelItems.Clear();
            this.editorData.SelectedItems.Clear();
            foreach (var item in data.LevelItems)
            {
                if (item is PhysicLevelItemExportData)
                    this.editorData.AddLevelItem(LevelItems.PhysicItem.PhysicLevelItem.CreateFromExportData((PhysicLevelItemExportData)item, prototyps));

                if (item is PolygonLevelItemExportData)
                    this.editorData.AddLevelItem(PolygonLevelItem.CreateFromExportData((PolygonLevelItemExportData)item, this.editorData.PolygonImages));

                if (item is BackgroundLevelItemExportData)
                    this.editorData.AddLevelItem(BackgroundLevelItem.CreateFromExportData((BackgroundLevelItemExportData)item, prototyps));

                if (item is GroupedItemLevelExportData)
                    this.editorData.AddLevelItem(GroupedItemsLevelItem.CreateFromExportData((GroupedItemLevelExportData)item, prototyps));
            }

            PolygonLevelItem.UpdateIsOutsideAndUVFromAllPolygons(this.editorData.LevelItems.Where(x => x is PolygonLevelItem).Cast<PolygonLevelItem>().ToList());

            foreach (var item in data.LevelItems)
            {
                //Die LawEdgeLevelItems dürfen erst nach dem Aufruf von UpdateIsOutsideAndUVFromAllPolygons erstellt werden, da dort noch das IsOutside-Flag gesetzt wird
                if (item is LawnEdgeExportData)
                    this.editorData.AddLevelItem(LawnEdgeLevelItem.CreateFromExportData((LawnEdgeExportData)item, this.editorData.LevelItems.ToList()));
            }

            if (data.CameraTrackerData == null) data.CameraTrackerData = new CameraTrackerData();            
            this.editorData.CameraTrackerData = data.CameraTrackerData;
            this.SimulatorViewModel.UseCameraTracking = data.CameraTrackerData.IsActive;

            this.editorData.CameraTrackedItem = this.editorData.LevelItems.FirstOrDefault(x => x.Id == data.CameraTrackedLevelItemId);

            this.CameraSettingViewModel.UseAutoZoom = data.UseCameraAutoZoom;
            this.editorData.Camera.InitialPosition = this.CameraSettingViewModel.InitialPosition = data.InitialCameraPosition;

            this.CameraSettingViewModel.Reset();

                      

            //this.TagItemViewModel.LoadFromExport(data.TagData, this.editorData.Prototyps.ToList(), this.editorData.LevelItems.ToList());
            this.editorData.TagDataStorrage.LoadExportData(data.TagData);

            UseFunction(FunctionType.MoveSelect);
        }


        #endregion

        #region IToTextWriteable
        public void WriteToTextFile(string filePath)
        {
            UseFunction(FunctionType.MoveSelect); //Wenn eine EditorFunction gerade ein Objekt editiert, dann bringt es teilweise erst beim Dispose die Daten in den EditorState (Sehe KeyBoardmapping-Funktion). Deswegen trigger ich hier Dispose von der aktiven Funktion.
            FileNameReplacer.SaveEditorFile(filePath, JsonHelper.Helper.ToJson((LevelEditorExportData)GetExportObject()));
        }
        public void LoadFromTextFile(string filePath)
        {
            var data = JsonHelper.Helper.CreateFromJson<LevelEditorExportData>(FileNameReplacer.LoadEditorFile(filePath));
            LoadFromExportObject(data);
        }
        #endregion

        #region ISimlatorUser
        public void SetSimulatorBuildMethod(CreateSimulatorFunction createSimulatorFunction)
        {
            this.editorData.CreateSimulator = createSimulatorFunction;
        }
        #endregion

        //Diese Funktion muss gerufen werden, wenn ein Prototyp editiert wurde
        //Aktualisiere all die LevelItems, welche den Prototyp nutzen, welcher editiert wurde
        public void UpdateAfterPrototypWasChanged(IPrototypItem oldItem, IPrototypItem newItem)
        {
            this.PrototypViewModel.UpdateItem(oldItem, newItem); //Aktualisiere die Item-Daten aus dem Prototyp-Control
            foreach (var item in this.editorData.LevelItems)
            {
                if (item is IPrototypLevelItem)
                {
                    var proto = (IPrototypLevelItem)item;
                    if (proto.AssociatedPrototyp.Id == newItem.Id)
                    {
                        proto.UpdateAfterPrototypWasChanged(oldItem, newItem); //Aktualisiere IPrototypLevelItem.AssociatedPrototyp von allen LevelItems


                        //Wenn sich die Anzahl der Joints/Animations geändert hat, dann müssen die Keyboard-Mapping-Einträge 
                        //für alle zugehörigen LevelItems gelöscht werden, da die KeyboardMappingEntry.HandlerId auf ein Index
                        //aus der PhysicScene verweist
                        var keyBoardTable = this.editorData.KeyboradMappings.FirstOrDefault(x => x.LevelItemId == item.Id);
                        if (keyBoardTable != null && oldItem is IKeyboardControlledLevelItem)
                        {
                            int oldHandlerCount = (oldItem as IKeyboardControlledLevelItem).GetAllKeyPressHandlerNames().Length;
                            int newHandlerCount = (newItem as IKeyboardControlledLevelItem).GetAllKeyPressHandlerNames().Length;
                            if (newHandlerCount < oldHandlerCount)
                            {
                                this.editorData.KeyboradMappings.Remove(keyBoardTable);
                                MessageBox.Show($"Remove KeyBoardMappings from LevelItem {item.Id} because the Joint/Animation-Count has decreased");
                            }
                        }
                    }
                }
            }
            this.LevelTreeViewModel.BuildTree(); //Aktualisiere das TreeControl

            

        }
    }
}
