using GraphicMinimal;
using LevelEditorControl.LevelItems.LawnEdge;
using LevelEditorControl.LevelItems.Polygon;
using LevelEditorControl.LevelItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using LevelEditorControl.Controls.PolygonControl;
using ReactiveUI;
using System.Windows.Forms;
using System.Drawing;

namespace LevelEditorControl.EditorFunctions
{
    internal class AddAndEditLawnEdgeFunction : DummyFunction, IEditorFunction
    {
        private readonly string DefaultTexture = "Lawn.png"; //Wenn er aus dem Data-Verzeichnis diese Datei findet, dann nimmt er die als Initialwert

        private EditorState state;
        private ILevelItemPolygon polygon;
        private LawnEdgeDrawer drawer;

        private LawnEdgeDrawer.PolygonPoint point1 = null;
        private LawnEdgeDrawer.PolygonPoint point2 = null;
        private LawnEdgeDrawer.PolygonPoint mousePolyPoint = null;
        private LawnEdgeDrawer.PolygonPoint selectedPoint = null;
        private LawnEdgePropertyViewModel propertys;
        private Action<MouseEventArgs?> isFinish;
        private bool addItemAtDispose = true;
        private LawnEdgeLevelItem itemToEdit = null;
        private bool firstMouseClick = true;

        public override FunctionType Type { get; } = FunctionType.AddLawnEdge;

        public override IEditorFunction Init(EditorState state)
        {
            throw new NotImplementedException();
        }

        public IEditorFunction InitForAddNewItem(EditorState state, string dataFolder, Action<MouseEventArgs?> isFinish)
        {
            this.state = state;
            this.isFinish = isFinish;

            if (state.SelectedItems.Count != 1 || (state.SelectedItems.Items.First() is ILevelItemPolygon) == false)
                throw new Exception("There must be on ILevelItemPolygon selected to use this function");

            this.polygon = (ILevelItemPolygon)state.SelectedItems.Items.First();
            this.drawer = new LawnEdgeDrawer(polygon);

            this.HasPropertyControl = true;

            string textureFile = dataFolder + DefaultTexture;
            if (File.Exists(textureFile))
            {
                this.drawer.TextureFile = Path.GetFullPath(textureFile);
            }
            else if (File.Exists(dataFolder + "..\\" + DefaultTexture))
            {
                this.drawer.TextureFile = Path.GetFullPath(dataFolder + "..\\" + DefaultTexture);
            }else
            {
                this.drawer.TextureFile = "";
            }

            return this;
        }

        public IEditorFunction InitForEditItem(EditorState state, Action<MouseEventArgs?> isFinish)
        {
            if (state.SelectedItems.Count != 1 || (state.SelectedItems.Items.First() is LawnEdgeLevelItem) == false)
                throw new Exception("There must be on LawnEdgeLevelItem selected to use this function");

            this.itemToEdit = (LawnEdgeLevelItem)state.SelectedItems.Items.First();

            this.state = state;
            this.isFinish = isFinish;

            this.polygon = itemToEdit.AssocitedPolygon;
            this.drawer = this.itemToEdit.Drawer;

            this.HasPropertyControl = true;

            this.point1 = this.itemToEdit.p1;
            this.point2 = this.itemToEdit.p2;

            this.addItemAtDispose = false;

            return this;
        }

        public override System.Windows.Controls.UserControl GetPropertyControl()
        {
            this.propertys = new LawnEdgePropertyViewModel(this.drawer);

            this.propertys.WhenAnyValue(x => x.TextureFile, y => y.LawnHeight, z => z.ZValue).Subscribe(x =>
            {
                Draw();
            });

            return new LawnEdgePropertyControl() { DataContext = this.propertys };
        }

        public override void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Delete && this.itemToEdit != null)
            {
                this.state.RemoveLevelItemsFromEditorState(new List<ILevelItem>() { this.itemToEdit });
                this.isFinish(null);
            }
        }

        public override void HandleMouseMove(MouseEventArgs e)
        {
            var point = state.Camera.PointToCamera(new PointF(e.X, e.Y)).ToGrx();

            if (this.selectedPoint == null && (this.point1 == null || this.point2 == null))
            {
                this.mousePolyPoint = GetPolyPoint(point);
            }
            else if (this.selectedPoint != null)
            {
                this.mousePolyPoint = null;
                var newPos = GetPolyPoint(point);
                if (newPos != null)
                {
                    this.selectedPoint.TakeData(newPos);
                }
            }

            Draw();
        }

        public override void HandleMouseClick(MouseEventArgs e)
        {
            var point = state.Camera.PointToCamera(new PointF(e.X, e.Y)).ToGrx();

            if (this.point1 == null)
            {
                this.point1 = GetPolyPoint(point);
            }
            else if (this.point2 == null)
            {
                this.point2 = GetPolyPoint(point);
            }

            if (this.point1 != null && this.point2 != null)
            {
                float dis1 = (this.point1.Position - point).Length();
                float dis2 = (this.point2.Position - point).Length();

                float distanceToQuit = state.Camera.LengthToCamera(50);
                if (dis1 > distanceToQuit && dis2 > distanceToQuit)
                {
                    this.isFinish(e);
                }
            }
        }

        public override void HandleMouseDown(MouseEventArgs e)
        {
            var point = state.Camera.PointToCamera(new PointF(e.X, e.Y)).ToGrx();

            this.selectedPoint = null;

            float maxDistance = state.Camera.LengthToCamera(50); //50 Pixel
            if (this.point1 != null && (this.point1.Position - point).Length() < maxDistance)
            {
                this.selectedPoint = this.point1;
            }

            if (this.point2 != null && (this.point2.Position - point).Length() < maxDistance)
            {
                this.selectedPoint = this.point2;
            }
        }

        public override void HandleMouseUp(MouseEventArgs e)
        {
            this.selectedPoint = null;
        }

        private LawnEdgeDrawer.PolygonPoint GetPolyPoint(Vector2D point)
        {
            float lineWidth = state.Camera.LengthToCamera(40); //Die Linie vom Polygon soll 40 Pixel breit sein. 

            var points = this.polygon.Points;

            for (int i = 0; i < points.Length; i++)
            {
                var p1 = points[i];
                var p2 = points[(i + 1) % points.Length];
                if (MathHelper.IsPointAboveLine(p1, p2, point, lineWidth))
                {
                    var pointOnLine = MathHelper.GetProjectedPointOnLine(p1, p2, point, out float distance, out float distancePercent);
                    if (pointOnLine == null)
                        throw new Exception("Abnormal error");

                    return new LawnEdgeDrawer.PolygonPoint(i, distancePercent, polygon);
                }
            }

            return null;
        }

        private void Draw()
        {
            var panel = this.state.Panel;

            state.DrawItems();

            //Zeichne das Objekt, was gerade neu erstellt wird
            if (this.itemToEdit == null)
            {
                panel.ZValue2D = this.drawer.ZValue;

                if (this.mousePolyPoint != null)
                {
                    DrawPolyPoint(this.mousePolyPoint, Color.Blue);
                }

                if (this.point1 != null)
                {
                    DrawPolyPoint(this.point1, Color.Red);
                }

                if (this.point2 != null)
                {
                    DrawPolyPoint(this.point2, Color.Red);
                }

                if (this.point1 != null)
                {
                    if (this.point2 != null)
                    {
                        this.drawer.DrawLawn(this.point1, this.point2, panel);
                    }
                    else if (this.mousePolyPoint != null)
                    {
                        this.drawer.DrawLawn(this.point1, this.mousePolyPoint, panel);
                    }
                }
            }


            panel.FlipBuffer();
        }



        private void DrawPolyPoint(LawnEdgeDrawer.PolygonPoint point, Color color)
        {
            float height = this.state.Camera.LengthToScreen(this.drawer.LawnHeight);
            var p1 = state.Camera.PointToScreen(point.Position.ToPointF()).ToGrx();
            var p2 = state.Camera.PointToScreen((point.Position + point.Normal * height).ToPointF()).ToGrx();
            this.state.Panel.DrawLine(new Pen(color, 5), p1, p2);
        }

        public override void Dispose()
        {
            if (this.point1 != null && this.point2 != null)
            {
                if (this.addItemAtDispose)
                {
                    this.drawer.TextureFile = this.propertys.TextureFile;
                    this.drawer.ZValue = this.propertys.ZValue;
                    this.drawer.LawnHeight = this.propertys.LawnHeight;
                    int newId = this.state.GenerateLevelItemId();
                    this.state.AddLevelItem(new LawnEdgeLevelItem(this.polygon, this.drawer, this.point1, this.point2, newId));
                }
                else
                {
                    state.SetSelectionState(this.itemToEdit, false);
                }
            }
        }
    }
}
