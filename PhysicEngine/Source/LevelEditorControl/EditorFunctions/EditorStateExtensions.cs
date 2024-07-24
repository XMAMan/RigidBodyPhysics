using DynamicData;
using LevelEditorControl.Controls.TagItemControl;
using LevelEditorControl.LevelItems;
using LevelEditorControl.LevelItems.LawnEdge;
using LevelEditorControl.LevelItems.Polygon;
using LevelEditorGlobal;
using System.Collections.Generic;
using System.Linq;

namespace LevelEditorControl.EditorFunctions
{
    //Kleine Helferlein für den täglichen Hausgebrauch
    internal static class EditorStateExtensions
    {
        public static int GenerateLevelItemId(this EditorState state)
        {
            int newId = state.LevelItems.Any() == false ? 1 : state.LevelItems.Max(x => x.Id) + 1;
            return newId;
        }

        public static void AddLevelItem(this EditorState state, ILevelItem item)
        {
            state.LevelItems.Add(item);

            //Dieser Aufruf sorgt dafür, dass das SmallWindow genau die ganze Scene zeigt
            state.Camera.UpdateBoundingBoxWithoutZoomChange(LevelItemsHelper.GetBoundingBox(state.LevelItems.ToList()));
        }

        public static void RemoveLevelItemsAssociatedToPrototyp(this EditorState state, IPrototypItem prototyp)
        {
            List<ILevelItem> delList = new List<ILevelItem>();

            foreach (var item in state.LevelItems)
            {
                if ((item is IPrototypLevelItem) && (item as IPrototypLevelItem).AssociatedPrototyp == prototyp)
                {
                    delList.Add(item);
                }

                RemoveFromTagStorrage(state.TagDataStorrage, item);
            }

            RemoveLevelItemsFromEditorState(state, delList);
        }

        private static void RemoveFromTagStorrage(TagDataStorrage storrage, object item)
        {
            var tags = GetAllTagsFromObject(item);
            foreach (var tag in tags)
            {
                storrage.RemoveTagData(tag);
            }
        }

        private static ITagable[] GetAllTagsFromObject(object obj)
        {
            if (obj == null) return new ITagable[0];

            List<ITagable> tags = new List<ITagable>();
            if (obj is ITagable) tags.Add((ITagable)obj);
            if (obj is ITagableContainer)
            {
                tags.AddRange((obj as ITagableContainer).Tagables);
            }

            return tags.ToArray();
        }

        public static void RemoveLevelItemsFromEditorState(this EditorState state, IEnumerable<ILevelItem> items)
        {
            foreach (var del in items)
            {
                RemoveFromTagStorrage(state.TagDataStorrage, del); //Erst muss die Löschung aus dem TagStorrage erfolgen, da
                state.LevelItems.Remove(del);                      //da diese Zeile hier aus dem TreeViewModel die Objekte löscht und die Daten vom Baum aber den TagName speichern, welche im Storrage dann benötigt wird
                var delList = state.KeyboradMappings.Where(x => x.LevelItemId == del.Id).ToList();
                foreach (var del1 in delList)
                {
                    state.KeyboradMappings.Remove(del1);
                }

                //Wenn ein PolygonLevelItem gelöscht werden soll, dann lösche auch die dort dranhängenden LawnEdgeLevelItems
                if (del is ILevelItemPolygon)
                {
                    var lawnEdgesToDelete = state.LevelItems.Where(x => (x is LawnEdgeLevelItem) && (x as LawnEdgeLevelItem).AssocitedPolygon == del).ToList();
                    foreach (var lawDel in lawnEdgesToDelete)
                        state.LevelItems.Remove(lawDel);
                }

                state.SelectedItems.Remove(del);

                
            }
            PolygonLevelItem.UpdateIsOutsideAndUVFromAllPolygons(state.LevelItems.Where(x => x is PolygonLevelItem).Cast<PolygonLevelItem>().ToList());
        }

        public static void UnselectAllItems(this EditorState state)
        {
            foreach (var item in state.LevelItems)
            {
                item.IsSelected = false;
            }
            state.SelectedItems.Clear();
            state.SelectedSubItem = null;
        }

        public static void SelectItems(this EditorState state, List<ILevelItem> items)
        {
            foreach (var item in items)
            {
                item.IsSelected = true;                
            }

            var selected = items.Where(x => state.SelectedItems.Items.Contains(x) == false).ToList();
            if (selected.Any())
            {
                state.SelectedItems.AddRange(selected);
            }
        }

        public static void SetSelectionState(this EditorState state, ILevelItem item, bool isSelected)
        {
            if (isSelected)
            {
                item.IsSelected = true;
                if (state.SelectedItems.Items.Contains(item) == false)
                    state.SelectedItems.Add(item);
            }
            else
            {
                item.IsSelected = false;
                if (state.SelectedItems.Items.Contains(item))
                    state.SelectedItems.Remove(item);
            }
        }


        public static void DrawItems(this EditorState state)
        {
            LevelItemsHelper.DrawItems(state.LevelItems.ToList(), state.SelectedSubItem, state.Panel, state.Camera, state.PolygonImages, state.Grid);
        }

        public static void DrawSmallWindow(this EditorState state, SmallWindow smallWindow)
        {
            if (state.ShowSmallWindow == false) return;

            smallWindow.Draw(state.Panel, (frontColor, backColor) => LevelItemsHelper.DrawItemsWithTwoColors(state.LevelItems.ToList(), state.Panel, frontColor, backColor));
        }

        public static EditorTagdata[] GetAllTagsForSimulator(this EditorState state)
        {
            //Tagdaten von den RigidBodys, Joints, Thrusters und RotaryMotors
            List<EditorTagdata> allTags = new List<EditorTagdata>();
            foreach (var levelItem in state.LevelItems)
            {
                if (levelItem is ITagableContainer && levelItem is IPrototypLevelItem)
                {
                    var proto = (levelItem as IPrototypLevelItem).AssociatedPrototyp;
                    var tags = (levelItem as ITagableContainer).Tagables;

                    var protoTagData = state.TagDataStorrage[(ITagable)proto];

                    foreach (var tag in tags)
                    {
                        var bodyTagData = state.TagDataStorrage[tag];

                        var tagData = new EditorTagdata()
                        {
                            LevelItemId = levelItem.Id,
                            TagId = tag.Id,
                            TagType = tag.TypeName,
                            PrototypTagName = protoTagData.Name,
                            PrototypColor = protoTagData.Color,
                            Name = bodyTagData.Name,
                            Color = bodyTagData.Color,
                            AnchorPoints = bodyTagData.AnchorPoints.ToArray()
                        };

                        allTags.Add(tagData);
                    }
                }else if (levelItem is IMouseclickableWithTagData)
                {
                    var tagItem = (ITagable)levelItem;
                    var bodyTagData = state.TagDataStorrage[tagItem];

                    var tagData = new EditorTagdata()
                    {
                        LevelItemId = levelItem.Id,
                        TagId = 0,
                        TagType = tagItem.TypeName,
                        PrototypTagName = null,
                        PrototypColor = 0,
                        Name = bodyTagData.Name,
                        Color = bodyTagData.Color,
                        AnchorPoints = bodyTagData.AnchorPoints.ToArray()
                    };

                    allTags.Add(tagData);
                }


            }

            return allTags.ToArray();
        }
    }
}
