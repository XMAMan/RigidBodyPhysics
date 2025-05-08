using LevelEditorControl.Controls.PolygonControl;
using LevelEditorGlobal;
using PhysicGlobal;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LevelEditorExports.Editor.LevelItems;
using LevelEditorExports.Simulator;
using LevelToSimulatorConverter._2_MergeToSingleScene;
using LevelEditorGlobal.Helper;

namespace LevelEditorControl.LevelItems.Polygon
{
    internal class PolygonLevelItem : ILevelItem, IMergeablePhysicPolygon, ILevelItemPolygon, IEditablePolygon, ICollidable, IMouseclickableWithTagData
    {
        private Vec2D[] localPoints;
        private SizeF localBoxSize;
        private PolygonImages images;
        private PhysicGlobal.BoundingBox globalBoundingBox; //BoundingBox über alle PolygonLevelItem im Editor
        private Vec2D[] GlobalPoints { get => localPoints.Select(x => PivotPoint + x).ToArray(); }

        public PolygonLevelItem(Vec2D[] points, PolygonImages images, int id)
        {
            points = PolygonHelper.OrderPointsClockWise(points);
            var box = PolygonHelper.GetBoundingBoxFromPolygon(points);
            PivotPoint = box.Min;
            localPoints = points.Select(x => x - PivotPoint).ToArray();
            localBoxSize = new SizeF(box.GetWidth(), box.GetHeight());
            this.images = images;
            Id = id;
        }

        public int Id { get; }
        public TagType TypeName { get => TagType.Polygon; } //ITagable
        public bool IsSelected { get; set; } = false;
        public Vec2D PivotPoint { get; set; }
        public RotatedRectangle Position { get; }
        public PhysicGlobal.BoundingBox GetBoundingBox()
        {
            return new PhysicGlobal.BoundingBox(PivotPoint.X, PivotPoint.Y, localBoxSize.Width, localBoxSize.Height);
        }
        public Vec2D[] GetCornerPoints()
        {
            return this.Points;
        }
        public float GetArea()
        {
            return PolygonHelper.GetAreaFromPolygon(localPoints);
        }
        public void Draw(IDrawingPanel panel)
        {
            string texture = IsOutside ? images.ForegroundImage : images.BackgroundImage;

            panel.ZValue2D = ZOrder;

            if (texture == null)
            {
                //Damit das Polygon zu sehen ist, wenn das Grid aktiv (liegt bei Z-Value=-1).
                //Es soll aber hinter LevelItems (liegen bei Z-Value=0) liegen, weswegen hier -0.1 verwendet wird.
                panel.ZValue2D = -0.1f;

                panel.DrawPolygon(Pens.Black, GlobalPoints);
                return;
            }

            var vertices = GlobalPoints.Select(x => new PhysicGlobal.Vertex2D(x,
                new Vec2D((x.X - globalBoundingBox.Min.X) / globalBoundingBox.GetWidth(), (x.Y - globalBoundingBox.Min.Y) / globalBoundingBox.GetHeight())))
                .ToList();


            panel.DrawFillPolygon(texture, false, Color.White, vertices);
        }
        public void DrawBorder(IDrawingPanel panel, Pen borderPen)
        {
            panel.ZValue2D = ZOrder;
            panel.DrawPolygon(borderPen, GlobalPoints);
        }
        public void DrawWithTwoColors(IDrawingPanel panel, Color frontColor, Color backColor)
        {
            panel.ZValue2D = ZOrder;
            var color = this.IsOutside ? frontColor : backColor;
            panel.DrawFillPolygon(color, GlobalPoints);
        }
        public bool IsPointInside(Vec2D point)
        {
            return PhysicGlobal.PolygonHelper.PointIsInsidePolygon(localPoints, point - PivotPoint);
        }

        public ILevelItemExportData GetExportData()
        {
            return new PolygonLevelItemExportData()
            {
                LevelItemId = Id,
                Points = GlobalPoints,
                Friction = this.Friction,
                Restiution = this.Restiution,
                CollisionCategory = this.CollisionCategory
            };
        }
        public static PolygonLevelItem CreateFromExportData(PolygonLevelItemExportData data, PolygonImages images)
        {
            return new PolygonLevelItem(data.Points, images, data.LevelItemId)
            {
                Friction = data.Friction,
                Restiution = data.Restiution,
                CollisionCategory = data.CollisionCategory
            };
        }

        #region ICollidable
        public bool IsPointInside(Vec2D point, Matrix4x4 screenToLocal)
        {
            point = Matrix4x4.MultPosition(screenToLocal, point); //screenToLocal = ScreenToCamera-Space
            return PhysicGlobal.PolygonHelper.PointIsInsidePolygon(localPoints, point - PivotPoint);
        }
        public Matrix4x4 GetScreenToLocalMatrix()
        {
            return Matrix4x4.Ident();
        }
        #endregion


        #region IMergeablePhysicPolygon
        public int LevelItemId { get => this.Id; }
        public Vec2D[] Points { get => localPoints.Select(x => PivotPoint + x).ToArray(); }
        public bool IsOutside { get; private set; } = true; //Zeigen die Normalen nach Außen?
        public int ZOrder { get; private set; }
        public float Friction { get; set; } = 0.2f;
        public float Restiution { get; set; } = 0.5f;
        public int CollisionCategory { get; set; } = 0;
        #endregion

        #region IEditablePolygon
        public void MovePointAtIndex(int index, Vec2D newPosition)
        {
            this.localPoints[index] = newPosition - PivotPoint;
        }
        public void RemovePointAtIndex(int index)
        {
            var list = this.localPoints.ToList();
            list.RemoveAt(index);
            this.localPoints = list.ToArray();
        }
        public void AddPointAfterIndex(int index, Vec2D newPosition)
        {
            var list = this.localPoints.ToList();
            list.Insert((index + 1) % localPoints.Length, newPosition - this.PivotPoint);
            this.localPoints = list.ToArray();
        }
        #endregion

        public static void UpdateIsOutsideAndUVFromAllPolygons(List<PolygonLevelItem> polygons)
        {
            //Aktualisiere die globalBoundingBox; Wird für die UV-Koordinaten benötigt
            var box = PhysicGlobal.BoundingBox.GetBoxFromBoxes(polygons.Select(x => x.GetBoundingBox()));
            foreach (var polygon in polygons)
            {
                polygon.globalBoundingBox = box;
            }

            //Aktualisiere die ZOrder und IsOutside-Werte
            PolygonHelper.UpdateIsOutsideAndUVFromAllPolygons<PolygonLevelItem>(
                polygons.Select(x => new PolygonHelper.NestedPolygon<PolygonLevelItem>(x.GlobalPoints, x)).ToArray(),
                (poly, zOrder, isOutside) =>
                {
                    poly.ZOrder = zOrder;
                    poly.IsOutside = isOutside;
                }
                );
        }
    }
}
