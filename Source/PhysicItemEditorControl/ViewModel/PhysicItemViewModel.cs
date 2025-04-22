using JsonHelper;
using KeyFrameEditorControl;
using KeyFrameGlobal;
using KeyFramePhysicImporter.Model;
using PhysicItemEditorControl.Model.PhysicTabMerging;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using RigidBodyPhysics.ExportData;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using TextureEditorControl.Controls.Editor;
using TextureEditorControl;
using TextureEditorGlobal;
using WpfControls.Model;
using GraphicPanelWpf;
using GraphicPanels;
using System.Reactive.Linq;
using KeyFrameEditorControl.Controls.KeyFrameEditor;
using PhysicSceneEditorControl;
using PhysicItemEditorControl.Model;
using LevelEditorGlobal;
using System.Windows.Forms;
using System.IO;
using DynamicData.Binding;

namespace PhysicItemEditorControl.ViewModel
{
    public class PhysicItemViewModel : ReactiveObject, IGraphicPanelHandler, ITimerHandler, IStringSerializable, IObjectSerializable, IPrototypItemFactory
    {
        private GraphicPanel2D panel;
        private float timerTickRateInMs;
        private int id;
        private InitialRotatedRectangleValues initialRecValues = new InitialRotatedRectangleValues();
        private bool showStartTimeTextbox = false;

        public ReactiveCommand<Unit, Unit> SaveClick { get; private set; } //LevelItem speichern
        public ReactiveCommand<Unit, Unit> LoadClick { get; private set; } //LevelItem laden
        [Reactive] public bool ShowSaveLoadButtons { get; private set; }
        public ObservableCollection<TabItemViewModel> Tabs { get; set; } = new ObservableCollection<TabItemViewModel>();
        [Reactive] public TabItemViewModel SelectedTab { get; set; }

        public ReactiveCommand<Unit, Unit> AddTextureClick { get; private set; }
        public ReactiveCommand<Unit, Unit> AddAnimationClick { get; private set; }
        [Reactive] public bool ShowGoBackButton { get; private set; } = false;
        
        public ReactiveCommand<Unit, Unit> GoBackClick { get; private set; }
        [Reactive] public string ErrorMessage { get; private set; } = null;

        public PhysicItemViewModel(EditorInputData data, int id, bool showGoBackButton, bool showStartTimeTextbox)
        {
            this.panel = data.Panel;
            this.id = id;
            this.ShowSaveLoadButtons = data.ShowSaveLoadButtons;
            this.ShowGoBackButton = showGoBackButton;
            this.showStartTimeTextbox = showStartTimeTextbox;
            this.timerTickRateInMs = data.TimerTickRateInMs;

            this.SaveClick = ReactiveCommand.Create(() =>
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    File.WriteAllText(saveFileDialog.FileName, GetExportString());

            },
            this.WhenAnyValue(x => x.ShowSaveLoadButtons) //CanExecute: Zeige den Savebutton nur, wenn IsLoaded true ist
            );
            this.LoadClick = ReactiveCommand.Create(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                    LoadFromExportString(File.ReadAllText(openFileDialog.FileName));
            },
            this.WhenAnyValue(x => x.ShowSaveLoadButtons) //CanExecute: Zeige den Loadbutton nur, wenn IsLoaded true ist
            );

            this.AddTextureClick = ReactiveCommand.Create(() =>
            {
                var physicScene = (PhysicSceneExportData)(this.Tabs[0].Content.DataContext as IObjectSerializable).GetExportObject();
                bool bodysAvailable = physicScene.Bodies.Any();

                if (bodysAvailable)
                {
                    this.Tabs.Add(CreateTabItem(TabItemViewModel.TabItemType.TextureEditor, null));
                    this.SelectedTab = this.Tabs.Last();
                }

            },
            this.Tabs.ToObservableChangeSet().Select(x => this.Tabs.All(y => y.Type != TabItemViewModel.TabItemType.TextureEditor)) //CanExecute: Prüfe dass es nur ein TexturTab gibt
            );

            this.AddAnimationClick = ReactiveCommand.Create(() =>
            {
                this.Tabs.Add(CreateTabItem(TabItemViewModel.TabItemType.AnimationEditor, null));
            });

            this.GoBackClick = ReactiveCommand.Create(() =>
            {
                foreach (var tab in this.Tabs)
                {
                    this.SelectedTab = tab; //Aktiviere nochmal jedes Tab, um auf diese Weise sicherzustellen, dass alle Tabs die gleiche PhysicScene nutzen
                }

                this.ErrorMessage = CheckCurrentConfigurationFromAllTabs();

                if (this.ErrorMessage == null)
                    data.IsFinished(this);
            });

            //So kann man mit Reavtive sowohl den alten als auch neuen Wert bei Änderung bekommen:
            //https://stackoverflow.com/questions/29100381/getting-prior-value-on-change-of-property-using-reactiveui-in-wpf-mvvm
            this.WhenAnyValue(x => x.SelectedTab)
                .Buffer(2, 1)
                .Select(b => (Previous: b[0], Current: b[1]))
                .Subscribe(x =>
                {
                    //Es wurde in den Textur- oder Animations-Editor gewechselt (Vorher war bereits ein Tab aktiv)
                    if (x.Previous?.Type != null &&
                        (x.Current?.Type == TabItemViewModel.TabItemType.TextureEditor || x.Current?.Type == TabItemViewModel.TabItemType.AnimationEditor))
                    {
                        var scene1 = GetPhysicDataFromTab(Tabs[0]);  //PhysicScene vom PhysikEditor
                        var scene2 = GetPhysicDataFromTab(x.Current);//PhysicScene vom Textur- oder Animationseditor

                        //Der TexturDaten werden ungültig, wenn die Body-Anzahl sich ändert
                        if (x.Current.Type == TabItemViewModel.TabItemType.TextureEditor &&
                            TextureTabMerger.PhysicSceneHasChanged(scene1, scene2))
                        {
                            var vm = (TextureEditorViewModel)x.Current.Content.DataContext;
                            var oldTextures = (VisualisizerOutputData)vm.GetExportObject();
                            var oldPhysicModel = JsonHelper.Helper.FromCompactJson<PhysicSceneExportData>((vm as IPhysicSceneEditor).PhysicSceneJson);

                            x.Current.Content = CreateEditorControl(x.Current.Type); //Erzeuge mit aktueller PhysicScene neue leere Texturdaten

                            vm = (TextureEditorViewModel)x.Current.Content.DataContext;
                            var newEmptyTextures = (VisualisizerOutputData)vm.GetExportObject();
                            var newPhysicModel = JsonHelper.Helper.FromCompactJson<PhysicSceneExportData>((this.Tabs[0].Content.DataContext as IStringSerializable).GetExportString());

                            var mergedTextures = TextureTabMerger.MergeOldWithNewData(oldTextures, oldPhysicModel, newEmptyTextures, newPhysicModel);
                            vm.LoadFromExportObject(mergedTextures);
                        }

                        //Der Animationsdaten werden ungültig, wenn die Joint- oder Thruster-Anzahl sich ändert
                        if (x.Current.Type == TabItemViewModel.TabItemType.AnimationEditor &&
                            AnimationTabMerger.PhysicSceneHasChanged(scene1, scene2))
                        {
                            var vm = (KeyFrameEditorViewModel)x.Current.Content.DataContext;
                            var oldAnimations = (KeyFrameEditorExportData)vm.GetExportObject();
                            var oldPhysicModel = JsonHelper.Helper.FromCompactJson<PhysicSceneExportData>((vm as IPhysicSceneEditor).PhysicSceneJson);

                            x.Current.Content = CreateEditorControl(x.Current.Type); //Erzeuge mit aktueller PhysicScene neues leere AnimationsTab
                            vm = (KeyFrameEditorViewModel)x.Current.Content.DataContext;
                            var newPhysicModel = JsonHelper.Helper.FromCompactJson<PhysicSceneExportData>((this.Tabs[0].Content.DataContext as IStringSerializable).GetExportString());

                            var mergedAnimations = AnimationTabMerger.MergeOldWithNewData(oldAnimations, oldPhysicModel, newPhysicModel);
                            vm.LoadFromExportObject(mergedAnimations);
                        }
                    }
                });

            LoadFromExportObject(data.InputData);

            this.SelectedTab = this.Tabs[0];
        }

        private string CheckCurrentConfigurationFromAllTabs()
        {
            //Prüfe, dass bei kein Animation-Tab der Import-Screen gezeigt wird damit es keine Null bei KeyFrameAnimatorViewModel.GetExportObject().AnimationData zurück gibt
            bool anyImportStepIsMissing = this
                    .Tabs
                    .Where(x => x.Content.DataContext is KeyFrameEditorViewModel)
                    .Select(x => x.Content.DataContext as KeyFrameEditorViewModel)
                    .Any(x => x.ImportStepIsDone == false);

            if (anyImportStepIsMissing)
                return "Check that the ImportStep from all Animation-Tabs are done";

            bool anyAnimationTabHasNoFrames = this
                    .Tabs
                    .Where(x => x.Content.DataContext is KeyFrameEditorViewModel)
                    .Select(x => x.Content.DataContext as KeyFrameEditorViewModel)
                    .Where(x => x.ImportStepIsDone)
                    .Select(x => (KeyFrameEditorExportData)x.GetExportObject())
                    .Any(x => x.AnimationData.Frames.Length == 0);
            if (anyAnimationTabHasNoFrames)
                return "Check that the each Animation-Tab has any Keys";

            var animations = this
                    .Tabs
                    .Where(x => x.Content.DataContext is KeyFrameEditorViewModel)
                    .Select(x => x.Content.DataContext as KeyFrameEditorViewModel)
                    .Where(x => x.ImportStepIsDone)
                    .Select(x => ((KeyFrameEditorExportData)x.GetExportObject()).AnimationData)
                    .ToArray();

            try
            {
                if (animations.Length > 0)
                    Animator.CheckThatEachPropertyHasOnlyOneAnimator(animations[0].Frames[0].Values.Length, animations);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }




            return null;
        }

        private PhysicSceneExportData GetPhysicDataFromTab(TabItemViewModel tab)
        {
            string physicSceneJson = ((IPhysicSceneEditor)tab.Content.DataContext).PhysicSceneJson;
            return Helper.FromCompactJson<PhysicSceneExportData>(physicSceneJson);
        }

        #region IGraphicPanelHandler, ITimerHandler
        public void HandleTimerTick(float dt)
        {
            if (this.SelectedTab.Content.DataContext is ITimerHandler)
                (this.SelectedTab.Content.DataContext as ITimerHandler).HandleTimerTick(dt);
        }
        public void HandleMouseClick(System.Windows.Forms.MouseEventArgs e)
        {
            if (this.SelectedTab.Content.DataContext is IGraphicPanelHandler)
                (this.SelectedTab.Content.DataContext as IGraphicPanelHandler).HandleMouseClick(e);
        }
        public void HandleMouseWheel(System.Windows.Forms.MouseEventArgs e)
        {
            if (this.SelectedTab.Content.DataContext is IGraphicPanelHandler)
                (this.SelectedTab.Content.DataContext as IGraphicPanelHandler).HandleMouseWheel(e);
        }
        public void HandleMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            if (this.SelectedTab.Content.DataContext is IGraphicPanelHandler)
                (this.SelectedTab.Content.DataContext as IGraphicPanelHandler).HandleMouseMove(e);
        }
        public void HandleMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            if (this.SelectedTab.Content.DataContext is IGraphicPanelHandler)
                (this.SelectedTab.Content.DataContext as IGraphicPanelHandler).HandleMouseDown(e);
        }
        public void HandleMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            if (this.SelectedTab.Content.DataContext is IGraphicPanelHandler)
                (this.SelectedTab.Content.DataContext as IGraphicPanelHandler).HandleMouseUp(e);
        }
        public void HandleMouseEnter()
        {
            if (this.SelectedTab.Content.DataContext is IGraphicPanelHandler)
                (this.SelectedTab.Content.DataContext as IGraphicPanelHandler).HandleMouseEnter();
        }
        public void HandleMouseLeave()
        {
            if (this.SelectedTab.Content.DataContext is IGraphicPanelHandler)
                (this.SelectedTab.Content.DataContext as IGraphicPanelHandler).HandleMouseLeave();
        }
        public void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (this.SelectedTab.Content.DataContext is IGraphicPanelHandler)
                (this.SelectedTab.Content.DataContext as IGraphicPanelHandler).HandleKeyDown(e);
        }
        public void HandleKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            if (this.SelectedTab.Content.DataContext is IGraphicPanelHandler)
                (this.SelectedTab.Content.DataContext as IGraphicPanelHandler).HandleKeyUp(e);
        }
        public void HandleSizeChanged(int width, int height)
        {
            if (this.SelectedTab.Content.DataContext is IGraphicPanelHandler)
                (this.SelectedTab.Content.DataContext as IGraphicPanelHandler).HandleSizeChanged(width, height);
        }

        #endregion
        #region IStringSerializable
        private System.Windows.Controls.UserControl CreateEditorControl(TabItemViewModel.TabItemType type)
        {
            switch (type)
            {
                case TabItemViewModel.TabItemType.PhysicEditor:
                    return new PhysicSceneEditorFactory().CreateEditorControl(new EditorInputData()
                    {
                        Panel = this.panel,
                        ShowSaveLoadButtons = false,
                        ShowGoBackButton = false,
                        TimerTickRateInMs = this.timerTickRateInMs
                    });

                case TabItemViewModel.TabItemType.TextureEditor:
                    return new TextureEditorFactory((this.Tabs[0].Content.DataContext as IStringSerializable).GetExportString()).CreateEditorControl(new EditorInputData()
                    {
                        Panel = this.panel,
                        ShowSaveLoadButtons = false,
                        TimerTickRateInMs = this.timerTickRateInMs
                    });

                case TabItemViewModel.TabItemType.AnimationEditor:
                    return new KeyFrameEditorFactory((this.Tabs[0].Content.DataContext as IStringSerializable).GetExportString(), this.showStartTimeTextbox).CreateEditorControl(new EditorInputData()
                    {
                        Panel = this.panel,
                        ShowSaveLoadButtons = false,
                        TimerTickRateInMs = this.timerTickRateInMs
                    });
            }

            throw new ArgumentException("Unknown type: " + type);
        }

        private TabItemViewModel CreateTabItem(TabItemViewModel.TabItemType type, object initialData)
        {
            var control = CreateEditorControl(type);

            if (initialData != null)
            {
                (control.DataContext as IObjectSerializable).LoadFromExportObject(initialData);
            }

            TabItemViewModel tabItem = new TabItemViewModel(type.ToString(), type, control, (tab) =>
            {
                int index = this.Tabs.IndexOf(tab);
                if (index > 0) //Tab mit Index 0 ist das PhysicTab was nicht gelöscht werden darf
                    this.Tabs.RemoveAt(index);
            });

            return tabItem;
        }

        private PhysicItemExportData GetExportData()
        {
            var data = new PhysicItemExportData()
            {
                Id = this.id,
                PhysicSceneData = (PhysicSceneExportData)GetExportObjectFromTab(this.Tabs[0]),
                InitialRecValues = this.initialRecValues
            };

            var textureTab = this.Tabs.Where(x => ((TabItemViewModel.TabItemType)x.Type) == TabItemViewModel.TabItemType.TextureEditor).FirstOrDefault();
            if (textureTab != null)
            {
                this.SelectedTab = this.Tabs[0];
                this.SelectedTab = textureTab; //Wechsel nochmal kurz in den Textur-Tab um den Merging-Prozess anzustoßen, sollte sich am PhysikModel was geändert haben
                data.TextureData = (VisualisizerOutputData)GetExportObjectFromTab(textureTab);
            }


            var animationTabs = this.Tabs.Where(x => ((TabItemViewModel.TabItemType)x.Type) == TabItemViewModel.TabItemType.AnimationEditor).ToList();
            if (animationTabs.Any())
            {
                this.SelectedTab = this.Tabs[0];
                foreach (var animationTab in animationTabs)
                {
                    this.SelectedTab = animationTab; //Wechsel nochmal kurz in den Animation-Tab um den Merging-Prozess anzustoßen, sollte sich am PhysikModel was geändert haben
                }
                data.AnimationData = animationTabs.Select(x => (KeyFrameEditorExportData)GetExportObjectFromTab(x)).ToArray();
            }


            if (data.AnimationData != null && data.AnimationData.Length > 0 && this.showStartTimeTextbox)
            {
                var isFix = data.AnimationData[0].ImporterData.IsFix;
                float timerIntercallInMilliseconds = 50; //Erstelle mit 50ms die korrigierten Daten
                var animations = data.AnimationData.Select(x => x.AnimationData).ToArray();
                int timeStepCount = 500;
                int iterationCount = 100;

                data.PhysicSceneForAnimationNull = PhysicSceneStartValueCreator.CreateSceneAfterPlayingNTimeSteps(isFix, data.PhysicSceneData, animations, timerIntercallInMilliseconds, timeStepCount, iterationCount);
            }

            return data;
        }
        private object GetExportObjectFromTab(TabItemViewModel tab)
        {
            return (tab.Content.DataContext as IObjectSerializable).GetExportObject();
        }

        public object GetExportObject()
        {
            return GetExportData();
        }
        public void LoadFromExportObject(object exportObject)
        {
            this.Tabs.Clear();

            if (exportObject == null)
            {
                //Erzeuge leere PhysicScene und sonst keine weiteren Tabs
                this.Tabs.Add(CreateTabItem(TabItemViewModel.TabItemType.PhysicEditor, null));
                return;
            }

            var data = (PhysicItemExportData)exportObject;

            this.id = data.Id;
            this.initialRecValues = data.InitialRecValues;
            this.Tabs.Add(CreateTabItem(TabItemViewModel.TabItemType.PhysicEditor, data.PhysicSceneData));

            if (data.TextureData != null)
            {
                this.Tabs.Add(CreateTabItem(TabItemViewModel.TabItemType.TextureEditor, data.TextureData));
            }

            if (data.AnimationData != null)
            {
                foreach (var aniData in data.AnimationData)
                {
                    this.Tabs.Add(CreateTabItem(TabItemViewModel.TabItemType.AnimationEditor, aniData));
                }
            }
        }

        public string GetExportString()
        {
            return JsonHelper.Helper.ToJson(GetExportData());
        }

        public void LoadFromExportString(string exportString)
        {
            var data = JsonHelper.Helper.CreateFromJson<PhysicItemExportData>(exportString);
            LoadFromExportObject(data);
        }
        #endregion

        #region IPrototypItemFactory
        //Erzeugt anhand der erstellen PhysicScene, VisualisizerOutputData und AnimationOutputData-Objekte ein PhysicPrototypItem-Objekt
        public IPrototypItem CreatePrototypItem() //Neues PhysicItem erzeugen (Editieren wird über LoadFromExportObject gemacht) 
        {
            var exportData = GetExportData();

            if (((PhysicSceneExportData)exportData.PhysicSceneData).Bodies.Any() == false) return null;

            return new PhysicPrototypItem(exportData);
        }


        #endregion
    }
}
