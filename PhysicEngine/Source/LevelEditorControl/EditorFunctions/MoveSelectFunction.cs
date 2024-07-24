using DynamicData;
using GraphicMinimal;
using LevelEditorControl.LevelItems;
using LevelEditorGlobal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LevelEditorControl.EditorFunctions
{
    //Per MouseDown können LevelItems verschoben werden und per MouseClick können sie selektiert werden
    internal class MoveSelectFunction : DummyFunction, IEditorFunction
    {
        private EditorState state;
        private Vector2D[] mouseDownToPivots = null; //Wenn ich mehrere Objekte verschieben will ist dass der Vektor vom PivotPunkt zum MouseDownpunkt
        private SmallWindow smallWindow;
        private Vector2D mouseDownPoint = null;
        private Vector2D mousePosition = null;

        public override FunctionType Type { get; } = FunctionType.MoveSelect;
        public override IEditorFunction Init(EditorState state)
        {
            this.state = state;
            this.smallWindow = new SmallWindow(state.Panel.Width, state.Panel.Height, state.Camera);
            this.state.UnselectAllItems();
            return this;
        }

        private ILevelItem GetItemFromPoint(Vector2D point)
        {
            List<KeyValuePair<float, ILevelItem>> list = new List<KeyValuePair<float, ILevelItem>>();
            foreach (var item in state.LevelItems)
            {
                if (item.IsPointInside(point))
                {
                    list.Add(new KeyValuePair<float, ILevelItem>(item.GetArea(), item));
                }
            }

            if (list.Any() == false) return null;

            var selectedShape = list.OrderBy(x => x.Key).First().Value;
            return selectedShape;
        }

        private void SelectSingleItem(ILevelItem item)
        {
            this.state.UnselectAllItems();
            item.IsSelected = true;
            this.state.SetSelectionState(item, true); //Select new item

        }

        public override void HandleSizeChanged(int width, int height)
        {
            this.smallWindow.HandleSizeChanged(width, height);
        }

        public override void HandleMouseClick(MouseEventArgs e)
        {
            this.smallWindow.HandleMouseClick(e);
        }

        public override void HandleMouseDown(MouseEventArgs e)
        {
            if (this.smallWindow.HandleMouseDown(e)) return;

            var point = state.Camera.PointToCamera(new PointF(e.X, e.Y)).ToGrx();

            this.mouseDownPoint = point;
            this.mousePosition = point;

            this.mouseDownToPivots = null;

            var item = GetItemFromPoint(point);

            if (item == null)
            {
                this.state.UnselectAllItems();
            }

            //Ist nur ein Item selektiert?
            if (this.state.SelectedItems.Count <= 1)
            {
                //Wurde auf ein Item geklickt?
                if (item != null)
                {
                    //Selektiere das neue Item
                    SelectSingleItem(item);
                }

            }
            else
            {
                //Meherere Items sollen verschoben werden
                this.mouseDownToPivots = this
                    .state
                    .SelectedItems
                    .Items
                    .Where(x => x.PivotPoint != null) //Beim LawnEdge ist der PivotPoint null und es läßt sich nicht verschieben
                    .Select(x => x.PivotPoint - mouseDownPoint)
                    .ToArray();
            }



        }

        public override void HandleMouseMove(MouseEventArgs e)
        {
            if (this.smallWindow.HandleMouseMove(e)) return;

            var point = state.Camera.PointToCamera(new PointF(e.X, e.Y)).ToGrx();

            this.mousePosition = point;

            if (this.mouseDownPoint != null)
            {
                //Selektionsrechteck aufspannen
                if (this.state.SelectedItems.Count == 0)
                {
                    //Prüfe für die 4 Eckpunkte von jeden LevelItem, ob es im Rechteck liegt
                    var rec = LevelItemsHelper.CreateRectangle(this.mouseDownPoint, this.mousePosition);
                    foreach (var item in this.state.LevelItems)
                    {
                        item.IsSelected = false;
                        var points = item.GetCornerPoints();
                        foreach (var p in points)
                        {
                            if (LevelItemsHelper.IsPointInRectangle(rec, p))
                            {
                                item.IsSelected = true;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    //Mehrere Objekte verschieben
                    if (this.mouseDownToPivots != null)
                    {
                        for (int i = 0; i < this.state.SelectedItems.Count; i++)
                        {
                            var item = this.state.SelectedItems.Items.ToList()[i];
                            if (item.PivotPoint != null)
                            {
                                item.PivotPoint = state.Grid.SnapMouse(point + this.mouseDownToPivots[i]);
                            }
                        }
                    }
                }

            }
        }

        public override void HandleMouseWheel(MouseEventArgs e)
        {
            this.smallWindow.HandleMouseWheel(e);
        }

        public override void HandleMouseUp(MouseEventArgs e)
        {
            this.smallWindow.HandleMouseUp(e);
            this.mouseDownPoint = null;

            //Während die Selektionsbox aufgezogen wird verändert sie nur  die IsSelected-Property
            //Hier erfolgt nun das Update der SelectedItems-Variable indem all die Items in hinzugefügt werden, welche noch fehlen
            var selected = this.state.LevelItems.Where(x => x.IsSelected).Where(x => this.state.SelectedItems.Items.Contains(x) == false).ToList();
            if (selected.Any())
            {
                this.state.SelectedItems.AddRange(selected);
            }
        }

        public override void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Delete && this.state.SelectedItems.Items.Any())
            {
                this.state.RemoveLevelItemsFromEditorState(this.state.SelectedItems.Items);
            }
        }

        public override void HandleTimerTick(float dt)
        {
            var panel = this.state.Panel;

            state.DrawItems();
            state.DrawSmallWindow(this.smallWindow);

            //Selektionsrechteck zeichnen
            if (this.mouseDownPoint != null && this.mousePosition != null && this.state.SelectedItems.Count == 0)
            {
                panel.DisableDepthTesting();
                Vector2D min = new Vector2D(Math.Min(this.mouseDownPoint.X, this.mousePosition.X), Math.Min(this.mouseDownPoint.Y, this.mousePosition.Y));
                Vector2D max = new Vector2D(Math.Max(this.mouseDownPoint.X, this.mousePosition.X), Math.Max(this.mouseDownPoint.Y, this.mousePosition.Y));
                panel.DrawRectangle(Pens.Black, min.Xi, min.Yi, (int)(max.X - min.X), (int)(max.Y - min.Y));
            }

            panel.FlipBuffer();
        }
    }
}
