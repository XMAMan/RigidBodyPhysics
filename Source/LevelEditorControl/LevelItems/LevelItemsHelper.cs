using LevelEditorControl.Controls.PolygonControl;
using LevelEditorControl.LevelItems.BackgroundItem;
using LevelEditorControl.LevelItems.GroupedItems;
using LevelEditorExports.Editor.BackgroundImage;
using LevelEditorExports.Editor.Prototyps;
using LevelEditorGlobal;
using PhysicGlobal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using WpfControls.Model;

namespace LevelEditorControl.LevelItems
{
    internal static class LevelItemsHelper
    {
        public static PhysicGlobal.BoundingBox GetBoundingBox(List<ILevelItem> items)
        {
            return BoundingBox.GetBoxFromBoxes(items.Select(x => x.GetBoundingBox()));
        }

        public static void DrawItems(List<ILevelItem> items, IMouseClickable selectedSubItem, IDrawingPanel panel, Camera2D camera, PolygonImages polygonImages, MouseGrid grid)
        {
            panel.ClearScreen(Color.White);
            if (string.IsNullOrEmpty(polygonImages.BackgroundImage) == false)
            {
                var s = polygonImages.Background.Size;

                switch (polygonImages.BackgroundImageMode)
                {
                    case ImageMode.StretchWithoutAspectRatio:
                        panel.DrawFillRectangle(polygonImages.BackgroundImage, 0, 0, panel.Width, panel.Height, false, Color.White);
                        break;

                    case ImageMode.StretchWithAspectRatio:                        
                        float factor = Camera2D.GetScaleFactor(panel.Size, s);
                        panel.DrawFillRectangle(polygonImages.BackgroundImage, 0, 0, s.Width * factor, s.Height * factor, false, Color.White);
                        break;

                    case ImageMode.NoStretch:
                        panel.DrawFillRectangle(polygonImages.BackgroundImage, 0, 0, s.Width, s.Height, false, Color.White);
                        break;
                }
            }


            panel.MultTransformationMatrix(camera.GetPointToSceenMatrix());

            panel.EnableDepthTesting();

            if (grid.ShowGrid)
            {
                panel.ZValue2D = -1; //Grid soll vor dem BackgroundPolygonen liegen aber hinter den Physic/Backgrounditems
                grid.Draw(panel, camera.LengthToCamera(1), new Vec2D(camera.X, camera.Y));
            }


            foreach (var item in items)
            {
                item.Draw(panel);

                if (item.IsSelected)
                {
                    //LevelItemsHelper.DrawBoundingBox(item, panel, camera);
                    item.DrawBorder(panel, new Pen(Color.Red, 3));
                }

                if (item is IMouseClickable && item == selectedSubItem)
                {
                    item.DrawBorder(panel, new Pen(Color.Red, 3));
                }

                if (item is ITagableContainer)
                {
                    var subItems = ((ITagableContainer)item).Tagables;

                    foreach (var item1 in subItems)
                    {
                        item1.Draw(panel);

                        if (item1 == selectedSubItem)
                        {
                            item1.DrawBorder(panel, new Pen(Color.Red, 3));
                        }
                    }
                }
            }
        }

        public static void DrawItemsWithTwoColors(List<ILevelItem> items, IDrawingPanel panel, Color frontColor, Color backColor)
        {
            foreach (var item in items)
            {
                item.DrawWithTwoColors(panel, frontColor, backColor);
            }
        }

        public static void DrawBoundingBox(ILevelItem item, IDrawingPanel panel, Camera2D camera)
        {
            var box = item.GetBoundingBox();
            var min = camera.PointToScreen(new Vec2D(box.Min.X, box.Min.Y));
            var size = new SizeF(camera.LengthToScreen(box.GetWidth()), camera.LengthToScreen(box.GetHeight()));
            panel.DrawRectangle(new Pen(Color.Red, 3), (int)min.X, (int)min.Y, (int)size.Width, (int)size.Height);
        }

        public static ILevelItem BuildFromPrototyp(IPrototypItem item, Vec2D position, int id)
        {
            switch (item.ProtoType)
            {
                case PrototypItemType.PhysicItem:
                    return new LevelItems.PhysicItem.PhysicLevelItem(item, position, item.InitialRecValues, id);

                case PrototypItemType.BackgroundItem:
                    return new BackgroundLevelItem(item as BackgroundPrototypItem, position, item.InitialRecValues, id);

                case PrototypItemType.GroupedItem:
                    return new GroupedItemsLevelItem(item as GroupedItemPrototyp, position, item.InitialRecValues, id);
            }

            throw new ArgumentException("Unknown type " + item.GetType());
        }
    }
}
