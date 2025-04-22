using BridgeBuilderControl.Controls.Helper;
using GraphicMinimal;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BridgeBuilderControl.Controls.LevelEditor.Functions
{
    //Beschreibt aus dem EditorState die Property: AnchorPoints
    internal class DefineAnchorPoints : IEditorFunction
    {
        private List<Point> points = new List<Point>();

        private EditorState state;
        private Vector2D mousePosition = new Vector2D(0, 0);
        public FunctionType Type { get => FunctionType.DefineAnchorPoints; }
        public IEditorFunction Init(EditorState state)
        {
            this.state = state;

            if (state.AnchorPoints != null)
            {
                this.points = state.AnchorPoints.ToList();
            }

            return this;
        }
        public void HandleTimerTick(float dt)
        {
            var panel = this.state.Panel;
            DrawingHelper.DrawBackgroundOrClipboardImage(panel, state.XCount, state.YCount, state.WaterHeight, state.GroundHeight);
            DrawingHelper.DrawGround(panel, state.GroundPolygon, state.GroundHeight, state.XCount, state.YCount);
            DrawingHelper.DrawAnchorPoints(panel, state.AnchorPoints);

            var mouse = state.MouseGrid.SnapToInt(mousePosition);
            Color newPointColor = Color.Yellow;
            DrawingHelper.DrawGridPoint(panel, newPointColor, mouse);

            panel.FlipBuffer();
        }
        public void HandleMouseClick(System.Windows.Forms.MouseEventArgs e) 
        {
            //Punkt erzeugen
            if (e.Button == MouseButtons.Left)
            {
                var mouse = new Vector2D(e.X, e.Y);
                if (GetPointIndex(mouse) == -1) // An der Klickstelle exisitert noch kein Ankerpunkt
                {
                    this.points.Add(state.MouseGrid.SnapToInt(mouse)); //Erzeuge ein neuen Ankerpunkt
                    this.state.AnchorPoints = this.points.ToArray();
                }
            }

            //Punkt löschen
            if (e.Button == MouseButtons.Right)
            {
                var mouse = new Vector2D(e.X, e.Y);
                int index = GetPointIndex(mouse);
                if (index != -1) // An der Klickstelle exisitert ein Ankerpunkt
                {
                    this.points.RemoveAt(index); //Lösche diesen Punkt
                    this.state.AnchorPoints = this.points.ToArray();
                }
            }
        }



        public void HandleMouseMove(System.Windows.Forms.MouseEventArgs e) 
        {
            this.mousePosition = new Vector2D(e.X, e.Y);
        }

        //Gibt den Index des Punktes von this.points zurück, wo die Maus gerade ist
        private int GetPointIndex(Vector2D pixelPosition)
        {
            var point = state.MouseGrid.SnapToInt(pixelPosition);
            for (int i = 0; i < this.points.Count; i++)
            {
                var p = this.points[i];
                if (p.X == point.X && p.Y == point.Y)
                    return i;
            }
            return -1;
        }
    }
}
