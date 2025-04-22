using GraphicMinimal;
using GraphicPanels;
using LevelEditorControl.EditorFunctions;
using LevelEditorControl.LevelItems.Polygon;
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
        public Vector2D PivotPoint { get; set; } = null;
        public RotatedRectangle Position { get; } = null;
        public RectangleF GetBoundingBox()
        {
            var boxes = Drawer.GetAllSegments(p1, p2)
                .Select(x => MathHelper.GetBoundingBoxFromPolygon(x.Points));

            return MathHelper.GetBoundingBox(boxes);
        }
        public Vector2D[] GetCornerPoints()
        {
            return this.Drawer.GetAllSegments(p1, p2).SelectMany(x => x.Points).ToArray();
        }
        public float GetArea()
        {
            return Drawer.GetAllSegments(p1, p2).Select(x => x.GetArea()).Sum();
        }
        public void Draw(GraphicPanel2D panel)
        {
            Drawer.DrawLawn(p1, p2, panel);
        }
        public void DrawBorder(GraphicPanel2D panel, Pen borderPen)
        {
            var segments = Drawer.GetAllSegments(p1, p2);
            foreach (var segment in segments)
            {
                panel.DrawPolygon(borderPen, segment.Points.ToList());
            }
        }
        public void DrawWithTwoColors(GraphicPanel2D panel, Color frontColor, Color backColor)
        {
            var segments = Drawer.GetAllSegments(p1, p2);
            foreach (var segment in segments)
            {
                panel.DrawFillPolygon(frontColor, segment.Points.ToList());
            }
        }
        public bool IsPointInside(Vector2D point)
        {
            bool isInside = Drawer.GetAllSegments(p1, p2).Any(x => x.IsPointInside(point));
            return isInside;
        }
        public bool IsPointInside(Vector2D point, Matrix4x4 screenToLocal) //point = ScreenSpace-Mousepoint
        {
            point = Matrix4x4.MultPosition(screenToLocal, new Vector3D(point.X, point.Y, 0)).XY; //CameraSpace-Mousepoint
            return IsPointInside(point);
        }
        public Matrix4x4 GetScreenToLocalMatrix()
        {
            return Matrix4x4.Ident();
        }

        public object GetExportData()
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
            var p1A = segment.Points[0];
            var p1B = segment.Points[1];
            var p2B = segment.Points[2];
            var p2A = segment.Points[3];

            var center = (p1A + p2B) / 2;
            float width = (p1A - p2A).Length();

            float angle = Vector2D.Angle360(new Vector2D(1, 0), (p1A - p2A).Normalize());
            if (AssocitedPolygon.IsOutside == false) angle += 180;

            return new LawnSegmentBackgroundItem(center, angle, width, Drawer.LawnHeight, Drawer.TextureFile, Drawer.ZValue);
        }
    }

    internal class LawnEdgeExportData
    {
        public int LevelItemId; //eigene Id
        public int PolygonLevelItemId; //Id von den Polygon wo dieses Objekt dranhängt
        public string TextureFile;
        public float ZValue;
        public float LawnHeight;
        public int Index1;
        public float FPos1;
        public int Index2;
        public float FPos2;
    }
}
