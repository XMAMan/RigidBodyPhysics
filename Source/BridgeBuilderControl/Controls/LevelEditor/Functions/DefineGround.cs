using BridgeBuilderControl.Controls.Helper;
using GraphicMinimal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BridgeBuilderControl.Controls.LevelEditor.Functions
{
    //Beschreibt aus dem EditorState die Property: GroundPolygon
    internal class DefineGround : IEditorFunction
    {
        private List<Point> points = new List<Point>();

        private EditorState state;
        private Vector2D mousePosition = new Vector2D(0, 0);
        public FunctionType Type { get => FunctionType.DefineGround; }
        public IEditorFunction Init(EditorState state)
        {
            this.state = state;

            if (state.GroundPolygon != null)
            {
                this.points = state.GroundPolygon.ToList();
            }
            
            return this;
        }

        private void Refresh()
        {
            var panel = this.state.Panel;
            DrawingHelper.DrawBackgroundOrClipboardImage(panel, state.XCount, state.YCount, state.WaterHeight, state.GroundHeight);


            var mouse = GetPolygonPoint(mousePosition);
            Color inactivePoint = Color.FromArgb(125, 127, 127);
            DrawingHelper.DrawPolyPoint(panel, inactivePoint, mouse, state.GroundHeight);
            DrawingHelper.DrawGroundBorder(panel, this.points, state.GroundHeight, state.XCount);

            panel.FlipBuffer();
        }

        public void HandleTimerTick(float dt)
        {
            Refresh();
        }

        public void HandleMouseClick(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.points.Add(GetPolygonPoint(mousePosition));
                this.state.GroundPolygon = this.points.ToArray();
            }     
            
            if (e.Button == MouseButtons.Right)
            {
                int index = GetPointIndex(new Vector2D(e.X, e.Y));
                if (index != -1)
                {
                    this.points.RemoveAt(index);
                    this.state.GroundPolygon = this.points.ToArray();
                }
            }
        }

        public void HandleMouseMove(MouseEventArgs e)
        {
            this.mousePosition = new Vector2D(e.X, e.Y);
        }

        //Gibt den nächsten Punkt zurück, wo ein Polygonpunkt erzeugt werden darf
        private Point GetPolygonPoint(Vector2D pixelPosition)
        {
            //Es soll der erste Polygonpunkt definiert werden:
            if (this.points.Count == 0)
            {
                var firstPoint = PixelPointToPolyPoint(pixelPosition);

                //Der erste Polygon-Punkt muss entweder ganz links auf ein beliebigen Gitterpunkt starten
                if (firstPoint.X > 0)
                {
                    return new Point(firstPoint.X, 0);
                }else
                {
                    //Oder auf der GroundHeight-Linie
                    return firstPoint;
                }
            }

            //Wenn es bereits Punkte gibt, dann darf ein Punkt immer nur rechts neben den letzten Punkt platziert werden
            var snapPoint = PixelPointToPolyPoint(pixelPosition);
            return new Point(Math.Max(this.points.Last().X + 1, snapPoint.X), snapPoint.Y);
        }

        private Point PixelPointToPolyPoint(Vector2D pixelPosition)
        {
            var snapPoint = state.MouseGrid.SnapToInt(pixelPosition);
            return new Point(snapPoint.X, snapPoint.Y - (int)state.GroundHeight);
        }

        //Gibt den Index des Punktes vom GroundPolygon zurück, wo die Maus gerade ist
        private int GetPointIndex(Vector2D pixelPosition)
        {
            var point = PixelPointToPolyPoint(pixelPosition);
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
