using LevelEditorControl.LevelItems;
using LevelEditorGlobal;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LevelEditorControl.Controls.TreeControl
{
    internal interface ITreeItem
    {
        ObservableCollection<ITreeItem> Items { get; set; }
        void UpdateCheckboxSelectionState(IPrototypItem selectedPrototyp, List<ILevelItem> selectedLevelItems);
        bool IsSelected { get; set; } //Blauer Selektionshintergrund vom Knoten
        string Title { get; }
        ITagable Tagable { get; }
    }

    internal class BaseTreeItem : ReactiveObject
    {
        public string Title { get; set; }
        [Reactive] public bool IsSelected { get; set; } //Wird vom TreeView geschrieben
        [Reactive] public bool IsSelected1 { get; set; } //Wird von UpdateSelectionState geschrieben, wenn jemand im Editor ein Item selektiert
        public ObservableCollection<ITreeItem> Items { get; set; } = new ObservableCollection<ITreeItem>();
        public ITagable Tagable { get; protected set; }
    }

    internal class ProtoTreeItem : BaseTreeItem, ITreeItem
    {
        private IPrototypItem prototypItem;
       
        public ProtoTreeItem(IPrototypItem prototypItem)
        {
            this.Tagable = prototypItem as ITagable;        //Bei Prototyps die nicht ITagable sind soll hier null rauskommen
            this.prototypItem = prototypItem;
            this.Title = "Proto_" + prototypItem.Id;
        }

        public void UpdateCheckboxSelectionState(IPrototypItem selectedPrototyp, List<ILevelItem> selectedLevelItems)
        {
            this.IsSelected1 = this.prototypItem == selectedPrototyp;
        }
    }

    internal class LevelItemTreeItem : BaseTreeItem, ITreeItem
    {
        private ILevelItem levelItem;
        public LevelItemTreeItem(ILevelItem levelItem)
        {
            this.Tagable = levelItem as ITagable; //Bei LevelItems die nicht ITagable sind soll hier null rauskommen
            this.levelItem = levelItem;
            this.Title = "Level_" + levelItem.Id;
        }

        public void UpdateCheckboxSelectionState(IPrototypItem selectedPrototyp, List<ILevelItem> selectedLevelItems)
        {
            this.IsSelected1 = selectedLevelItems.Contains(this.levelItem);
        }
    }

    internal class RigidBodyTreeItem : BaseTreeItem, ITreeItem
    {
        public RigidBodyTreeItem(ILevelItem parentLevelItem, ITagable body)
        {
            this.Tagable = body;
            this.Title = this.Tagable.TypeName + "_" + parentLevelItem.Id + "_" + body.Id;
        }

        //Ein Body kann nicht selektiert werden
        public void UpdateCheckboxSelectionState(IPrototypItem selectedPrototyp, List<ILevelItem> selectedLevelItems)
        {
        }
    }
}
