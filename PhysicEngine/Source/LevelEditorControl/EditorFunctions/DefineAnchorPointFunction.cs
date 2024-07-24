using GraphicMinimal;
using LevelEditorControl.Controls.TagItemControl;
using LevelEditorGlobal;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LevelEditorControl.EditorFunctions
{
    internal class DefineAnchorPointFunction : DummyFunction, IEditorFunction
    {
        private EditorState state;
        private IMouseclickableWithTagData[] tagables;
        private IMouseclickableWithTagData mouseOverItem = null; //Über diesen Item befindet sich die Maus gerade
        private AnchorPoint mouseOverPoint = null; //Über diesen Ankerpunkt befindet sich die Maus gerade
        private IMouseclickableWithTagData selectedItem = null;
        private Vector2D localAnchorPosition = null; //Testausgabe der lokalen Ankerpunktposition
        public override FunctionType Type => FunctionType.DefineAnchorPoint;

        public override IEditorFunction Init(EditorState state)
        {
            this.state = state;

            List<IMouseclickableWithTagData> list = new List<IMouseclickableWithTagData>();

            list.AddRange(
                this.state.LevelItems
                .Where(x => x is IMouseclickableWithTagData)
                .Where(x => (x is ITagableContainer) == false) //Das Containerelement soll nicht mit in die Liste
                .Cast<IMouseclickableWithTagData>());

            list.AddRange(
                this.state.LevelItems
                    .Where(x => x is ITagableContainer)
                    .Cast<ITagableContainer>()
                    .SelectMany(x => x.Tagables));

            this.tagables = list.ToArray();

            return this;
        }

        private IMouseclickableWithTagData GetSelectedItem(Vector2D point)
        {
            var screenToCamera = this.state.Camera.GetPointToCameraMatrix();

            List<KeyValuePair<float, IMouseclickableWithTagData>> list = new List<KeyValuePair<float, IMouseclickableWithTagData>>();
            foreach (var item in this.tagables)
            {
                if (item.IsPointInside(point, screenToCamera))
                {
                    list.Add(new KeyValuePair<float, IMouseclickableWithTagData>(item.GetArea(), item));
                }
            }

            if (list.Any())
                return list.OrderBy(x => x.Key).First().Value;

            return null;
        }

        public override void HandleMouseMove(MouseEventArgs e)
        {
            var point = new Vector2D(e.X, e.Y);
            this.mouseOverItem = GetSelectedItem(point);

            if (this.selectedItem != null)
            {
                var screenToLocal = this.state.Camera.GetPointToCameraMatrix() * this.selectedItem.GetScreenToLocalMatrix();
                this.localAnchorPosition = Matrix4x4.MultPosition(screenToLocal, new Vector3D(point.X, point.Y, 0)).XY;
            }else
            {
                this.localAnchorPosition = null;
            }

            this.mouseOverPoint = GetAnchorPoint(point, 5);

            Refresh();
        }

        public override void HandleMouseClick(MouseEventArgs e)
        {
            var point = new Vector2D(e.X, e.Y);
            if (e.Button == MouseButtons.Left)
            {
                var clickItem = GetSelectedItem(point);

                if (this.selectedItem != clickItem)
                {
                    this.selectedItem = clickItem;
                }else
                if (this.selectedItem != null)
                {
                    var screenToLocal = this.state.Camera.GetPointToCameraMatrix() * this.selectedItem.GetScreenToLocalMatrix();
                    var anchorPoint = Matrix4x4.MultPosition(screenToLocal, new Vector3D(point.X, point.Y, 0)).XY;
                    var tagData = GetTagData(this.selectedItem);
                    tagData.AnchorPoints.Add(anchorPoint);
                }
            }
            if (e.Button == MouseButtons.Right)
            {
                if (this.mouseOverPoint != null)
                {
                    //Lösche den Ankerpunkt
                    var tagData = GetTagData(this.mouseOverPoint.Item);
                    tagData.AnchorPoints.RemoveAt(this.mouseOverPoint.PointIndex);
                    this.mouseOverPoint = null;
                }else
                if (this.selectedItem != null)
                {
                    this.selectedItem = null; //Deselektiere das Item
                }
            }

            Refresh();
        }

        class AnchorPoint
        {
            public IMouseclickableWithTagData Item;
            public int PointIndex;
        }

        private AnchorPoint GetAnchorPoint(Vector2D mousePoint, float anchorPointRadius)
        {
            mousePoint = this.state.Camera.PointToCamera(mousePoint.ToPointF()).ToGrx();
            foreach (var item in this.tagables)
            {
                var tagData = GetTagData(item);
                if (tagData.AnchorPoints.Any())
                {
                    var localToScreen = Matrix4x4.Invert(item.GetScreenToLocalMatrix());
                    for (int i=0;i<tagData.AnchorPoints.Count;i++)
                    {
                        var localPoint = tagData.AnchorPoints[i];
                        var screenPoint = Matrix4x4.MultPosition(localToScreen, new Vector3D(localPoint.X, localPoint.Y, 0)).XY;
                        if ((screenPoint - mousePoint).Length() < anchorPointRadius)
                        {
                            return new AnchorPoint() {Item = item, PointIndex = i };
                        }
                    }
                }
            }

            return null;
        }

        private TagEditorData GetTagData(IMouseclickableWithTagData tagable)
        {
            var tagData = this.state.TagDataStorrage[tagable];
            return tagData;
        }

        public override void HandleTimerTick(float dt)
        {
            Refresh();
        }

        private void Refresh()
        {
            var panel = this.state.Panel;

            panel.ClearScreen(Color.White);

            if (this.localAnchorPosition != null)
            {
                panel.DrawString(10, 20, Color.Black, 20, localAnchorPosition.ToString());
            }

            panel.MultTransformationMatrix(this.state.Camera.GetPointToSceenMatrix());

            float size1 = this.state.Camera.LengthToCamera(5);
            float size2 = this.state.Camera.LengthToCamera(3);

            foreach (var item in this.tagables)
            {
                var color = item == this.selectedItem ? Color.Red : Color.Black;
                item.DrawBorder(panel, new Pen(color, (item == this.mouseOverItem || item == state.SelectedSubItem) ? size1 : size2));

                var tagData = GetTagData(item);
                if (tagData.AnchorPoints.Any())
                {
                    var localToScreen = Matrix4x4.Invert(item.GetScreenToLocalMatrix());
                    foreach (var localPoint in tagData.AnchorPoints)
                    {
                        var screenPoint = Matrix4x4.MultPosition(localToScreen, new Vector3D(localPoint.X, localPoint.Y, 0)).XY;
                        panel.DrawFillRegularPolygon(Color.Green, screenPoint, size1, 7);
                    }
                }
            }

            if (this.mouseOverPoint != null)
            {
                var item = this.mouseOverPoint.Item;
                var localToScreen = Matrix4x4.Invert(item.GetScreenToLocalMatrix());
                var localPoint = GetTagData(item).AnchorPoints[this.mouseOverPoint.PointIndex];
                var screenPoint = Matrix4x4.MultPosition(localToScreen, new Vector3D(localPoint.X, localPoint.Y, 0)).XY;
                panel.DrawCircle(Pens.Red, screenPoint, 6);
            }

            panel.FlipBuffer();
        }
    }
}
