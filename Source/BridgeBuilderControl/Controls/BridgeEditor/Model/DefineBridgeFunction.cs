using BridgeBuilderControl.Controls.Helper;
using BridgeBuilderControl.Controls.LevelEditor;
using BridgeBuilderControl.Controls.Simulator.Model;
using GraphicMinimal;
using GraphicPanels;
using RigidBodyPhysics.MathHelper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WpfControls.Controls.CameraSetting;

namespace BridgeBuilderControl.Controls.BridgeEditor.Model
{
    internal class DefineBridgeFunction
    {
        private GraphicPanel2D panel;
        private EditorCamera camera;
        private LevelExport levelData;
        private Vector2D mousePosition = new Vector2D(0, 0); //Maus-Position im Kameraspace
        private Vector2D screenMousePosition = new Vector2D(0, 0);
        enum State { PlacePoint1, PlacePoint2 };
        private State state = State.PlacePoint1;
        private List<Bar> bars = new List<Bar>();
        private Action barCountChanged;
        private Point point1;
        
        //Über diesen Punkt/Bereich ist gerade die Maus
        enum MouseHoverState { NoValue, FreeSpace, Ground, AnchorPoint, BridgePoint}
        private MouseHoverState mouseHoverState = MouseHoverState.NoValue;
        private List<Bar> selectedBars = new List<Bar>();

        public int BarCount { get => this.bars.Count; }

        public DefineBridgeFunction(GraphicPanel2D panel, LevelExport levelExport, Action barCountChanged)
        {
            this.panel = panel;
            this.levelData = levelExport;
            this.barCountChanged = barCountChanged;

            this.camera = new EditorCamera(panel.Width, panel.Height, levelExport.XCount, levelExport.YCount);
            this.camera.InitialPosition = Camera2D.InitialPositionIfAutoZoomIsActivated.ToLeftTopCorner;
        }

        public void Clear()
        {
            this.bars.Clear();
            this.state = State.PlacePoint1;
            this.barCountChanged();
        }

        public BridgeExport GetExport(string associatedLevel)
        {
            return new BridgeExport() {AssociatedLevel = associatedLevel, Bars = this.bars.ToArray() };
        }

        public SimulatorInput GetSimulatorInputData(string associatedLevel)
        {
            return new SimulatorInput()
            {
                Camera = this.camera,
                LevelExport = this.levelData,
                BridgeExport = GetExport(associatedLevel),
                LevelFile = associatedLevel
            };
        }

        public void LoadFromExport(BridgeExport bridgeExport)
        {
            this.bars = bridgeExport.Bars.ToList();
            this.state = State.PlacePoint1;
            this.barCountChanged();
        }

        public void ZoomIn()
        {
            this.camera.ZoomIn();
        }

        public void ZoomOut()
        {
            this.camera.ZoomOut();
        }

        private void Refresh()
        {
            var panel = this.panel;
            var lev = levelData;

            DrawingHelper.ClearScreen(panel);
            panel.MultTransformationMatrix(camera.GetPointToSceenMatrix());
            DrawingHelper.DrawBackground(panel, lev.XCount, lev.YCount, lev.WaterHeight, lev.GroundHeight);
            DrawingHelper.DrawGround(panel, lev.GroundPolygon, lev.GroundHeight, lev.XCount, lev.YCount);
            DrawingHelper.DrawAnchorPoints(panel, lev.AnchorPoints);

            var mouse = MouseGrid.SnapToInteger(this.mousePosition);

            foreach (var bar in bars)
            {
                DrawingHelper.DrawBar(panel, Color.Gray, bar.P1, bar.P2);
            }

            //Zeichne die selektierten Stangen
            foreach (var bar in bars)
            {
                bool isP1Selected = IsEqual(bar.P1, mouse);
                bool isP2Selected = IsEqual(bar.P2, mouse);

                if (this.state == State.PlacePoint1 && isP1Selected == false && isP2Selected == false)
                {
                    bool isLineSelected = this.selectedBars.Contains(bar);
                    if (isLineSelected)
                    {
                        DrawingHelper.DrawBar(panel, Color.Gray, Color.Gray, isLineSelected ? Color.White : Color.Gray, bar.P1, bar.P2);
                    }
                }
                              
            }

            if (IsValidBarPoint(this.mousePosition, this.state == State.PlacePoint1))
            {
                if (this.state == State.PlacePoint2)
                {
                    DrawingHelper.DrawBar(panel, Color.Gray, this.point1, mouse);
                }

                Color newPointColor = GetMouseHoverColor(this.mouseHoverState);
                DrawingHelper.DrawGridPoint(panel, newPointColor, mouse);
            }

            panel.FlipBuffer();
        }
        public void HandleTimerTick(float dt, bool zoomInIsPressed, bool zoomOutIsPressed)
        {
            this.camera.MoveCameraWithMouse(this.screenMousePosition); //Bewege die Kamera, wenn die Maus am Bildschirmrand ist
            this.camera.SetZoom(zoomInIsPressed, zoomOutIsPressed);
            Refresh();
        }

        public void HandleMouseClick(MouseEventArgs e)
        {
            //Neuen Brückenpunkt definieren
            if (e.Button == MouseButtons.Left)
            {
                if (this.state == State.PlacePoint1 && IsValidBarPoint(this.mousePosition, true))
                {
                    this.point1 = MouseGrid.SnapToInteger(this.mousePosition);

                    if (IsBudgedAvailable())
                    {
                        this.state = State.PlacePoint2;
                    }                   
                }else if (this.state == State.PlacePoint2 && IsValidBarPoint(this.mousePosition, false))
                {
                    var point2 = MouseGrid.SnapToInteger(this.mousePosition);
                    this.bars.Add(new Bar() { P1 = point1, P2 = point2 });
                    this.barCountChanged();
                    this.state = State.PlacePoint1;

                    if (IsValidBarPoint(this.mousePosition, true))
                    {
                        this.point1 = MouseGrid.SnapToInteger(this.mousePosition);

                        if (IsBudgedAvailable())
                        {
                            this.state = State.PlacePoint2;
                        }
                    }
                }
            }
            
            if (e.Button == MouseButtons.Right)
            {
                //Brückenpunkt/Stange löschen
                if (this.state == State.PlacePoint1)
                {
                    foreach (var bar in this.selectedBars)
                    {
                        this.bars.Remove(bar);
                    }
                    this.barCountChanged();
                }
                
                //Baue doch keine neue Stange
                if (this.state == State.PlacePoint2)
                {
                    this.state = State.PlacePoint1;
                }
            }
        }

        private bool IsBudgedAvailable()
        {
            int cost = this.BarCount * 100;
            return this.levelData.Budget > cost;
        }

        private List<Bar> GetSelectedBars(Vector2D pixelPosition)
        {
            List<Bar> bars = new List<Bar>();

            var mouse = MouseGrid.SnapToInteger(pixelPosition);
            foreach (var bar in this.bars)
            {
                if (IsEqual(mouse, bar.P1) || 
                    IsEqual(mouse, bar.P2) || 
                    MathHelper.IsPointAboveBar(pixelPosition, bar, 1, this.camera.LengthToCamera(1))
                    )
                {
                    bars.Add(bar);
                }
            }

            return bars;
        }

        //Darf an diesen Punkt das Endstück von einer Brückenstange definiert werden?
        private bool IsValidBarPoint(Vector2D pixelPoint, bool isPoint1)
        {
            var state = GetMouseHoverState(pixelPoint);
            if (isPoint1)
            {
                return state == MouseHoverState.AnchorPoint || state == MouseHoverState.BridgePoint;
            }else
            {
                if (state == MouseHoverState.Ground) return false;
                var point2 = MouseGrid.SnapToInteger(mousePosition);

                //Prüfe das P1 != P2 gilt
                if (IsEqual(this.point1, point2)) return false;

                //Prüfe dass dort nicht schon eine Stange ist
                if (IsP1P2LineAboveBar(point1, point2)) return false;

                //Maximallänge prüfen
                if (MathHelper.GetDistance(this.point1, point2) > 8.99f) return false;

                //Wenn der Endpunkt ein Ankerpunkt ist, dann prüfe nicht, ob die P1-P2-Linie den Boden schneidet                
                if (IsAnchorPoint(this.point1) || IsAnchorPoint(point2)) return true;

                //Prüfe ob die P1-P2-Linie das Boden-Polygon schneidet
                var p1 = MathHelper.GridToPixelPoint(this.point1, 1);
                var p2 = pixelPoint;
                var lev = this.levelData;
                var groundPoly = DrawingHelper.GetGroundPolygon(lev.GroundPolygon, lev.GroundHeight, lev.XCount, lev.YCount);

                return MathHelper.LineIntersectsPolygon(p1, p2, groundPoly) == false;
            }            
        }

        public void HandleMouseMove(MouseEventArgs e)
        {
            this.mousePosition = this.camera.PointToCamera(new PointF(e.X, e.Y)).ToGrx();
            //this.mousePosition = new Vector2D(e.X, e.Y);
            this.screenMousePosition = new Vector2D(e.X, e.Y);
            this.mouseHoverState = GetMouseHoverState(this.mousePosition);
            this.selectedBars = GetSelectedBars(this.mousePosition);
        }

        public void HandleSizeChanged(int width, int height)
        {
            this.camera.UpdateScreenSize(width, height);
        }

        

        private Color GetMouseHoverColor(MouseHoverState state)
        {
            switch (state)
            {
                case MouseHoverState.AnchorPoint:
                    return Color.Yellow;

                case MouseHoverState.BridgePoint:
                    return Color.White;

                case MouseHoverState.Ground:
                    return Color.DarkRed;
            }

            return Color.Gray;
        }

        private bool IsAnchorPoint(Point point)
        {
            foreach (var anchor in this.levelData.AnchorPoints)
            {
                if (IsEqual(point, anchor))
                    return true;
            }

            return false;
        }

        private bool IsP1P2LineAboveBar(Point p1, Point p2)
        {
            foreach (var bar in this.bars)
            {
                if (IsEqual(p1, bar.P1) && IsEqual(p2, bar.P2))
                    return true;

                if (IsEqual(p2, bar.P1) && IsEqual(p1, bar.P2))
                    return true;
            }

            return false;
        }

        private MouseHoverState GetMouseHoverState(Vector2D mousePosition)
        {
            var mouse = MouseGrid.SnapToInteger(mousePosition);

            //Schritt 1: Prüfe ob es über ein Ankerpunkt ist
            foreach (var anchor in this.levelData.AnchorPoints)
            {
                if (IsEqual(mouse, anchor))
                    return MouseHoverState.AnchorPoint;
            }

            //Schritt 2: Prüfe ob es über ein BridgePoint ist
            foreach (var bar in this.bars)
            {
                if (IsEqual(mouse, bar.P1) || IsEqual(mouse, bar.P2)) 
                    return MouseHoverState.BridgePoint;
            }

            //Schritt 3: Prüfe ob es innerhalb vom Ground-Polygon ist
            if (IsPointInGround(mousePosition))
                return MouseHoverState.Ground;

            return MouseHoverState.FreeSpace;
        }

        private static bool IsEqual(Point p1, Point p2)
        {
            return p1.X == p2.X && p1.Y == p2.Y;
        }

        private bool IsPointInGround(Vector2D point)
        {
            var groundPoly = DrawingHelper.GetGroundPolygon(this.levelData.GroundPolygon, this.levelData.GroundHeight, this.levelData.XCount, this.levelData.YCount);
            return PolygonHelper.PointIsInsidePolygon(groundPoly.ToPhx(), point.ToPhx());
        }
    }
}
