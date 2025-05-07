using LevelEditorControl.EditorFunctions;
using LevelEditorControl.LevelItems.Polygon;
using LevelEditorExports.Editor.Helper;
using LevelEditorExports.Editor.LevelItems;
using PhysicGlobal;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace LevelEditorControl.LevelItems.LawnEdge
{
    //Wird vom Editor genutzt. Hier kann das Objekt noch editiert werden.
    internal class LawnEdgeLevelItem : ILevelItem, IBackgroundItemProvider
    {
        public LawnEdgeDrawer Drawer { get; private set; }
        public LawnEdgeDrawer.PolygonPoint p1 { get; private set; }
        public LawnEdgeDrawer.PolygonPoint p2 { get; private set; }

        public LawnEdgeLevelItem(ILevelItemPolygon polygon, LawnEdgeDrawer drawer, LawnEdgeDrawer.PolygonPoint p1, LawnEdgeDrawer.PolygonPoint p2, int id)
        {
            Id = id;
            AssocitedPolygon = polygon;
            Drawer = drawer;
            this.p1 = p1;
            this.p2 = p2;
        }

        public ILevelItemPolygon AssocitedPolygon { get; }

        public int Id { get; }
        public bool IsSelected { get; set; }
        public Vec2D PivotPoint { get; set; } = null;
        public RotatedRectangle Position { get; } = null;
        public PhysicGlobal.BoundingBox GetBoundingBox()
        {
            var boxes = Drawer.GetAllSegments(p1, p2)
                .Select(x => PolygonHelper.GetBoundingBoxFromPolygon(x.Points));

            return PhysicGlobal.BoundingBox.GetBoxFromBoxes(boxes);
        }
        public Vec2D[] GetCornerPoints()
        {
            return this.Drawer.GetAllSegments(p1, p2).SelectMany(x => x.Points).ToArray();
        }
        public float GetArea()
        {
            return Drawer.GetAllSegments(p1, p2).Select(x => x.GetArea()).Sum();
        }
        public void Draw(IDrawingPanel panel)
        {
            Drawer.DrawLawn(p1, p2, panel);
        }
        public void DrawBorder(IDrawingPanel panel, Pen borderPen)
        {
            var segments = Drawer.GetAllSegments(p1, p2);
            foreach (var segment in segments)
            {
                panel.DrawPolygon(borderPen, segment.Points);
            }
        }
        public void DrawWithTwoColors(IDrawingPanel panel, Color frontColor, Color backColor)
        {
            var segments = Drawer.GetAllSegments(p1, p2);
            foreach (var segment in segments)
            {
                panel.DrawFillPolygon(frontColor, segment.Points);
            }
        }
        public bool IsPointInside(Vec2D point)
        {
            bool isInside = Drawer.GetAllSegments(p1, p2).Any(x => x.IsPointInside(point));
            return isInside;
        }
        public bool IsPointInside(Vec2D point, Matrix4x4 screenToLocal) //point = ScreenSpace-Mousepoint
        {
            point = Matrix4x4.MultPosition(screenToLocal, point); //CameraSpace-Mousepoint
            return IsPointInside(point);
        }
        public Matrix4x4 GetScreenToLocalMatrix()
        {
            return Matrix4x4.Ident();
        }

        public ILevelItemExportData GetExportData()
        {
            return new LawnEdgeExportData()
            {
                LevelItemId = Id,
                PolygonLevelItemId = AssocitedPolygon.Id,
                TextureFile = Drawer.TextureFile,
                ZValue = Drawer.ZValue,
                LawnHeight = Drawer.LawnHeight,
                Index1 = p1.Index,
                Index2 = p2.Index,
                FPos1 = p1.FPos,
                FPos2 = p2.FPos,
            };
        }

        public static LawnEdgeLevelItem CreateFromExportData(LawnEdgeExportData data, List<ILevelItem> items)
        {
            var polygon = (ILevelItemPolygon)items.First(x => x.Id == data.PolygonLevelItemId);

            var drawer = new LawnEdgeDrawer(polygon) { TextureFile = data.TextureFile, LawnHeight = data.LawnHeight, ZValue = data.ZValue };
            var p1 = new LawnEdgeDrawer.PolygonPoint(data.Index1, data.FPos1, polygon);
            var p2 = new LawnEdgeDrawer.PolygonPoint(data.Index2, data.FPos2, polygon);
            return new LawnEdgeLevelItem(polygon, drawer, p1, p2, data.LevelItemId);
        }

        public IBackgroundItem[] GetBackgroundItems()
        {
            var segments = Drawer.GetAllSegments(p1, p2);
            return segments.Select(Convert).ToArray();
        }

        private LawnSegmentBackgroundItem Convert(LawnEdgeDrawer.PolygonWith4Points segment)
        {
            return new LawnSegmentBackgroundItem(segment.GetCenter(), segment.GetAngle(), segment.GetWidth(), Drawer.LawnHeight, Drawer.TextureFile, Drawer.ZValue);
        }
    }
}
