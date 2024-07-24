using GraphicMinimal;
using GraphicPanels;
using LevelEditorControl.Controls.PolygonControl;
using LevelEditorControl.EditorFunctions;
using LevelEditorGlobal;
using LevelToSimulatorConverter;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static LevelEditorGlobal.ITagable;

namespace LevelEditorControl.LevelItems.Polygon
{
    internal class PolygonLevelItem : ILevelItem, IMergeablePhysicPolygon, ILevelItemPolygon, IEditablePolygon, ICollidable, IMouseclickableWithTagData
    {
        private Vector2D[] localPoints;
        private SizeF localBoxSize;
        private PolygonImages images;
        private RectangleF globalBoundingBox; //BoundingBox über alle PolygonLevelItem im Editor
        private Vector2D[] GlobalPoints { get => localPoints.Select(x => PivotPoint + x).ToArray(); }

        public PolygonLevelItem(Vector2D[] points, PolygonImages images, int id)
        {
            points = MathHelper.OrderPointsCCW(points);
            var box = MathHelper.GetBoundingBoxFromPolygon(points);
            PivotPoint = new Vector2D(box.X, box.Y);
            localPoints = points.Select(x => x - PivotPoint).ToArray();
            localBoxSize = new SizeF(box.Width, box.Height);
            this.images = images;
            Id = id;
        }

        public int Id { get; }
        public TagType TypeName { get => TagType.Polygon; } //ITagable
        public bool IsSelected { get; set; } = false;
        public Vector2D PivotPoint { get; set; }
        public RotatedRectangle Position { get; }
        public RectangleF GetBoundingBox()
        {
            return new RectangleF(PivotPoint.X, PivotPoint.Y, localBoxSize.Width, localBoxSize.Height);
        }
        public Vector2D[] GetCornerPoints()
        {
            return this.Points;
        }
        public float GetArea()
        {
            return MathHelper.GetAreaFromPolygon(localPoints);
        }
        public void Draw(GraphicPanel2D panel)
        {
            string texture = IsOutside ? images.ForegroundImage : images.BackgroundImage;

            panel.ZValue2D = ZOrder;

            if (texture == null)
            {
                panel.DrawPolygon(Pens.Black, GlobalPoints.ToList());
                return;
            }

            var vertices = GlobalPoints.Select(x => new Vertex2D(x,
                new Vector2D((x.X - globalBoundingBox.X) / globalBoundingBox.Width, (x.Y - globalBoundingBox.Y) / globalBoundingBox.Height)))
                .ToList();


            panel.DrawFillPolygon(texture, false, Color.White, vertices);
        }
        public void DrawBorder(GraphicPanel2D panel, Pen borderPen)
        {
            panel.ZValue2D = ZOrder;
            panel.DrawPolygon(borderPen, GlobalPoints.ToList());
        }
        public void DrawWithTwoColors(GraphicPanel2D panel, Color frontColor, Color backColor)
        {
            panel.ZValue2D = ZOrder;
            var color = this.IsOutside ? frontColor : backColor;
            panel.DrawFillPolygon(color, GlobalPoints.ToList());
        }
        public bool IsPointInside(Vector2D point)
        {
            return MathHelper.PointIsInsidePolygon(localPoints, point - PivotPoint);
        }

        public object GetExportData()
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
        public bool IsPointInside(Vector2D point, Matrix4x4 screenToLocal)
        {
            point = Matrix4x4.MultPosition(screenToLocal, new Vector3D(point.X, point.Y, 0)).XY; //screenToLocal = ScreenToCamera-Space
            return MathHelper.PointIsInsidePolygon(localPoints, point - PivotPoint);
        }
        public Matrix4x4 GetScreenToLocalMatrix()
        {
            return Matrix4x4.Ident();
        }
        #endregion


        #region IMergeablePhysicPolygon
        public int LevelItemId { get => this.Id; }
        public Vector2D[] Points { get => localPoints.Select(x => PivotPoint + x).ToArray(); }
        public bool IsOutside { get; private set; } = true; //Zeigen die Normalen nach Außen?
        public int ZOrder { get; private set; }
        public float Friction { get; set; } = 0.2f;
        public float Restiution { get; set; } = 0.5f;
        public int CollisionCategory { get; set; } = 0;
        #endregion

        #region IEditablePolygon
        public void MovePointAtIndex(int index, Vector2D newPosition)
        {
            this.localPoints[index] = newPosition - PivotPoint;
        }
        public void RemovePointAtIndex(int index)
        {
            var list = this.localPoints.ToList();
            list.RemoveAt(index);
            this.localPoints = list.ToArray();
        }
        public void AddPointAfterIndex(int index, Vector2D newPosition)
        {
            var list = this.localPoints.ToList();
            list.Insert((index + 1) % localPoints.Length, newPosition - this.PivotPoint);
            this.localPoints = list.ToArray();
        }
        #endregion

        public static void UpdateIsOutsideAndUVFromAllPolygons(List<PolygonLevelItem> polygons)
        {
            //Schritt 1: Finde den Y-Bereich herraus und setze alle ZOrder auf 0
            var box = MathHelper.GetBoundingBox(polygons.Select(x => x.GetBoundingBox()));
            foreach (var polygon in polygons)
            {
                polygon.ZOrder = 0;
                polygon.globalBoundingBox = box;
            }

            //Schritt 2: Setze die ZOrder-Werte
            for (int y = (int)box.Y; y <= (int)box.Bottom; y++)
            {
                List<PolygonLevelItem> polys = GetOrderedScanlineList(polygons, y);
                if (polys == null) continue;
                int currentZ = 0;
                foreach (PolygonLevelItem poly in polys)
                {
                    if (poly.ZOrder == 0)
                    {
                        currentZ++;
                        poly.ZOrder = currentZ;
                    }
                    else
                    {
                        if (currentZ != poly.ZOrder)
                            currentZ = poly.ZOrder;
                        else
                            currentZ--;
                    }
                }
            }

            //Schritt 3: Sortiere nach ZOrder
            polygons = polygons.OrderBy(x => x.ZOrder).ToList();

            //Schritt 4: Lege IsOutside für die Polygone fest
            foreach (var poly in polygons)
            {
                if (poly.ZOrder == 0) throw new Exception("Error: ZOrder can not be detected");
                if (poly.ZOrder % 2 == 0)
                    poly.IsOutside = false;
                else
                {
                    poly.IsOutside = true;
                }

                poly.ZOrder -= 100; //Damit ein Hintergrund-Polygon kein anders Objekt verdeckt (Ihre Z-Order-Werte sind alle größer 0)
            }
        }

        //Rückgabe alle Schnittpunkte von allen Polygonen für eine Scanline. Sie werden dabei nach X sortiert.
        private static List<PolygonLevelItem> GetOrderedScanlineList(List<PolygonLevelItem> polygons, int yScan)
        {
            List<PolygonLevelItem> ret = new List<PolygonLevelItem>();
            List<int> xValues = new List<int>();

            //Schritt 1: Schnittpunkte von allen Polygonen einsammeln
            for (int i = 0; i < polygons.Count; i++)
            {
                PolygonLevelItem poly = polygons[i];
                List<int> points = MathHelper.PolygonScanlineIntersectionTest(poly.GlobalPoints, yScan);
                if (points == null) return null;
                foreach (int p in points)
                {
                    ret.Add(poly);
                    xValues.Add(p);
                }
            }

            //Schritt 2: Schnittpunkte sortieren
            for (int i = 0; i < ret.Count; i++)
                for (int j = i; j < ret.Count; j++)
                {
                    if (xValues[i] > xValues[j])
                    {
                        PolygonLevelItem tmp1 = ret[i];
                        ret[i] = ret[j];
                        ret[j] = tmp1;
                        int tmp2 = xValues[i];
                        xValues[i] = xValues[j];
                        xValues[j] = tmp2;
                    }
                }

            return ret;
        }

        
    }

    internal class PolygonLevelItemExportData
    {
        public int LevelItemId { get; set; }
        public Vector2D[] Points { get; set; }
        public float Friction { get; set; }
        public float Restiution { get; set; }
        public int CollisionCategory { get; set; }
    }
}
