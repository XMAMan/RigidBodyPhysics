using DynamicData;
using GraphicPanels;
using LevelEditorControl.Controls.PolygonControl;
using LevelEditorControl.Controls.TagItemControl;
using LevelEditorControl.LevelItems;
using LevelEditorGlobal;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Simulator;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WpfControls.Controls.CameraSetting;
using WpfControls.Controls.CollisionMatrix;
using WpfControls.Model;

namespace LevelEditorControl.EditorFunctions
{
    internal class EditorState : ReactiveObject
    {
        public GraphicPanel2D Panel;
        public Camera2D Camera;
        public ObservableCollection<IPrototypItem> Prototyps = new ObservableCollection<IPrototypItem>();
        [Reactive] public IPrototypItem SelectedPrototyp { get; set; }
        public ObservableCollection<ILevelItem> LevelItems = new ObservableCollection<ILevelItem>();

        //Das ist eine Obervable-Liste damit sie als CanExecute-Trigger dienen kann
        //Es darf aber nicht ObservableCollectionExtended verwendet werden, da dort AddRange die Elememnte einzeln zur Liste hinzufügt
        //und jedes mal ein PropertyChanged-Event triggert. Wenn ich per Selektionsbox mehrere Elemente selektieren will, dann würde
        //immer zuerst die Funktion anspringen, welche für ein einzelnes Item da ist.
        public SourceList<ILevelItem> SelectedItems { get; set; } = new SourceList<ILevelItem>();

        public IMouseClickable SelectedSubItem { get; set; } = null; //Dieses Objekt (RigidBody, Polygon, Joint) wurde im TreeControl selektiert

        public float TimerIntervallInMilliseconds;
        public PolygonImages PolygonImages = new PolygonImages();
        public bool HasGravity = true;
        public int SimulatorIterationCount = 50;
        public float Gravity = 0.001f;
        public List<KeyboardMappingTable> KeyboradMappings = new List<KeyboardMappingTable>(); //Für jedes LevelItem wo es Tastenverarbeitung gibt gibt es ein eigenen Eintrag hier
        public MouseGrid Grid = new MouseGrid() { ShowGrid = false };
        public bool ShowSmallWindow = false;
        public bool ShowForceData = false; //Soll das ForcePlotterControl angezeigt werden?
        public ILevelItem CameraTrackedItem { get; set; } = null;
        public CameraTrackerData CameraTrackerData { get; set; } = new CameraTrackerData();

        private static int MatrixSize = 5;
        public CollisionMatrixViewModel CollisionMatrixViewModel = new CollisionMatrixViewModel(MatrixSize);
        public TagDataStorrage TagDataStorrage;

        //Build-Funktion zur Erzeugung von ein ILevelediturUsedSimulator-Objekt. Hiermit kann von außen dem LevelEditor vorgegeben werden,
        //welchen Simulator er nutzen soll.
        public CreateSimulatorFunction CreateSimulator = (data, panelSize, camera, timerIntercallInMilliseconds) => new Simulator.Simulator(data, panelSize, camera, timerIntercallInMilliseconds);
    }
}
