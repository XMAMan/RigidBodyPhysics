using GraphicMinimal;
using GraphicPanels;
using LevelEditorGlobal;
using LevelToSimulatorConverter;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace LevelEditorControl.LevelItems.GroupedItems
{
    internal class GroupedItemsLevelItem : IPrototypLevelItem, IRotateableLevelItem, IBackgroundItemProvider, IPhysicSceneContainer
    {
        public GroupedItemsLevelItem(GroupedItemPrototyp prototyp, Vector2D position, InitialRotatedRectangleValues initialRecValues, int id)
        {
            this.AssociatedPrototyp = prototyp;
            this.Id = id;
            this.RotatedRectangle = new RotatedRectangle(position, prototyp.BoundingBox.Size, initialRecValues);
        }

        public IPrototypItem AssociatedPrototyp { get; private set; }
        public void UpdateAfterPrototypWasChanged(IPrototypItem oldItem, IPrototypItem newItem)
        {
            this.AssociatedPrototyp = newItem;
        }

        public int Id { get; }
        public bool IsSelected { get; set; }
        public Vector2D PivotPoint { get => this.RotatedRectangle.PivotPoint; set => this.RotatedRectangle.PivotPoint = value; }
        public RotatedRectangle RotatedRectangle { get; }
        public RectangleF GetBoundingBox()
        {
            return this.RotatedRectangle.GetBoundingBox();
        }
        public Vector2D[] GetCornerPoints()
        {
            return this.RotatedRectangle.GetCornerPoints();
        }
        public float GetArea()
        {
            var protoBox = AssociatedPrototyp.BoundingBox;
            return protoBox.Width * protoBox.Height;
        }

        public void Draw(GraphicPanel2D panel)
        {
            panel.PushMatrix();
            panel.MultTransformationMatrix(this.RotatedRectangle.GetLocalToScreenMatrix());
            this.AssociatedPrototyp.Draw(panel);
            panel.PopMatrix();
        }

        public void DrawBorder(GraphicPanel2D panel, Pen borderPen)
        {
            panel.PushMatrix();
            panel.MultTransformationMatrix(this.RotatedRectangle.GetLocalToScreenMatrix());
            this.AssociatedPrototyp.DrawBorder(panel, borderPen);
            panel.PopMatrix();
        }
        public void DrawWithTwoColors(GraphicPanel2D panel, Color frontColor, Color backColor)
        {
            panel.PushMatrix();
            panel.MultTransformationMatrix(this.RotatedRectangle.GetLocalToScreenMatrix());
            this.AssociatedPrototyp.DrawWithTwoColors(panel, frontColor, backColor);
            panel.PopMatrix();
        }
        public bool IsPointInside(Vector2D point)
        {
            return this.RotatedRectangle.IsPointInside(point);
        }
        public bool IsPointInside(Vector2D point, Matrix4x4 screenToLocal) //point = ScreenSpace-Mousepoint
        {
            point = Matrix4x4.MultPosition(screenToLocal, new Vector3D(point.X, point.Y, 0)).XY; //CameraSpace-Mousepoint
            return IsPointInside(point);
        }

        public IPrototypLevelItem CreateCopy(int newId)
        {
            var export = (GroupedItemLevelExportData)GetExportData();
            export.LevelItemId = newId;
            return CreateFromExportData(export, new List<IPrototypItem>() { this.AssociatedPrototyp });
        }

        #region IObjectSerializable
        public object GetExportData()
        {
            return new GroupedItemLevelExportData()
            {
                LevelItemId = Id,
                PrototypId = AssociatedPrototyp.Id,
                Position = PivotPoint,
                SizeFactor = this.RotatedRectangle.SizeFactor,
                AngleInDegree = this.RotatedRectangle.AngleInDegree,
                LocalPivot = this.RotatedRectangle.LocalPivot
            };
        }

        public static GroupedItemsLevelItem CreateFromExportData(GroupedItemLevelExportData data, List<IPrototypItem> prototyps)
        {
            var proto = prototyps.First(x => x.Id == data.PrototypId);
            if (data.SizeFactor == 0) data.SizeFactor = 1;
            if (data.LocalPivot == null) data.LocalPivot = new Vector2D(0, 0);
            var initialRecData = new InitialRotatedRectangleValues()
            {
                SizeFactor = data.SizeFactor,
                AngleInDegree = data.AngleInDegree,
                LocalPivot = data.LocalPivot
            };
            return new GroupedItemsLevelItem((GroupedItemPrototyp)proto, data.Position, initialRecData, data.LevelItemId);
        }
        #endregion

        #region IBackgroundItemProvider
        public IBackgroundItem[] GetBackgroundItems()
        {
            var protoBox = this.AssociatedPrototyp.BoundingBox;
            var matrix = Matrix4x4.Translate(-protoBox.X, -protoBox.Y, 0) * this.RotatedRectangle.GetLocalToScreenMatrix();

            return (this.AssociatedPrototyp as IBackgroundItemProvider)
                .GetBackgroundItems()
                .Select(x => new BackgroundItemDecorator(x, matrix))
                .ToArray();

        }
        #endregion
        #region IPhysicSceneContainer
        public IMergeablePhysicScene[] GetPhysicMergerItems()
        {
            var protoBox = this.AssociatedPrototyp.BoundingBox;
            var matrix = Matrix4x4.Translate(-protoBox.X, -protoBox.Y, 0) * this.RotatedRectangle.GetLocalToScreenMatrix();

            return (this.AssociatedPrototyp as IPhysicSceneContainer)
                .GetPhysicMergerItems()
                .Select(x => new PhysicMergerItemDecorator(x, matrix))
                .ToArray();
        }
        #endregion
    }
}
