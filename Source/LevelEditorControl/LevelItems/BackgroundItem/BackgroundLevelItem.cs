using GraphicMinimal;
using GraphicPanels;
using LevelEditorGlobal;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace LevelEditorControl.LevelItems.BackgroundItem
{
    internal class BackgroundLevelItem : IPrototypLevelItem, IBackgroundItem, IRotateableLevelItem
    {
        public BackgroundLevelItem(BackgroundPrototypItem prototyp, Vector2D position, InitialRotatedRectangleValues initialRecValues, int id)
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
        public bool IsPointInside(Vector2D point) //point = Globalspace-Mousepoint
        {
            return this.RotatedRectangle.IsPointInside(point);
        }
        
        public bool IsPointInside(Vector2D point, Matrix4x4 screenToLocal) //point = ScreenSpace-Mousepoint
        {
            point = Matrix4x4.MultPosition(screenToLocal, new Vector3D(point.X, point.Y, 0)).XY; //CameraSpace-Mousepoint
            return IsPointInside(point);
        }
        public Matrix4x4 GetScreenToLocalMatrix()
        {
            return Matrix4x4.Invert(this.RotatedRectangle.GetLocalToScreenMatrix());
        }
        public IPrototypLevelItem CreateCopy(int newId)
        {
            var export = (BackgroundLevelItemExportData)GetExportData();
            export.LevelItemId = newId;
            return CreateFromExportData(export, new List<IPrototypItem>() { this.AssociatedPrototyp });
        }

        #region IObjectSerializable
        public object GetExportData()
        {
            return new BackgroundLevelItemExportData()
            {
                LevelItemId = Id,
                PrototypId = AssociatedPrototyp.Id,
                Position = PivotPoint,
                SizeFactor = this.RotatedRectangle.SizeFactor,
                AngleInDegree = this.RotatedRectangle.AngleInDegree,
                LocalPivot = this.RotatedRectangle.LocalPivot
            };
        }

        public static BackgroundLevelItem CreateFromExportData(BackgroundLevelItemExportData data, List<IPrototypItem> prototyps)
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
            return new BackgroundLevelItem((BackgroundPrototypItem)proto, data.Position, initialRecData, data.LevelItemId);
        }
        #endregion

        public BackgroundItemSimulatorExportData GetSimulatorExportData()
        {
            var r = this.RotatedRectangle;
            var cornerPoints = r.GetCornerPoints();

            var protoExport = (BackgroundPrototypExportData)this.AssociatedPrototyp.EditorExportData;

            return new BackgroundItemSimulatorExportData()
            {
                Width = r.OriginalSize.Width * r.SizeFactor,
                Height = r.OriginalSize.Height * r.SizeFactor,
                AngleInDegree = r.AngleInDegree,
                Center = (cornerPoints[0] + cornerPoints[2]) / 2,
                TextureFile = protoExport.TextureFile,
                ZValue = protoExport.ZValue,
            };
        }
    }
}
