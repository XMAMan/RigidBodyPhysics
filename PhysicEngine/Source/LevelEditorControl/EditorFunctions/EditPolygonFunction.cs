using GraphicMinimal;
using LevelEditorControl.Controls.EditPolygonControl;
using LevelEditorControl.LevelItems.Polygon;
using LevelEditorControl.LevelItems;
using System;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;

namespace LevelEditorControl.EditorFunctions
{
    //Mit dieser Funktion können bei ein bereits erstellten Level-Rand-Polygon die Punkte verschoben werden oder neue
    //Punkte hinzu/gelöscht werden.
    internal class EditPolygonFunction : DummyFunction, IEditorFunction
    {
        private readonly float CircleRadius = 8;
        private EditorState state;
        private IEditablePolygon polygon;

        private Vector2D mouseOverPoint = null;
        private int mouseOverPolyPointIndex = -1;
        private Vector2D mouseOverPointOnLine = null;

        private Vector2D mouseDownPoint = null;
        private Vector2D mouseDownPolyPoint = null;
        private Vector2D mouseDownPointOnLine = null;
        private Vector2D mouseDownPolyPivotPoint = null;
        private Vector2D[] mouseDownSelectedPoints = null;
        private bool mouseDownIsInPolygon = false;

        private Action<MouseEventArgs?> isFinish;

        private RectangleF? selectionRec = null;
        private List<int> selectedPolyPoints = new List<int>();

        public override FunctionType Type { get; } = FunctionType.EditPolygon;

        public override IEditorFunction Init(EditorState state)
        {
            throw new NotImplementedException();
        }

        public IEditorFunction Init(EditorState state, Action<MouseEventArgs?> isFinish)
        {
            this.state = state;
            this.polygon = (IEditablePolygon)this.state.SelectedItems.Items.First();
            this.isFinish = isFinish;
            this.HasPropertyControl = true;
            return this;
        }

        public override System.Windows.Controls.UserControl GetPropertyControl()
        {
            return new EditPolygonControl() { DataContext = new EditPolygonViewModel(this.polygon) };
        }

        public override void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Delete)
            {
                this.state.RemoveLevelItemsFromEditorState(new List<ILevelItem>() { this.state.SelectedItems.Items.First() });
                this.isFinish(null);
            }
        }

        public override void HandleMouseMove(MouseEventArgs e)
        {
            var point = state.Camera.PointToCamera(new PointF(e.X, e.Y)).ToGrx();

            if (e.Button == MouseButtons.None)
            {
                float radius = this.state.Camera.LengthToCamera(CircleRadius);

                //Prüfe ob die Maus über ein Polygonpunkt ist
                this.mouseOverPoint = null;
                this.mouseOverPolyPointIndex = -1;

                for (int i = 0; i < this.polygon.Points.Length; i++)
                {
                    var polyPoint = this.polygon.Points[i];
                    if ((point - polyPoint).Length() < radius)
                    {
                        this.mouseOverPoint = polyPoint;
                        this.mouseOverPolyPointIndex = i;
                        break;
                    }
                }

                //Prüfe ob die Maus über einer Polygonkante ist
                this.mouseOverPointOnLine = null;
                var points = this.polygon.Points;
                for (int i = 0; i < points.Length; i++)
                {
                    var p1 = points[i];
                    var p2 = points[(i + 1) % points.Length];
                    if (MathHelper.IsPointAboveLine(p1, p2, point, radius))
                    {
                        var linePoint = MathHelper.GetProjectedPointOnLine(p1, p2, point, out float distance, out float distancePercent);
                        if (distance > radius * 2 && distance < (p1 - p2).Length() - radius * 2)
                        {
                            this.mouseOverPointOnLine = linePoint;
                            this.mouseOverPolyPointIndex = i;
                        }
                        break;
                    }
                }
            }

            //Die gedrückte Maustaste wird verschoben und sie hält ein Polygonpunkt fest -> Verschiebe den Polygonpunkt
            if (e.Button == MouseButtons.Left && this.mouseDownPoint != null && this.mouseDownPolyPoint != null)
            {
                this.mouseOverPointOnLine = null;

                var delta = this.mouseDownPoint - this.mouseDownPolyPoint;
                var newPolyPos = state.Grid.SnapMouse(point - delta);
                if (IsNewPolyPointPositionValid(mouseOverPolyPointIndex, newPolyPos))
                {
                    this.polygon.MovePointAtIndex(mouseOverPolyPointIndex, newPolyPos);
                }
            }

            //Die gedrückt Maustaste wird verschoben und sie hält kein Polygonpunkt fest oder ist über einer Kante -> Verschiebe das Polygon
            if (e.Button == MouseButtons.Left && this.mouseDownPolyPoint == null && this.mouseDownPointOnLine == null && this.mouseDownSelectedPoints == null &&  this.mouseDownPolyPivotPoint != null && mouseDownIsInPolygon)
            {
                var delta = this.mouseDownPoint - this.mouseDownPolyPivotPoint;
                this.polygon.PivotPoint = state.Grid.SnapMouse(point - delta);
            }

            //Die gedrückt Maustaste wird verschoben und es sind mehrere Punkte selektiert -> Verschiebe alle selektierten Punkte
            if (e.Button == MouseButtons.Left && this.mouseDownPolyPoint == null && this.mouseDownPointOnLine == null && this.mouseDownSelectedPoints != null)
            {
                this.mouseOverPointOnLine = null;

                //foreach (var mouseDownP in this.mouseDownSelectedPoints)
                for (int i=0;i<this.mouseDownSelectedPoints.Length;i++)
                {
                    int index = this.selectedPolyPoints[i];
                    var mouseDownP = this.mouseDownSelectedPoints[i];
                    var delta = this.mouseDownPoint - mouseDownP;
                    var newPolyPos = state.Grid.SnapMouse(point - delta);
                    if (IsNewPolyPointPositionValid(index, newPolyPos))
                    {
                        this.polygon.MovePointAtIndex(index, newPolyPos);
                    }
                }
                
            }

            //Schaue ob das Selektionsrechteck aufgespannt ist und ob Polygonpunkte darin liegen
            this.selectionRec = null;            
            if (e.Button == MouseButtons.Left && ShowSelectionRectangle())
            {
                this.selectedPolyPoints.Clear();
                this.selectionRec = LevelItemsHelper.CreateRectangle(this.mouseDownPoint, point);
                for (int i = 0; i < this.polygon.Points.Length; i++)
                {
                    var polyPoint = this.polygon.Points[i];
                    if (LevelItemsHelper.IsPointInRectangle(selectionRec.Value, polyPoint))
                    {
                        this.selectedPolyPoints.Add(i);
                    }
                }
            }
        }

        private bool ShowSelectionRectangle()
        {
            return this.mouseDownPoint != null && mouseOverPoint == null && mouseOverPointOnLine == null && this.mouseDownSelectedPoints == null && this.mouseDownIsInPolygon == false;
        }

        //Prüfe ob die Linien [i-1]-[i] und [i]-[i+1] andere Polygonlinien schneiden
        private bool IsNewPolyPointPositionValid(int pointIndex, Vector2D newPosition)
        {
            var points = this.polygon.Points;

            int i1 = pointIndex - 1;
            if (i1 < 0) i1 += points.Length;
            int i2 = (pointIndex + 1) % points.Length;

            for (int i = 0; i < points.Length; i++)
            {
                if (i != i1 && i != pointIndex)
                {
                    bool intersectI1 = MathHelper.IntersectLines(points[i], points[(i + 1) % points.Length], points[i1], newPosition);
                    bool intersectI2 = MathHelper.IntersectLines(points[i], points[(i + 1) % points.Length], newPosition, points[i2]);

                    if (intersectI1 || intersectI2)
                        return false;
                }
            }

            return true;
        }

        public override void HandleMouseDown(MouseEventArgs e)
        {
            var point = state.Camera.PointToCamera(new PointF(e.X, e.Y)).ToGrx();

            this.mouseDownPoint = point;
            this.mouseDownPolyPoint = this.mouseOverPoint;
            this.mouseDownPointOnLine = this.mouseOverPointOnLine;
            this.mouseDownPolyPivotPoint = this.polygon.PivotPoint;
            if (this.selectedPolyPoints.Any())
            {
                this.mouseDownSelectedPoints = this.selectedPolyPoints.Select(x => this.polygon.Points[x]).ToArray();
            }
            else
            {
                this.mouseDownSelectedPoints = null;
            }
            this.mouseDownIsInPolygon = this.polygon.IsPointInside(point);
        }

        public override void HandleMouseUp(MouseEventArgs e)
        {
            this.mouseDownPolyPoint = null;

            if (e.Button == MouseButtons.Right)
            {
                var point = state.Camera.PointToCamera(new PointF(e.X, e.Y)).ToGrx();
                var minDistance = this.polygon.Points.Select(x => (point - x).Length()).ToList().Min();
                float distanceToQuit = state.Camera.LengthToCamera(50);
                if (minDistance > distanceToQuit && this.polygon.IsPointInside(point) == false)
                {
                    this.isFinish(e); //Beende die Funktion, wenn ins Leere geklickt wurde
                }
            }
        }

        public override void HandleMouseClick(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && this.mouseOverPoint != null && this.mouseOverPolyPointIndex != -1 && this.polygon.Points.Length > 3)
            {
                this.polygon.RemovePointAtIndex(this.mouseOverPolyPointIndex);
            }

            if (e.Button == MouseButtons.Left && this.mouseOverPointOnLine != null && this.mouseOverPolyPointIndex != -1)
            {
                this.polygon.AddPointAfterIndex(this.mouseOverPolyPointIndex, this.mouseOverPointOnLine);
            }

            var point = state.Camera.PointToCamera(new PointF(e.X, e.Y)).ToGrx();
            if (e.Button == MouseButtons.Left && this.mouseDownPoint != null && (this.mouseDownPoint - point).Length() < 5)
            {
                this.selectedPolyPoints.Clear();
                this.mouseDownSelectedPoints = null;
            }
        }

        public override void HandleTimerTick(float dt)
        {
            Refresh();
        }

        private void Refresh()
        {
            var panel = this.state.Panel;

            state.DrawItems();

            float radius = this.state.Camera.LengthToCamera(CircleRadius); //Die Punkte sollen unabhängig vom Kamera-Zoom immer gleich groß sein
            foreach (var point in this.polygon.Points)
            {
                panel.DrawFillRegularPolygon(Color.Green, point, radius, 7);
            }

            if (this.mouseOverPolyPointIndex != -1 && this.mouseOverPoint != null)
            {
                panel.DrawFillRegularPolygon(Color.Blue, this.polygon.Points[mouseOverPolyPointIndex], radius, 7);
            }

            if (this.mouseOverPointOnLine != null)
            {
                panel.DrawCircle(new Pen(Color.Blue, 3), this.mouseOverPointOnLine, radius);
            }

            if (this.selectionRec != null)
            {
                var r = this.selectionRec.Value;
                panel.DrawRectangle(Pens.Black, r.X, r.Y, r.Width, r.Height);
            }

            if (this.selectedPolyPoints.Any())
            {
                foreach (var index in this.selectedPolyPoints)
                {
                    panel.DrawFillRegularPolygon(Color.Blue, this.polygon.Points[index], radius, 7);
                }
            }

            panel.FlipBuffer();
        }
    }
}
