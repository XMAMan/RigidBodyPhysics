using LevelEditorControl.LevelItems;
using LevelEditorGlobal;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using LevelEditorControl.EditorFunctions;

namespace LevelEditorControl.Controls.PrototypControl
{
    internal class PrototypControlActions
    {
        public Action AddPhysicItem;
        public Action<string> AddBackgroundItem;
        public Action<IPrototypItem> EditItemAction; //Wenn jemand per Rechtklick->Edit in das PrototypControl auf ein Item klickt
        public Action<IPrototypItem> CreateCopyFromItemClick; //Wenn jemand auf ein Item aus der Box Rechtsklick->ContextMenü->CreateCopy klickt
        public Action<IPrototypItem> CopyToClipboardClick;    //Wenn jemand auf ein Item aus der Box Rechtsklick->ContextMenü->Copy to Clipboard klickt
        public Action<IPrototypItem> DeleteItemAction; //Rechtsklick->Delete auf Item aus PrototypControl
        public Action<IPrototypItem> MouseDownAction; //MouseDown-Event von Items aus PrototypControl        
    }

    internal class PrototypViewModel : ReactiveObject
    {
        public ObservableCollection<PrototypItemViewModel> Items { get; set; } = new ObservableCollection<PrototypItemViewModel>();
        [Reactive] public PrototypItemViewModel SelectedItem { get; set; }

        public ReactiveCommand<Unit, Unit> AddPhysicItemClick { get; private set; }
        public ReactiveCommand<Unit, Unit> AddBackgroundItemClick { get; private set; }
        public ReactiveCommand<Unit, Unit> PasteFromClipboard { get; private set; }

        private EditorState state;
        private PrototypControlActions actions;
        

        public PrototypViewModel(EditorState state, PrototypControlActions actions)
        {
            this.state = state;
            this.actions = actions;            

            this.AddPhysicItemClick = ReactiveCommand.Create(() =>
            {
                this.actions.AddPhysicItem();
            });
            this.AddBackgroundItemClick = ReactiveCommand.Create(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "png files (*.png)|*.png|jpeg files (*.jpg)|*.jpg|bmp files (*.bmp)|*.bmp|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    this.actions.AddBackgroundItem(openFileDialog.FileName);
                }
            });

            this.actions.CreateCopyFromItemClick = (item) =>
            {
                var copy = CreateCopy(item, CreateNewId(), this.state.Prototyps.ToList());
                AddItem(copy);
            };

            this.actions.CopyToClipboardClick = (item) =>
            {
                string exportString = CreateExportString(item);

                System.Windows.Clipboard.SetText(exportString);
            };
            this.PasteFromClipboard = ReactiveCommand.Create(() =>
            {
                string exportString = System.Windows.Clipboard.GetText();

                //Geht nur wenn ich die Objekte über die CoptyToClipboard-Methode von in die Zwischenablage kopiere
                //Wenn ich aber die Objekte über GameSimulator.CopyLevelToClipboard() in die Zwischenablage kopiert habe,
                //dann muss ich PasteItemsFromExportString nutzen
                //var prototyp = CreateFromExportString(exportString, CreateNewId(), this.state.Prototyps.ToList());
                //AddItem(prototyp);

                PasteItemsFromExportString(exportString);
            });



            var deleteActionFromOutside = actions.DeleteItemAction;
            this.actions.DeleteItemAction = (item) =>
            {
                var del = this.Items.First(x => x.Item == item);
                this.Items.Remove(del); //Lösche aus der Prototyp-Box
                deleteActionFromOutside(item); //Lösche alle LevelItems die von diesen Typ sind
                this.state.Prototyps.Remove(item);
            };

            this.WhenAnyValue(x => x.SelectedItem).Subscribe(x =>
            {
                this.state.SelectedPrototyp = this.SelectedItem?.Item;
            });

            this.state.WhenAnyValue(x => x.SelectedPrototyp).Subscribe(x =>
            {
                this.SelectedItem = this.Items.FirstOrDefault(x => x.Item == this.state.SelectedPrototyp);
            });

        }

        #region Clipboard-Functions
        private static string CreateExportString(IPrototypItem prototypItem)
        {
            var exportObject = new PrototypControlExportData()
            {
                PrototypItems = new IPrototypExportData[] { prototypItem.EditorExportData }
            };
            string exportString = JsonHelper.Helper.ToJson(exportObject);

            return exportString;
        }

        //Einzelnes Objekt einfügen
        private static IPrototypItem CreateFromExportString(string exportString, int newId, List<IPrototypItem> prototyps)
        {
            var exportObject = JsonHelper.Helper.CreateFromJson<PrototypControlExportData>(exportString);
            if (exportObject.PrototypItems.Length != 1) throw new Exception("Use this function only for pasting single objects");
            exportObject.PrototypItems[0].Id = newId;
            var prototyp = PrototypBuilder.BuildPrototyp(exportObject.PrototypItems[0], prototyps);
            return prototyp;
        }

        //Wenn aus dem Spiel das gesamte Level mit GameSimulator.CopyLevelToClipboard() in die Zwischenablage kopiert wurde:
        private void PasteItemsFromExportString(string exportString)
        {
            var exportObject = JsonHelper.Helper.TryToCreateFromJson<PrototypControlExportData>(exportString);
            if (exportObject == null) return;

            List<IPrototypItem> prototyps = new List<IPrototypItem>();
            prototyps.AddRange(this.state.Prototyps.ToList());

            foreach (var  item in exportObject.PrototypItems)
            {
                item.Id = CreateNewId();
                var prototyp = PrototypBuilder.BuildPrototyp(item, prototyps);
                prototyps.Add(prototyp);
                AddItem(prototyp);
            }            
        }

        private static IPrototypItem CreateCopy(IPrototypItem prototypItem, int newId, List<IPrototypItem> prototyps)
        {
            var exportString = CreateExportString(prototypItem);
            var copy = CreateFromExportString(exportString, newId, prototyps);
            return copy;
        }
        #endregion

        public void AddItem(IPrototypItem item)
        {
            this.Items.Add(new PrototypItemViewModel(item, this.actions));
            this.state.Prototyps.Add(item);
        }

        //Das PhysicLevelItem wurde im Editor bearbeitet wodurch ein neues IPrototypItem-Objekt erzeugt wurde
        //oldItem = Dieses Objekt war der Input für den Editor; newItem = Dieses Objekt hat der Editor daraus erzeugt
        public void UpdateItem(IPrototypItem oldItem, IPrototypItem newItem)
        {
            this.Items.First(x => x.Item == oldItem).UpdateItem(newItem);
            this.state.Prototyps.Remove(oldItem);
            this.state.Prototyps.Add(newItem);
        }

        internal PrototypControlExportData GetExportData()
        {
            return new PrototypControlExportData() { PrototypItems = this.Items.Select(x => x.Item.EditorExportData).ToArray() };
        }

        internal void LoadExportData(PrototypControlExportData exportData)
        {
            this.state.Prototyps.Clear();
            this.Items.Clear();
            List<IPrototypItem> prototyps = new List<IPrototypItem>(); //Wird für die Erstellung des GroupedItem benötigt. Es verweist auf Items, die weiter vorne in der Protobox kommen
            foreach (var protoExport in exportData.PrototypItems)
            {
                var prototyp = PrototypBuilder.BuildPrototyp(protoExport, prototyps);
                prototyps.Add(prototyp);
                AddItem(prototyp);
            }
        }

        public int CreateNewId()
        {
            if (this.Items.Count == 0) return 1;

            return this.Items.Max(x => x.Item.Id) + 1;
        }
    }
}
