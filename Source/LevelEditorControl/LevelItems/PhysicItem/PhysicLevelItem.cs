using LevelEditorExports.Editor.Helper;
using LevelEditorExports.Editor.LevelItems;
using LevelEditorExports.Editor.Prototyps;
using LevelEditorGlobal;
using LevelToSimulatorConverter._2_MergeToSingleScene;
using PhysicGlobal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace LevelEditorControl.LevelItems.PhysicItem
{
    internal class PhysicLevelItem : IPrototypLevelItem, IMergeablePhysicScene, IKeyboardControlledLevelItem, IRotateableLevelItem, ICollidableContainer, ITagableContainer
    {
        private IPrototypItem prototyp;

        public PhysicLevelItem(IPrototypItem item, Vec2D position, InitialRotatedRectangleValues initialRecValues, int id)
        {
            if (item is IKeyboardControlledLevelItem == false) throw
                    new ArgumentException("item must implement IKeyboardControlledLevelItem");

            if (item is ICollidableContainer == false) throw
                    new ArgumentException("item must implement ICollidableContainer");

            if (item is ITagableContainer == false) throw
                    new ArgumentException("item must implement ITagableContainer");

            if (item.EditorExportData is PhysicItemExportData == false) throw
                    new ArgumentException("item.EditorExportData  must implement PhysicItemExportData");

            Id = id;
            prototyp = item;

            this.RotatedRectangle = new RotatedRectangle(position, prototyp.BoundingBox.GetSize(), initialRecValues);

            this.Collidables = (item as ICollidableContainer).Collidables.Select(x => new MouseClickableWithCollision(x, this.RotatedRectangle)).ToArray();
            this.Tagables = (item as ITagableContainer).Tagables.Select(x => new MouseClickableDecorator(x, this.RotatedRectangle)).ToArray();
        }

        public int Id { get; }
        public bool IsSelected { get; set; } = false;
        public Vec2D PivotPoint { get => this.RotatedRectangle.PivotPoint; set => this.RotatedRectangle.PivotPoint = value; }
        public RotatedRectangle RotatedRectangle { get; }

        public PhysicItemExportData PhysicData { get => (PhysicItemExportData)prototyp.EditorExportData; }

        public ICollidable[] Collidables { get; } //ICollidableContainer
        public IMouseclickableWithTagData[] Tagables { get; } //ITagableContainer

        public PhysicGlobal.BoundingBox GetBoundingBox()
        {
            return this.RotatedRectangle.GetBoundingBox();
        }
        public Vec2D[] GetCornerPoints()
        {
            return this.RotatedRectangle.GetCornerPoints();
        }
        public float GetArea()
        {
            return prototyp.BoundingBox.GetWidth() * prototyp.BoundingBox.GetHeight();
        }
        public void Draw(IDrawingPanel panel)
        {
            panel.PushMatrix();
            panel.MultTransformationMatrix(this.RotatedRectangle.GetLocalToScreenMatrix());
            prototyp.Draw(panel);
            panel.PopMatrix();
        }
        public void DrawBorder(IDrawingPanel panel, Pen borderPen)
        {
            panel.PushMatrix();
            panel.MultTransformationMatrix(this.RotatedRectangle.GetLocalToScreenMatrix());
            this.prototyp.DrawBorder(panel, borderPen);
            panel.PopMatrix();
        }
        public void DrawWithTwoColors(IDrawingPanel panel, Color frontColor, Color backColor)
        {
            panel.PushMatrix();
            panel.MultTransformationMatrix(this.RotatedRectangle.GetLocalToScreenMatrix());
            this.prototyp.DrawWithTwoColors(panel, frontColor, backColor);
            panel.PopMatrix();
        }
        public bool IsPointInside(Vec2D point)
        {
            return this.RotatedRectangle.IsPointInside(point);
        }
        public bool IsPointInside(Vec2D point, Matrix4x4 screenToLocal) //point = ScreenSpace-Mousepoint
        {
            point = Matrix4x4.MultPosition(screenToLocal, point); //CameraSpace-Mousepoint
            return IsPointInside(point);
        }
        public Matrix4x4 GetScreenToLocalMatrix()
        {
            return Matrix4x4.Invert(this.RotatedRectangle.GetLocalToScreenMatrix());
        }

        #region IObjectSerializable
        public ILevelItemExportData GetExportData()
        {
            return new PhysicLevelItemExportData()
            {
                LevelItemId = Id,
                PrototypId = prototyp.Id,
                Position = PivotPoint,
                SizeFactor = this.SizeFactor,
                AngleInDegree = this.AngleInDegree,
                LocalPivot = LocalPivotPoint
            };
        }

        public static PhysicLevelItem CreateFromExportData(PhysicLevelItemExportData data, List<IPrototypItem> prototyps)
        {
            var proto = prototyps.First(x => x.Id == data.PrototypId);
            if (data.SizeFactor == 0) data.SizeFactor = 1;
            if (data.LocalPivot == null) data.LocalPivot = new Vec2D(0, 0);
            var initialRecValues = new InitialRotatedRectangleValues()
            {
                SizeFactor = data.SizeFactor,
                AngleInDegree = data.AngleInDegree,
                LocalPivot = data.LocalPivot
            };
            return new PhysicLevelItem(proto, data.Position, initialRecValues, data.LevelItemId);
        }
        #endregion

        #region IPhysicMergerItem
        public int LevelItemId { get => this.Id; }
        public Matrix4x4 GetTranslationMatrix()
        {
            return Matrix4x4.Translate(-this.prototyp.BoundingBox.X, -this.prototyp.BoundingBox.Y, 0) * this.RotatedRectangle.GetLocalToScreenMatrix();
        }
        public Vec2D LocalPivotPoint { get => this.RotatedRectangle.LocalPivot; }
        public float SizeFactor { get => this.RotatedRectangle.SizeFactor; }
        public float AngleInDegree { get => this.RotatedRectangle.AngleInDegree; }
        #endregion

        #region IPrototypLevelItem

        public IPrototypItem AssociatedPrototyp { get => prototyp; }

        public void UpdateAfterPrototypWasChanged(IPrototypItem oldItem, IPrototypItem newItem)
        {
            if (prototyp == oldItem)
            {
                prototyp = newItem;
            }
        }

        public IPrototypLevelItem CreateCopy(int newId)
        {
            var export = (PhysicLevelItemExportData)GetExportData();
            export.LevelItemId = newId;
            return CreateFromExportData(export, new List<IPrototypItem>() { this.AssociatedPrototyp });
        }
        #endregion

        #region IKeyboardControlledLevelItem
        public string[] GetAllKeyPressHandlerNames()
        {
            return (prototyp as IKeyboardControlledLevelItem).GetAllKeyPressHandlerNames();
        }
        #endregion
    }
}
