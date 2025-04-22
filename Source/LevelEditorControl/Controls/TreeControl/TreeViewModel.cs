using DynamicData;
using DynamicData.Binding;
using LevelEditorControl.EditorFunctions;
using LevelEditorControl.LevelItems;
using LevelEditorGlobal;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace LevelEditorControl.Controls.TreeControl
{
    //Mit diesen Control kann man sehen, welches Prototyp- oder Levelitem gerade selektiert ist und man kann auch über den Baum den Selektionsstatus ändern
    //Der aktuell blau selektierte Baumknoten ist über die SelectedNode-Property zu sehen

    //https://wpf-tutorial.com/treeview-control/treeview-data-binding-multiple-templates/
    internal class TreeViewModel : ReactiveObject
    {
        public ObservableCollection<ITreeItem> Items { get; set; } = new ObservableCollection<ITreeItem>();

        [Reactive] public ITreeItem SelectedNode { get; set; } //Dieser Knoten wurde gerade selektiert (Wird blau angezeigt; Das Item aus dem Baum wo IsSelected==true ist)

        private EditorState state;

        public TreeViewModel(EditorState state)
        {
            this.state = state;

            //Wenn sich an der Anzahl der Prototyps oder LevelItems sich was ändert, dann soll der Baum aktualisiert werden
            state.Prototyps.ToObservableChangeSet().Subscribe(x =>
            {
                BuildTree();
            });

            state.LevelItems.ToObservableChangeSet().Subscribe(x =>
            {
                BuildTree();
            });

            //Wenn die MoveAndSelect-Funktion ein Objekt selektiert, dann soll auch im Baum der Knoten selektiert werden
            state.SelectedItems.Connect().Subscribe(x =>
            {
                UpdateSelectionStateFromTree();
            });

            //Wenn im PrototypControl ein Item selektiert wird, dann soll auch im Baum der Knoten selektiert werden
            state.WhenAnyValue(x => x.SelectedPrototyp).Subscribe(x =>
            {
                UpdateSelectionStateFromTree(this.Items);
            });

            this.WhenAnyValue(x => x.SelectedNode).Subscribe(x =>
            {
                if (x == null || x.Tagable == null)
                {
                    this.state.SelectedSubItem = null;
                }
                else if (x.Tagable is IMouseClickable)
                {
                    this.state.SelectedSubItem = (IMouseClickable)x.Tagable;
                }

            });
        }

        public void BuildTree()
        {
            Items.Clear();

            var protoLevelItems = state.LevelItems
                    .Where(x => x is IPrototypLevelItem)
                    .Cast<IPrototypLevelItem>()
                    .ToList();

            foreach (var proto in state.Prototyps)
            {
                var levelItems = protoLevelItems
                    .Where(x => x.AssociatedPrototyp == proto)
                    .ToList();

                var protoNode = CreatePrototypNode(proto); //Elternknoten

                foreach (var item in levelItems) //Alle Levelitems die am ProtoNode hängen sind die Kindknoten
                {
                    var levelNode = CreateLevelItemNode(item);

                    if (item is ITagableContainer) //Wenn das LevelItem ein PhysicLevelItem ist, dann hänge daran noch die RigidBodys als Kindknoten dran
                    {
                        levelNode.Items.AddRange(CreateRigidBodyNodes((ITagableContainer)item));
                    }

                    protoNode.Items.Add(levelNode);
                }

                Items.Add(protoNode);
            }

            var noProto = state.LevelItems
                    .Where(x => (x is IPrototypLevelItem) == false)
                    .ToList();

            foreach (var item in noProto)
            {
                var levelNode = CreateLevelItemNode(item); //Alle Polygon-LevelItems

                Items.Add(levelNode);
            }
        }

        private ProtoTreeItem CreatePrototypNode(IPrototypItem proto)
        {
            var protoNode = new ProtoTreeItem(proto);

            //Jemand klickt im Baum ein Prototyp-Knoten an
            protoNode.WhenAnyValue(x => x.IsSelected).Subscribe(isSelected =>
            {
                //Ändere im EditorState die SelectedPrototyp-Property
                if (isSelected)
                    this.state.SelectedPrototyp = proto;
                else
                    this.state.SelectedPrototyp = null;

                this.SelectedNode = isSelected ? protoNode : null;
            });

            return protoNode;
        }

        private LevelItemTreeItem CreateLevelItemNode(ILevelItem item)
        {
            var levelNode = new LevelItemTreeItem(item);

            //Jemand klickt im Baum ein Levelitem-Knoten an
            levelNode.WhenAnyValue(x => x.IsSelected).Subscribe(isSelected =>
            {
                //Ändere im EditorState SelectedItems und die IsSelected-Property im LevelItem
                item.IsSelected = isSelected;

                if (isSelected && this.state.SelectedItems.Items.Contains(item) == false)
                    this.state.SelectedItems.Add(item);

                if (isSelected == false && this.state.SelectedItems.Items.Contains(item))
                    this.state.SelectedItems.Remove(item);

                this.SelectedNode = isSelected ? levelNode : null;
            });

            //Jemand klickt im Baum auf die Selected1-Checkbox -> Ändere im EditorState SelectedItems und die IsSelected-Property im LevelItem
            levelNode.WhenAnyValue(x => x.IsSelected1).Subscribe(isSelected =>
            {
                item.IsSelected = isSelected;

                if (isSelected && this.state.SelectedItems.Items.Contains(item) == false)
                    this.state.SelectedItems.Add(item);

                if (isSelected == false && this.state.SelectedItems.Items.Contains(item))
                    this.state.SelectedItems.Remove(item);
            });

            return levelNode;
        }

        private RigidBodyTreeItem[] CreateRigidBodyNodes(ITagableContainer tagContainer)
        {
            List< RigidBodyTreeItem > nodes = new List<RigidBodyTreeItem> ();
            foreach (var body in tagContainer.Tagables)
            {
                var bodyNode = new RigidBodyTreeItem((ILevelItem)tagContainer, body);

                //Jemand klickt im Baum ein BodyItem-Knoten an
                bodyNode.WhenAnyValue(x => x.IsSelected).Subscribe(isSelected =>
                {
                    this.SelectedNode = isSelected ? bodyNode : null;
                });

                nodes.Add(bodyNode);
            }

            return nodes.ToArray();
        }

        private void UpdateSelectionStateFromTree()
        {
            UpdateSelectionStateFromTree(this.Items);
            
            if (state.SelectedItems.Count == 0)
            {
                UnselectAllTreeItems(this.Items);
            }
        }

        private void UpdateSelectionStateFromTree(ObservableCollection<ITreeItem> items)
        {
            var selectedProto = state.SelectedPrototyp;
            var selectedItems = state.SelectedItems.Items.ToList();

            foreach (var item in items)
            {
                item.UpdateCheckboxSelectionState(selectedProto, selectedItems);
                UpdateSelectionStateFromTree(item.Items);
            }
        }

        private void UnselectAllTreeItems(ObservableCollection<ITreeItem> items)
        {
            foreach (var item in items)
            {
                item.IsSelected = false;
                UnselectAllTreeItems(item.Items);
            }
        }

        public string GetNodeNameByTagObject(ITagable tagable)
        {
            var node = GetAllNodes().First(x => x.Tagable == tagable);
            return node.Title;
        }

        private IEnumerable<ITreeItem> GetAllNodes()
        {
            return GetAllNodes(this.Items);
        }

        private IEnumerable<ITreeItem> GetAllNodes(ObservableCollection<ITreeItem> items)
        {
            foreach (var item in items)
            {
                yield return item;
                foreach (var node in GetAllNodes(item.Items))
                {
                    yield return node;
                }
            }
        }
    }
}
