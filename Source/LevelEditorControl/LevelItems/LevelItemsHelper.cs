using GraphicMinimal;
using GraphicPanels;
using LevelEditorControl.Controls.PolygonControl;
using LevelEditorControl.LevelItems.BackgroundItem;
using LevelEditorControl.LevelItems.GroupedItems;
using LevelEditorGlobal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using WpfControls.Controls.CameraSetting;
using WpfControls.Model;

namespace LevelEditorControl.LevelItems
{
    internal static class LevelItemsHelper
    {
        public static RectangleF GetBoundingBox(List<ILevelItem> items)
        {
            return GetBoundingBox(items.Select(x => x.GetBoundingBox()).ToList());
        }

        public static RectangleF GetBoundingBox(List<RectangleF> boxes)
        {
            if (boxes.Any() == false) return new RectangleF(0, 0, 0, 0);

            Vector2D min = new Vector2D(float.MaxValue, float.MaxValue);
            Vector2D max = new Vector2D(float.MinValue, float.MinValue);
            foreach (var box in boxes)
            {
                min.X = Math.Min(min.X, box.X);
                min.Y = Math.Min(min.Y, box.Y);
                max.X = Math.Max(max.X, box.X + box.Width);
                max.Y = Math.Max(max.Y, box.Y + box.Height);
            }

            return new RectangleF(min.X, min.Y, max.X - min.X, max.Y - min.Y);
        }

        public static RectangleF CreateRectangle(Vector2D mouseDownPoint, Vector2D mousePosition)
        {
            Vector2D min = new Vector2D(Math.Min(mouseDownPoint.X, mousePosition.X), Math.Min(mouseDownPoint.Y, mousePosition.Y));
            Vector2D max = new Vector2D(Math.Max(mouseDownPoint.X, mousePosition.X), Math.Max(mouseDownPoint.Y, mousePosition.Y));

            var range = max - min;
            return new RectangleF(min.X, min.Y, range.X, range.Y);
        }
        public static bool IsPointInRectangle(RectangleF rec, Vector2D point)
        {
            return point.X >= rec.Left && point.X <= rec.Right && point.Y >= rec.Top && point.Y <= rec.Bottom;
        }

        public static void DrawItems(List<ILevelItem> items, IMouseClickable selectedSubItem, GraphicPanel2D panel, Camera2D camera, PolygonImages polygonImages, MouseGrid grid)
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
                grid.Draw(panel, camera.LengthToCamera(1), new Vector2D(camera.X, camera.Y));
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

        public static void DrawItemsWithTwoColors(List<ILevelItem> items, GraphicPanel2D panel, Color frontColor, Color backColor)
        {
            foreach (var item in items)
            {
                item.DrawWithTwoColors(panel, frontColor, backColor);
            }
        }

        public static void DrawBoundingBox(ILevelItem item, GraphicPanel2D panel, Camera2D camera)
        {
            var box = item.GetBoundingBox();
            var min = camera.PointToScreen(new PointF(box.X, box.Y));
            var size = new SizeF(camera.LengthToScreen(box.Width), camera.LengthToScreen(box.Height));
            panel.DrawRectangle(new Pen(Color.Red, 3), (int)min.X, (int)min.Y, (int)size.Width, (int)size.Height);
        }

        public static ILevelItem BuildFromPrototyp(IPrototypItem item, Vector2D position, int id)
        {
            switch (item.ProtoType)
            {
                case IPrototypItem.Type.PhysicItem:
                    return new LevelItems.PhysicItem.PhysicLevelItem(item, position, item.InitialRecValues, id);

                case IPrototypItem.Type.BackgroundItem:
                    return new BackgroundLevelItem(item as BackgroundPrototypItem, position, item.InitialRecValues, id);

                case IPrototypItem.Type.GroupedItem:
                    return new GroupedItemsLevelItem(item as GroupedItemPrototyp, position, item.InitialRecValues, id);
            }

            throw new ArgumentException("Unknown type " + item.GetType());
        }
    }
}
