using DynamicData;
using GraphicMinimal;
using GraphicPanels;
using LevelEditorControl.LevelItems.Polygon;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WpfControls.Controls.CameraSetting;

namespace LevelEditorControl.EditorFunctions
{
    internal class AddPolygonFunction : DummyFunction, IEditorFunction
    {
        private static readonly float MinPointDistance = 5;

        private EditorState state;
        private List<Vector2D> points = new List<Vector2D>();
        private Vector2D currentMousePosition = new Vector2D(0, 0);
        private bool currentLineIntersects = false; //Gibt es ein Schnittpunkt zwischen der Linie: points.Last-currentMousePosition und den Linine points[0..Lenght-2]
        private Action isFinish;

        public override FunctionType Type { get; } = FunctionType.AddPolygon;

        public override IEditorFunction Init(EditorState state)
        {
            this.state = state;
            return this;
        }

        public IEditorFunction Init(EditorState state, Action isFinish)
        {
            this.state = state;
            this.isFinish = isFinish;
            return this;
        }

        public override void HandleTimerTick(float dt)
        {
            Refresh(state.Panel, state.Camera);
        }

        public override void HandleMouseMove(MouseEventArgs e)
        {
            var point = state.Camera.PointToCamera(new PointF(e.X, e.Y)).ToGrx();
            point = state.Grid.SnapMouse(point);

            currentMousePosition = point;

            this.currentLineIntersects = false;
            for (int i = 0; i < points.Count - 2; i++)
            {
                if (MathHelper.IntersectLines(points[i], points[i + 1], points.Last(), currentMousePosition))
                {
                    this.currentLineIntersects = true;
                    break;
                }
            }

            if (this.points.Any() && (this.points.Last() - currentMousePosition).Length() < MinPointDistance)
                this.currentLineIntersects = true;

            Refresh(state.Panel, state.Camera);
        }

        public override void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                CreatePolygon();
            }
        }

        public override void HandleMouseClick(System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && this.currentLineIntersects == false)
            {
                var point = state.Camera.PointToCamera(new PointF(e.X, e.Y)).ToGrx();
                point = state.Grid.SnapMouse(point);
                points.Add(point);
            }

            if (e.Button == MouseButtons.Right)
            {
                CreatePolygon();
            }
        }

        private void CreatePolygon()
        {
            if (this.points.Count >= 3)
            {
                int newId = this.state.GenerateLevelItemId();
                this.state.AddLevelItem(new PolygonLevelItem(points.ToArray(), this.state.PolygonImages, newId));
                PolygonLevelItem.UpdateIsOutsideAndUVFromAllPolygons(this.state.LevelItems.Where(x => x is PolygonLevelItem).Cast<PolygonLevelItem>().ToList());
                this.points.Clear();
                this.isFinish();
            }
        }

        private void Refresh(GraphicPanel2D panel, Camera2D camera)
        {
            state.DrawItems();

            if (this.points.Any())
            {
                for (int i = 0; i < this.points.Count - 1; i++)
                {
                    panel.DrawLine(new Pen(Color.Black, 2), this.points[i], this.points[i + 1]);
                }

                panel.DrawLine(new Pen(this.currentLineIntersects ? Color.Red : Color.Black, 2), this.points.Last(), this.currentMousePosition);
            }
            panel.FlipBuffer();
        }
    }
}
