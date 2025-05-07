using DynamicData;
using LevelEditorExports.Editor.Prototyps;
using LevelEditorGlobal;
using LevelToSimulatorConverter._2_MergeToSingleScene;
using PhysicGlobal;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace LevelEditorControl.LevelItems.GroupedItems
{
    internal class GroupedItemPrototyp : IPrototypItem, IBackgroundItemProvider, IPhysicSceneContainer
    {
        private readonly List<IPrototypLevelItem> items;
        public GroupedItemPrototyp(List<IPrototypLevelItem> items, InitialRotatedRectangleValues initialRecValues, int id)
        {
            this.items = items.Select(x => x.CreateCopy(-1)).ToList();
            this.Id = id;
            this.BoundingBox = LevelItemsHelper.GetBoundingBox(items.Cast<ILevelItem>().ToList());
            this.InitialRecValues = initialRecValues;
        }


        public static GroupedItemPrototyp CreateFromExportData(GroupedItemProtoExportData data, List<IPrototypItem> prototyps)
        {
            var levelItems = data.LevelItemsExport.Select(x => PrototypBuilder.BuildLevelItem(x, prototyps)).ToList();

            return new GroupedItemPrototyp(levelItems, data.InitialRecValues, data.Id);
        }

        public PrototypItemType ProtoType { get => PrototypItemType.GroupedItem; }
        public int Id { get; }
        public PhysicGlobal.BoundingBox BoundingBox { get; }
        public InitialRotatedRectangleValues InitialRecValues { get; }
        public IPrototypExportData EditorExportData { get => CreateExportData(); } //Mit diesen Daten kann der Editor der dieses Item erzeugt hat dann neu geladen werden

        private GroupedItemProtoExportData CreateExportData()
        {
            return new GroupedItemProtoExportData()
            {
                Id = this.Id,
                LevelItemsExport = this.items.Select(x => x.GetExportData()).ToArray(),
                InitialRecValues = this.InitialRecValues,
            };
        }



        public Bitmap GetImage(int maxWidth, int maxHeight)
        {
            var panel = new DrawingPanel.DrawingPanel(maxWidth, maxHeight, true);
            var camera = new Camera2D(maxWidth, maxHeight, this.BoundingBox);

            panel.ClearScreen(Color.White);

            panel.MultTransformationMatrix(camera.GetPointToSceenMatrix());
            panel.EnableDepthTesting();

            foreach (var item in this.items)
            {
                item.Draw(panel);
            }
            panel.FlipBuffer();

            return panel.GetScreenShoot();
        }

        public void Draw(IDrawingPanel panel)
        {
            panel.PushMatrix();
            panel.MultTransformationMatrix(Matrix4x4.Translate(-this.BoundingBox.X, -this.BoundingBox.Y, 0));
            foreach (var item in this.items)
            {
                item.Draw(panel);
            }
            panel.PopMatrix();
        }
        public void DrawBorder(IDrawingPanel panel, Pen borderPen)
        {
            panel.PushMatrix();
            panel.MultTransformationMatrix(Matrix4x4.Translate(-this.BoundingBox.X, -this.BoundingBox.Y, 0));
            foreach (var item in this.items)
            {
                item.DrawBorder(panel, borderPen);
            }
            panel.PopMatrix();
        }

        public void DrawWithTwoColors(IDrawingPanel panel, Color frontColor, Color backColor)
        {
            panel.PushMatrix();
            panel.MultTransformationMatrix(Matrix4x4.Translate(-this.BoundingBox.X, -this.BoundingBox.Y, 0));
            foreach (var item in this.items)
            {
                item.DrawWithTwoColors(panel, frontColor, backColor);
            }
            panel.PopMatrix();
        }

        #region IBackgroundItemProvider
        public IBackgroundItem[] GetBackgroundItems()
        {
            List<IBackgroundItem> backgroundItems = new List<IBackgroundItem>();

            backgroundItems.AddRange(this.items
                .Where(x => x is IBackgroundItem)
                .Cast<IBackgroundItem>());

            backgroundItems.AddRange(this.items
                .Where(x => x is IBackgroundItemProvider)
                .Cast<IBackgroundItemProvider>()
                .SelectMany(x => x.GetBackgroundItems()));

            return backgroundItems.ToArray();

        }
        #endregion

        #region IPhysicSceneContainer
        public IMergeablePhysicScene[] GetPhysicMergerItems()
        {
            List<IMergeablePhysicScene> physicItems = new List<IMergeablePhysicScene>();

            physicItems.AddRange(this.items
                .Where(x => x is IMergeablePhysicScene)
                .Cast<IMergeablePhysicScene>());

            physicItems.AddRange(this.items
                .Where(x => x is IPhysicSceneContainer)
                .Cast<IPhysicSceneContainer>()
                .SelectMany(x => x.GetPhysicMergerItems()));

            return physicItems.ToArray();
        }
        #endregion
    }
}
