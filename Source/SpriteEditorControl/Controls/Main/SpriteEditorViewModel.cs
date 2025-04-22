using GraphicPanelWpf;
using LevelEditorGlobal;
using SpriteEditorControl.Controls.Main.Model;
using SpriteEditorControl.Controls.Sprite;
using SpriteEditorControl.Controls.Sprite.Model;
using PhysicItemEditorControl.Model;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Forms;
using WpfControls.Model;

namespace SpriteEditorControl.Controls.Main
{
    internal class SpriteEditorViewModel : ReactiveObject, IGraphicPanelHandler, ITimerHandler, IToTextWriteable, IObjectSerializable
    {
        public ReactiveCommand<Unit, Unit> SaveClick { get; private set; } //LevelItem speichern
        public ReactiveCommand<Unit, Unit> LoadClick { get; private set; } //LevelItem laden
        public ReactiveCommand<Unit, Unit> GoBackClick { get; private set; } //Zurück zum Spiel
        [Reactive] public bool ShowSaveLoadButtons { get; private set; }
        [Reactive] public bool ShowGoBackButton { get; private set; }
        public ObservableCollection<TabItemViewModel> Tabs { get; set; } = new ObservableCollection<TabItemViewModel>();
        [Reactive] public TabItemViewModel SelectedTab { get; set; }

        public SpriteEditorViewModel(EditorInputData data)
        {
            ShowSaveLoadButtons = data.ShowSaveLoadButtons;
            ShowGoBackButton = data.ShowGoBackButton;

            SaveClick = ReactiveCommand.Create(() =>
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    WriteToTextFile(saveFileDialog.FileName);

            },
            this.WhenAnyValue(x => x.ShowSaveLoadButtons) //CanExecute: Zeige den Savebutton nur, wenn IsLoaded true ist
            );
            LoadClick = ReactiveCommand.Create(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                    LoadFromTextFile(openFileDialog.FileName);
            },
            this.WhenAnyValue(x => x.ShowSaveLoadButtons) //CanExecute: Zeige den Loadbutton nur, wenn IsLoaded true ist
            );
            this.GoBackClick = ReactiveCommand.Create(() =>
            {
                data.IsFinished?.Invoke(this);
            });

            //So kann man mit Reavtive sowohl den alten als auch neuen Wert bei Änderung bekommen:
            //https://stackoverflow.com/questions/29100381/getting-prior-value-on-change-of-property-using-reactiveui-in-wpf-mvvm
            this.WhenAnyValue(x => x.SelectedTab)
                .Buffer(2, 1)
                .Select(b => (Previous: b[0], Current: b[1]))
                .Subscribe(x =>
                {
                    //Es wurde vom PhysicEditor in den SpriteExport-Editor gewechselt
                    if (x.Previous?.Type == TabItemViewModel.TabItemType.PhysicEditor && x.Current?.Type == TabItemViewModel.TabItemType.SpriteEditor)
                    {
                        var physicData = (PhysicItemExportData)(Tabs[0].Content.DataContext as IObjectSerializable).GetExportObject();
                        var spriteVm = (SpriteViewModel)Tabs[1].Content.DataContext;
                        spriteVm.UpdatePhysicData(physicData);
                    }
                });


            CreateTabs(data);
        }

        private void CreateTabs(EditorInputData data)
        {
            Tabs.Clear();

            var physicItemFactory = new PhysicEditorFactory(() => 1, false, false);
            var physicItemControl = physicItemFactory.CreateEditorControl(new EditorInputData()
            {
                DataFolder = data.DataFolder,
                Panel = data.Panel,
                TimerTickRateInMs = data.TimerTickRateInMs,
                ShowSaveLoadButtons = false
            });
            var physicItemTab = new TabItemViewModel("PhysicItem", TabItemViewModel.TabItemType.PhysicEditor, physicItemControl);
            Tabs.Add(physicItemTab);

            var spriteExportControl = new SpriteControl(data.Panel) { DataContext = new SpriteViewModel(data.Panel, data.DataFolder) };
            var spriteTab = new TabItemViewModel("SpriteExport", TabItemViewModel.TabItemType.SpriteEditor, spriteExportControl);
            Tabs.Add(spriteTab);
        }

        public SpriteData GetSpriteDataFromAnimationTab(int animationTabIndex)
        {
            var spriteEditor = (SpriteViewModel)Tabs[1].Content.DataContext;
            var physicData = (PhysicItemExportData)(Tabs[0].Content.DataContext as IObjectSerializable).GetExportObject();
            spriteEditor.UpdatePhysicData((PhysicItemExportData)physicData);
            spriteEditor.SelectedAnimationIndex = animationTabIndex;

            return spriteEditor.GetSpriteData();
        }

        #region IGraphicPanelHandler, ITimerHandler
        public void HandleTimerTick(float dt)
        {
            if (SelectedTab.Content.DataContext is ITimerHandler)
                (SelectedTab.Content.DataContext as ITimerHandler).HandleTimerTick(dt);
        }
        public void HandleMouseClick(MouseEventArgs e)
        {
            if (SelectedTab.Content.DataContext is IGraphicPanelHandler)
                (SelectedTab.Content.DataContext as IGraphicPanelHandler).HandleMouseClick(e);
        }
        public void HandleMouseWheel(MouseEventArgs e)
        {
            if (SelectedTab.Content.DataContext is IGraphicPanelHandler)
                (SelectedTab.Content.DataContext as IGraphicPanelHandler).HandleMouseWheel(e);
        }
        public void HandleMouseMove(MouseEventArgs e)
        {
            if (SelectedTab.Content.DataContext is IGraphicPanelHandler)
                (SelectedTab.Content.DataContext as IGraphicPanelHandler).HandleMouseMove(e);
        }
        public void HandleMouseDown(MouseEventArgs e)
        {
            if (SelectedTab.Content.DataContext is IGraphicPanelHandler)
                (SelectedTab.Content.DataContext as IGraphicPanelHandler).HandleMouseDown(e);
        }
        public void HandleMouseUp(MouseEventArgs e)
        {
            if (SelectedTab.Content.DataContext is IGraphicPanelHandler)
                (SelectedTab.Content.DataContext as IGraphicPanelHandler).HandleMouseUp(e);
        }
        public void HandleMouseEnter()
        {
            if (SelectedTab.Content.DataContext is IGraphicPanelHandler)
                (SelectedTab.Content.DataContext as IGraphicPanelHandler).HandleMouseEnter();
        }
        public void HandleMouseLeave()
        {
            if (SelectedTab.Content.DataContext is IGraphicPanelHandler)
                (SelectedTab.Content.DataContext as IGraphicPanelHandler).HandleMouseLeave();
        }
        public void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (SelectedTab.Content.DataContext is IGraphicPanelHandler)
                (SelectedTab.Content.DataContext as IGraphicPanelHandler).HandleKeyDown(e);
        }
        public void HandleKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            if (SelectedTab?.Content.DataContext is IGraphicPanelHandler)
                (SelectedTab.Content.DataContext as IGraphicPanelHandler).HandleKeyUp(e);
        }
        public void HandleSizeChanged(int width, int height)
        {
            if (SelectedTab.Content.DataContext is IGraphicPanelHandler)
                (SelectedTab.Content.DataContext as IGraphicPanelHandler).HandleSizeChanged(width, height);
        }

        #endregion

        #region IObjectSerializable
        public object GetExportObject()
        {
            return GetExportData();
        }

        public void LoadFromExportObject(object exportObject)
        {
            var data = (SpriteEditorExportData)exportObject;
            (Tabs[0].Content.DataContext as IObjectSerializable).LoadFromExportObject(data.PhysicItemData);
            (Tabs[1].Content.DataContext as IObjectSerializable).LoadFromExportObject(data.SpriteData);

            //Wenn das Sprite-Tab gerade aktiv ist, dann aktualisiere die Spritedaten beim Laden
            if (this.SelectedTab == Tabs[1])
            {
                var spriteVm = (SpriteViewModel)Tabs[1].Content.DataContext;
                spriteVm.UpdatePhysicData(data.PhysicItemData);
            }
            
        }

        private SpriteEditorExportData GetExportData()
        {            
            return new SpriteEditorExportData()
            {
                PhysicItemData = (PhysicItemExportData)(Tabs[0].Content.DataContext as IObjectSerializable).GetExportObject(),
                SpriteData = (SpriteExportData)(Tabs[1].Content.DataContext as IObjectSerializable).GetExportObject(),
            };
        }


        #endregion

        #region IToTextWriteable
        public void WriteToTextFile(string filePath)
        {
            FileNameReplacer.SaveEditorFile(filePath, JsonHelper.Helper.ToJson(GetExportData()));
        }
        public void LoadFromTextFile(string filePath)
        {
            var data = JsonHelper.Helper.CreateFromJson<SpriteEditorExportData>(FileNameReplacer.LoadEditorFile(filePath));
            LoadFromExportObject(data);
        }

        #endregion
    }
}
