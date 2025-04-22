using LevelEditorControl.EditorFunctions;
using LevelEditorGlobal;
using System;
using System.Linq;

namespace LevelEditorControl.LevelItems.GroupedItems
{
    //Erzeugt ein neues Objekt indem es alle aktuell selektierten Objekte zusammenfasst
    //Es können nur Items aus der Prototypbox gruppiert werden (Keine Polygone) da bei Polygonen
    //das IsOutside-Flag aktualisiert werden muss und man dazu an alle Polygone der Szene ran kommen muss.
    //Wenn das Polygon aber nicht mehr direkt in der EditorState.LevelItems-Liste steckt sondern ein Unterobjekt von ein
    //GroupedItemsLevelItem ist, dann kommt man an das Polygon nicht mehr so leicht ran.
    internal static class GroupedItemBuilder
    {
        public static GroupedItemPrototyp BuildPrototypFromSelectedItems(EditorState state, int newPrototypId)
        {
            if (state.SelectedItems.Count <= 1 || state.SelectedItems.Items.All(x => x is IPrototypLevelItem) == false)
                throw new ArgumentException("There must be multiple Prototypitems selected");

            var items = state.SelectedItems.Items.Cast<IPrototypLevelItem>().ToList();
            var prototyp = new GroupedItemPrototyp(items, new InitialRotatedRectangleValues(), newPrototypId);
            return prototyp;
        }
    }
}
