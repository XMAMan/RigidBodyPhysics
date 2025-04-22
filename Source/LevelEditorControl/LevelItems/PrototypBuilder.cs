using LevelEditorControl.LevelItems.BackgroundItem;
using LevelEditorControl.LevelItems.GroupedItems;
using LevelEditorControl.LevelItems.PhysicItem;
using LevelEditorGlobal;
using PhysicItemEditorControl.Model;
using System;
using System.Collections.Generic;

namespace LevelEditorControl.LevelItems
{
    //Erzeugt Prototyp-Objekte und LevelItems, welche aus Prototyps bestehen
    internal static class PrototypBuilder
    {
        public static IPrototypItem BuildPrototyp(IPrototypExportData protoExport, List<IPrototypItem> prototyps)
        {
            switch (protoExport.ProtoType)
            {
                case IPrototypItem.Type.PhysicItem:
                    return PhysicPrototypItemBuilder.CreateFromObject(protoExport);

                case IPrototypItem.Type.BackgroundItem:
                    return BackgroundPrototypItem.CreateFromExportData(protoExport as BackgroundPrototypExportData);

                case IPrototypItem.Type.GroupedItem:
                    return GroupedItemPrototyp.CreateFromExportData(protoExport as GroupedItemProtoExportData, prototyps);
            }

            throw new ArgumentException("Unknown type " + protoExport.ProtoType);
        }

        public static IPrototypLevelItem BuildLevelItem(object exportData, List<IPrototypItem> prototyps)
        {
            if (exportData is PhysicLevelItemExportData)
                return LevelItems.PhysicItem.PhysicLevelItem.CreateFromExportData((PhysicLevelItemExportData)exportData, prototyps);

            if (exportData is BackgroundLevelItemExportData)
                return BackgroundLevelItem.CreateFromExportData((BackgroundLevelItemExportData)exportData, prototyps);

            if (exportData is GroupedItemLevelExportData)
                return GroupedItemsLevelItem.CreateFromExportData((GroupedItemLevelExportData)exportData, prototyps);

            throw new ArgumentException("Unknown type " + exportData.GetType());
        }
    }
}
