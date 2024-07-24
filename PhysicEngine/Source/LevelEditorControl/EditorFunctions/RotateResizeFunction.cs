using GraphicMinimal;
using LevelEditorControl.Controls.RotateResizeControl;
using LevelEditorControl.LevelItems;
using LevelEditorGlobal;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LevelEditorControl.EditorFunctions
{
    //Hiermit kann man für ein LevelItem festelegen, was sein Rotations/Skalierungspunkt ist und welche Größe/Ausrichtung das Objekt hat
    internal class RotateResizeFunction : DummyFunction, IEditorFunction
    {
        private readonly float CircleRadius = 8;
        private EditorState state;
        private RotatedRectangle rec;
        private Vector2D mouseOverPoint = null;
        private Vector2D mouseDownPoint = null;
        private Vector2D mouseDownPivot = null;
        private bool pivotPointWasPressedDurringMouseDown = false;
        private float mouseDownSize = 0;
        private float mouseDownAngle = 0;
        private bool shiftIsPressed = false;
        private bool ctrlIsPressed = false;
        private Action<MouseEventArgs?> isFinish;
        private RotateResizeViewModel vm;

        public override FunctionType Type { get; } = FunctionType.RotateResize;
        public override IEditorFunction Init(EditorState state)
        {
            throw new NotImplementedException();
        }

        public IEditorFunction Init(EditorState state, Action<MouseEventArgs?> isFinish)
        {
            this.state = state;
            this.isFinish = isFinish;
            var item = (IRotateableLevelItem)this.state.SelectedItems.Items.First();
            this.rec = item.RotatedRectangle;
            this.HasPropertyControl = true;
            return this;
        }

        public override System.Windows.Controls.UserControl GetPropertyControl()
        {
            IPrototypItem associatedPrototyp = null;
            if (this.state.SelectedItems.Items.First() is IPrototypLevelItem)
            {
                associatedPrototyp = ((IPrototypLevelItem)this.state.SelectedItems.Items.First()).AssociatedPrototyp;
            }
            this.vm = new RotateResizeViewModel(this.rec, associatedPrototyp);

            return new RotateResizeControl() { DataContext = vm };
        }

        public override void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.LeftShift)
                this.shiftIsPressed = true;

            if (e.Key == System.Windows.Input.Key.LeftCtrl)
                this.ctrlIsPressed = true;

            if (e.Key == System.Windows.Input.Key.Delete && this.state.SelectedItems.Items.Any())
            {
                this.state.RemoveLevelItemsFromEditorState(this.state.SelectedItems.Items);
                this.isFinish(null);
            }
        }

        public override void HandleKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.LeftShift)
                this.shiftIsPressed = false;

            if (e.Key == System.Windows.Input.Key.LeftCtrl)
                this.ctrlIsPressed = false;
        }

        public override void HandleMouseMove(MouseEventArgs e)
        {
            var point = state.Camera.PointToCamera(new PointF(e.X, e.Y)).ToGrx();


            if (e.Button == MouseButtons.None)
            {
                this.mouseOverPoint = null;

                float radius = this.state.Camera.LengthToCamera(CircleRadius);

                var pivotPoints = GetPivotPoints(this.rec.GetCornerPoints());
                foreach (var pivotPoint in pivotPoints)
                {
                    if ((point - pivotPoint).Length() < radius)
                    {
                        this.mouseOverPoint = pivotPoint;
                        break;
                    }
                }
            }

            //Die gedrückte Maustaste wird verschoben
            if (e.Button == MouseButtons.Left && this.mouseDownPoint != null)
            {


                //Es wird ein Pivotpunkt festgehalten -> Rotieren/skalieren
                if (this.pivotPointWasPressedDurringMouseDown)
                {
                    point = state.Grid.SnapMouse(point);

                    if (this.shiftIsPressed == false)
                    {
                        //Skaliere wenn Shift nicht gedrückt ist
                        var diff = (point - this.mouseDownPivot).Length();
                        float startDiff = (this.mouseDownPoint - this.mouseDownPivot).Length();
                        float factor = (diff / startDiff);
                        this.vm.Size = this.mouseDownSize * factor;
                    }
                    else
                    {
                        //Rotiere wenn Shift gedrückt ist
                        float angle = Vector2D.Angle360(new Vector2D(1, 0), point - this.mouseDownPivot);
                        float startAngle = Vector2D.Angle360(new Vector2D(1, 0), this.mouseDownPoint - this.mouseDownPivot);
                        float angleDiff = angle - startAngle;
                        float newAngle = this.mouseDownAngle + angleDiff;
                        if (newAngle > 180) newAngle -= 360;
                        if (newAngle < -180) newAngle += 360;
                        this.vm.Angle = newAngle;
                    }
                }
                else
                {
                    //Es wurde auf Objekt neben die Punkte gedrückt -> Verschiebe das Objekt
                    var delta = this.mouseDownPoint - this.mouseDownPivot;
                    this.rec.PivotPoint = state.Grid.SnapMouse(point - delta);
                }
            }

        }

        public override void HandleMouseClick(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                //Pivot-Punkt definieren
                if (this.mouseOverPoint != null && this.ctrlIsPressed)
                {
                    this.rec.DefinePivotPoint(this.mouseOverPoint);
                }
            }
        }

        public override void HandleMouseDown(MouseEventArgs e)
        {
            var point = state.Camera.PointToCamera(new PointF(e.X, e.Y)).ToGrx();

            this.mouseDownPoint = point;
            this.mouseDownPivot = this.rec.PivotPoint;
            this.mouseDownSize = this.rec.SizeFactor;
            this.mouseDownAngle = this.rec.AngleInDegree;

            //Jemand hat ein Pivotpoint angeklickt und möchte das Objekt skalieren/rotieren
            this.pivotPointWasPressedDurringMouseDown = e.Button == MouseButtons.Left && this.mouseOverPoint != null && this.ctrlIsPressed == false;
        }

        public override void HandleMouseUp(MouseEventArgs e)
        {
            this.mouseDownPivot = null;

            if (e.Button == MouseButtons.Left && this.ctrlIsPressed == false && this.shiftIsPressed == false)
            {
                var point = state.Camera.PointToCamera(new PointF(e.X, e.Y)).ToGrx();
                var pivotPoints = GetPivotPoints(this.rec.GetCornerPoints());
                var minDistance = pivotPoints.Select(x => (point - x).Length()).ToList().Min();
                float distanceToQuit = state.Camera.LengthToCamera(50);
                if (minDistance > distanceToQuit && this.rec.IsPointInside(point) == false)
                {
                    this.isFinish(e); //Beende die Funktion, wenn ins Leere geklickt wurde
                }
            }
        }

        public override void HandleTimerTick(float dt)
        {
            var panel = this.state.Panel;

            float radius = this.state.Camera.LengthToCamera(CircleRadius);

            state.DrawItems();

            var cornerPoints = this.rec.GetCornerPoints();
            var pivotPoints = GetPivotPoints(this.rec.GetCornerPoints());

            panel.DrawPolygon(new Pen(Color.Green, 3), cornerPoints.ToList());

            foreach (var point in pivotPoints)
            {
                //Zeichne die Punkte lila, wenn ein neuer Pivot-Punkt definiert werden soll
                panel.DrawFillCircleWithTriangles(this.ctrlIsPressed ? Color.Purple : Color.Green, point, radius, 10);
            }
            if (this.mouseOverPoint != null && this.mouseDownPoint == null)
                panel.DrawFillCircleWithTriangles(Color.Blue, this.mouseOverPoint, radius, 10); //Mouserover-Punkt blau

            panel.DrawFillCircleWithTriangles(Color.Red, this.rec.PivotPoint, radius, 10); //Pivot-Punkt rot

            panel.FlipBuffer();
        }

        //Gibt alle möglichen Punkte zurück, wo ein Pivot-Punkt definiert werden kann
        private Vector2D[] GetPivotPoints(Vector2D[] cornerPoints)
        {
            return new Vector2D[]
            {
                cornerPoints[0],
                cornerPoints[1],
                cornerPoints[2],
                cornerPoints[3],
                (cornerPoints[0] + cornerPoints[1]) / 2,
                (cornerPoints[1] + cornerPoints[2]) / 2,
                (cornerPoints[2] + cornerPoints[3]) / 2,
                (cornerPoints[3] + cornerPoints[0]) / 2,
                (cornerPoints[0] + cornerPoints[2]) / 2,
            };

        }
    }
}
