using GraphicPanels;
using GraphicPanelWpf;
using KeyFrameEditorControl.Controls.KeyDefine;
using KeyFrameGlobal;
using KeyFramePhysicImporter.ViewModel;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Linq;
using System.Reactive;
using System.Windows.Forms;
using WpfControls.Model;
using System.IO;
using System.Reactive.Linq;

namespace KeyFrameEditorControl.Controls.KeyFrameEditor
{
    //Diese Klasse zeigt entweder das Importer-UserControl an, um mit dessen Hilfe ein AnimatorInputData-Objekt zu erzeugen
    //oder es erzeugt mit den AnimatorInputData-Objekt ein KeyDefineControl, der dann ein AnimationOutputData-Objekt erzeugt
    //Auf diese Weise kann aus ein beliebigen Objekt, was aus einer Menge von Propertys besteht ein zugehöriges AnimationOutputData-Objekt erzeugt werden
    public class KeyFrameEditorViewModel : ReactiveObject, IGraphicPanelHandler, ITimerHandler, IStringSerializable, IObjectSerializable, IPhysicSceneEditor
    {
        private GraphicPanel2D panel;
        private float timerTickRateInMs; //Wird für die Anzeige der Animationsdauer in Sekunden gebraucht
        private KeyFrameEditorExportData initialSettings;
        private ImporterControlViewModel importerViewModel = null;
        private bool showStartTimeTextbox = true;

        [Reactive] public System.Windows.Controls.UserControl ContentUserControl { get; set; }

        public ReactiveCommand<Unit, Unit> ImportPhysicSceneClick { get; private set; }
        public ReactiveCommand<Unit, Unit> SaveClick { get; private set; } //Animationsdatei speichern
        public ReactiveCommand<Unit, Unit> LoadClick { get; private set; } //Animationsdatei laden
        [Reactive] public bool ShowSaveLoadButtons { get; private set; }

        [Reactive] public bool ImportStepIsDone { get; private set; } = false;

        public string PhysicSceneJson { get; private set; } //Wird für den PhysicEngine-Editor benötigt damit er sieht, ob es Änderungen in der PhysicScene gab

        public KeyFrameEditorViewModel(GraphicPanel2D panel, float timerTickRateInMs, bool showSaveLoadButtons, bool showStartTimeTextbox, string physicSceneJson)
        {
            this.panel = panel;
            this.timerTickRateInMs = timerTickRateInMs;
            this.ShowSaveLoadButtons = showSaveLoadButtons;
            this.showStartTimeTextbox = showStartTimeTextbox;

            this.ImportPhysicSceneClick = ReactiveCommand.Create(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                    ImportPhysicScene(File.ReadAllText(openFileDialog.FileName));
            });
            this.SaveClick = ReactiveCommand.Create(() =>
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    File.WriteAllText(saveFileDialog.FileName, GetAnimationDataJson());

            },
            this.WhenAnyValue(x => x.ContentUserControl).Select(x => x?.DataContext is KeyDefineViewModel) //CanExecute: Zeige den Savebutton nur, wenn vom ContentUserControl der DataContext ein KeyDefineViewModel ist
            );
            this.LoadClick = ReactiveCommand.Create(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                    LoadAnimationDataJson(File.ReadAllText(openFileDialog.FileName));
            },
            this.WhenAnyValue(x => x.ContentUserControl).Select(x => x?.DataContext is KeyDefineViewModel) //CanExecute: Zeige den Loadbutton nur, wenn vom ContentUserControl der DataContext ein KeyDefineViewModel ist
            );

            if (string.IsNullOrEmpty(physicSceneJson) == false)
                ImportPhysicScene(physicSceneJson);
        }

        private string GetAnimationDataJson()
        {
            var vm = (KeyDefineViewModel)this.ContentUserControl.DataContext;
            string json = JsonHelper.Helper.ToJson(vm.GetAnimationData());
            return json;
        }

        private void LoadAnimationDataJson(string json)
        {
            var vm = (KeyDefineViewModel)this.ContentUserControl.DataContext;
            var data = JsonHelper.Helper.CreateFromJson<AnimationOutputData>(json);
            data.ReplaceDoublesWithFloats();
            vm.LoadAnimationData(data);
        }

        private void ImportPhysicScene(string physicSceneJson)
        {
            this.PhysicSceneJson = physicSceneJson;
            var vm = new ImporterControlViewModel(this.panel, physicSceneJson);
            vm.ImportIsFinished += ImportIsFinished;
            this.ContentUserControl = new KeyFramePhysicImporter.View.ImporterControl(vm, this.panel);
            this.importerViewModel = vm;
        }
        private void ImportIsFinished(AnimatorInputData data)
        {
            var vm = new KeyDefineViewModel(this.panel, data, this.timerTickRateInMs, this.showStartTimeTextbox);
            if (this.initialSettings?.AnimationData != null)
            {
                vm.LoadAnimationData(initialSettings.AnimationData);
            }
            (this.ContentUserControl.DataContext as IImporterControl).ImportIsFinished -= ImportIsFinished;
            this.ContentUserControl = new Controls.KeyDefine.KeyDefineControl(vm, this.panel);
            this.ImportStepIsDone = true;
        }

        public void HandleTimerTick(float dt)
        {
            if (this.ContentUserControl != null)
                (this.ContentUserControl.DataContext as ITimerHandler).HandleTimerTick(dt);
        }

        #region IGraphicPanelHandler
        public void HandleMouseClick(System.Windows.Forms.MouseEventArgs e)
        {
            if (this.ContentUserControl?.DataContext is IGraphicPanelHandler)
                (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleMouseClick(e);
        }
        public void HandleMouseWheel(System.Windows.Forms.MouseEventArgs e)
        {
            if (this.ContentUserControl?.DataContext is IGraphicPanelHandler)
                (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleMouseWheel(e);
        }
        public void HandleMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            if (this.ContentUserControl?.DataContext is IGraphicPanelHandler)
                (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleMouseMove(e);
        }
        public void HandleMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            if (this.ContentUserControl?.DataContext is IGraphicPanelHandler)
                (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleMouseDown(e);
        }
        public void HandleMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            if (this.ContentUserControl?.DataContext is IGraphicPanelHandler)
                (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleMouseUp(e);
        }
        public void HandleMouseEnter()
        {
            if (this.ContentUserControl?.DataContext is IGraphicPanelHandler)
                (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleMouseEnter();
        }
        public void HandleMouseLeave()
        {
            if (this.ContentUserControl?.DataContext is IGraphicPanelHandler)
                (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleMouseLeave();
        }
        public void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (this.ContentUserControl?.DataContext is IGraphicPanelHandler)
                (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleKeyDown(e);
        }
        public void HandleKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            if (this.ContentUserControl?.DataContext is IGraphicPanelHandler)
                (this.ContentUserControl.DataContext as IGraphicPanelHandler).HandleKeyUp(e);
        }
        public void HandleSizeChanged(int width, int height)
        {
            if (this.ContentUserControl?.DataContext is ISizeChangeable)
                (this.ContentUserControl.DataContext as ISizeChangeable).HandleSizeChanged(width, height);
        }
        #endregion


        #region IStringSerializable

        public object GetExportObject()
        {
            if (this.ContentUserControl.DataContext is ImporterControlViewModel)
            {
                var vm = (ImporterControlViewModel)this.ContentUserControl.DataContext;
                var data = new KeyFrameEditorExportData()
                {
                    ImporterData = vm.GetExportData(),
                    AnimationData = null
                };

                return data;
            }

            if (this.ContentUserControl.DataContext is KeyDefineViewModel)
            {
                var vm = (KeyDefineViewModel)this.ContentUserControl.DataContext;

                var data = new KeyFrameEditorExportData()
                {
                    ImporterData = this.importerViewModel.GetExportData(),
                    AnimationData = vm.GetAnimationData(),
                };
                return data;

            }

            return new KeyFrameEditorExportData();
        }
        public void LoadFromExportObject(object exportObject)
        {
            this.initialSettings = (KeyFrameEditorExportData)exportObject;

            //Diese Klasse hier wurde neu angelegt und zeigt gerade den Importer -> Mach alle Eingaben für den Importer und wechsle dann zur KeyDefine-Ansicht
            if (this.ContentUserControl.DataContext is ImporterControlViewModel && this.initialSettings.ImporterData != null)
            {
                var vm = (ImporterControlViewModel)this.ContentUserControl.DataContext;
                vm.LoadFromExportData(this.initialSettings.ImporterData);

                var animationInputData = vm.GetAnimationData();
                var vm1 = new KeyDefineViewModel(this.panel, animationInputData, this.timerTickRateInMs, this.showStartTimeTextbox);
                this.ContentUserControl = new Controls.KeyDefine.KeyDefineControl(vm1, this.panel);
                this.ImportStepIsDone = true;
            }

            //Nach den Wechsel zur KeyDefine-Ansicht werden hier noch alle Keys angelegt
            if (this.ContentUserControl.DataContext is KeyDefineViewModel && initialSettings.AnimationData != null)
            {
                initialSettings.AnimationData.ReplaceDoublesWithFloats();
                var vm = (KeyDefineViewModel)this.ContentUserControl.DataContext;
                initialSettings.AnimationData.ReplaceDoublesWithFloats();
                vm.LoadAnimationData(initialSettings.AnimationData);
            }
        }

        public string GetExportString()
        {
            return JsonHelper.Helper.ToJson((KeyFrameEditorExportData)GetExportObject());
        }
        public void LoadFromExportString(string exportString)
        {
            LoadFromExportObject(JsonHelper.Helper.CreateFromJson<KeyFrameEditorExportData>(exportString));
        }
        #endregion
    }
}
