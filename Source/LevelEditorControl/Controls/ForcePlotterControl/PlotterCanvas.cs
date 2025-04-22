using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Brushes = System.Windows.Media.Brushes;
using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;

namespace LevelEditorControl.Controls.ForcePlotterControl
{
    //Hiermit kann ein Signal geplottet werden, was aus lauter Floats-Samples besteht
    //Quelle für FrameworkElement: https://www.codeproject.com/Tips/1197911/Using-DrawingContext-to-create-custom-drawn-elemen
    public class PlotterCanvas : FrameworkElement
    {
        private Point mousePoint = new Point(0, 0);
        protected override void OnMouseMove(MouseEventArgs e)
        {
            this.mousePoint = e.GetPosition(this);
            Update();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            //Wenn man das MouseMove-Event verarbeiten will, dann benötigt man ein Hintergrund: Mit dieser Anweisung erstelle ich den Hintergrund:
            //Quelle: https://stackoverflow.com/questions/27153558/why-mousemove-is-not-fired-on-a-custom-frameworkelement
            drawingContext.DrawRectangle(Brushes.Transparent, new Pen(Brushes.Transparent, 1), new Rect(0, 0, this.ActualWidth, this.ActualHeight));

            base.OnRender(drawingContext);

            //drawingContext.DrawLine(new Pen(Brushes.Black, 1.0), new System.Windows.Point(0, 0), new System.Windows.Point(this.ActualWidth, this.ActualHeight));

            if (Numbers == null) return;

            //So gibt man Text aus:
            //FormattedText formattedText = new FormattedText(Numbers.Count+"", CultureInfo.GetCultureInfo("en-us"),
            //    FlowDirection.LeftToRight, new Typeface("Verdana"), 16, Brushes.Black,
            //    VisualTreeHelper.GetDpi(this).PixelsPerDip);

            //drawingContext.DrawText(formattedText, new Point(0, 0));

            float range = (this.MaxValue - this.MinValue);
            Point lastPoint;
            var pen = new Pen(Brushes.Black, 1.0);
            int indexNearestToMouse = -1;
            double xDiffNearestToMouse = double.MaxValue;
            for (int i = 0; i < Numbers.Count; i++)
            {
                double x = i / (double)this.MaxTime;
                double y = (Numbers[i] - this.MinValue) / range;

                x *= ActualWidth;
                y *= ActualHeight;
                y = ActualHeight - y;

                var point = new Point(x, y);
                if (i > 0)
                {
                    drawingContext.DrawLine(pen, lastPoint, point);
                }
                lastPoint = point;

                double diff = Math.Abs(mousePoint.X - x);
                if (diff < xDiffNearestToMouse)
                {
                    xDiffNearestToMouse = diff;
                    indexNearestToMouse = i;
                }
            }

            drawingContext.DrawLine(new Pen(Brushes.Red, 1.0), new Point(mousePoint.X, 0), new Point(mousePoint.X, ActualHeight));

            float newSelectedValue = float.NaN;
            if (xDiffNearestToMouse < 10 && indexNearestToMouse != -1)
            {
                double x = indexNearestToMouse / (double)this.MaxTime;
                double y = (Numbers[indexNearestToMouse] - this.MinValue) / range;

                x *= ActualWidth;
                y *= ActualHeight;
                y = ActualHeight - y;

                drawingContext.DrawEllipse(Brushes.Transparent, new Pen(Brushes.Red, 1.0), new Point(x, y), 5, 5);
                newSelectedValue = Numbers[indexNearestToMouse];
            }
            this.SelectedValue = newSelectedValue;
        }

        private void Update(int addedItems = 0)
        {
            if (this.Numbers == null || this.Numbers.Count == 0)
            {
                this.MinValue = float.MaxValue;
                this.MaxValue = float.MinValue;
            }
            else
            {
                //Es wurde 1 Item durch den TimerTick-Handler hinzugefügt -> Ermittle den MinMax-Wert indem nur aufs letzte Item geschaut wird
                if (addedItems == 1)
                {
                    this.MinValue = Math.Min(this.MinValue, this.Numbers.Last());
                    this.MaxValue = Math.Max(this.MaxValue, this.Numbers.Last());
                }

                //Es wurden mehrere Items hinzugefügt -> Ermittle den MinMax-Wert indem auf alle Items geschaut wird
                if (addedItems > 1)
                {
                    this.MinValue = this.Numbers.Min(x => x);
                    this.MaxValue = this.Numbers.Max(x => x);
                }
            }

            this.InvalidateVisual();
        }

        #region Numbers-Property
        public static readonly DependencyProperty NumbersProperty =
            DependencyProperty.Register("Numbers", typeof(ObservableCollection<float>), typeof(PlotterCanvas),
         new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(ChangeNumbers)));
        private static void ChangeNumbers(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //https://stackoverflow.com/questions/4362278/observablecollection-dependency-property-does-not-update-when-item-in-collection
            if (e.OldValue != e.NewValue)
            {
                (d as PlotterCanvas).Update(); //Liste wurde erzeugt (Mit initialen Werten)
            }

            if (e.OldValue != null)
            {
                var coll = (INotifyCollectionChanged)e.OldValue;
                coll.CollectionChanged -= (d as PlotterCanvas).Numbers_CollectionChanged; // Unsubscribe from CollectionChanged on the old collection
            }

            if (e.NewValue != null)
            {
                var coll = (INotifyCollectionChanged)e.NewValue;
                coll.CollectionChanged += (d as PlotterCanvas).Numbers_CollectionChanged; // Subscribe to CollectionChanged on the new collection
            }
        }
        //Neues Element wurde in Liste hinzugefügt
        private void Numbers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Update(e.Action == NotifyCollectionChangedAction.Add ? e.NewItems.Count : 0);
        }

        public ObservableCollection<float> Numbers
        {
            get => (ObservableCollection<float>)GetValue(NumbersProperty);
            set => SetValue(NumbersProperty, value);
        }
        #endregion

        #region MaxTime-Property
        public static readonly DependencyProperty MaxTimeProperty =
            DependencyProperty.Register("MaxTime", typeof(int), typeof(PlotterCanvas),
         new FrameworkPropertyMetadata((int)1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ChangeMaxTime)));
        private static void ChangeMaxTime(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                (d as PlotterCanvas).Update();
            }
        }
        public int MaxTime
        {
            get => (int)GetValue(MaxTimeProperty);
            set => SetValue(MaxTimeProperty, value);
        }
        #endregion

        #region MinValue-Property
        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(float), typeof(PlotterCanvas),
         new FrameworkPropertyMetadata((float)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ChangeMinValue)));
        private static void ChangeMinValue(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                (d as PlotterCanvas).Update();
            }
        }
        public float MinValue
        {
            get => (float)GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }
        #endregion

        #region MaxValue-Property
        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(float), typeof(PlotterCanvas),
         new FrameworkPropertyMetadata((float)1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ChangeMaxValue)));
        private static void ChangeMaxValue(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                (d as PlotterCanvas).Update();
            }
        }
        public float MaxValue
        {
            get => (float)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }
        #endregion

        #region SelectedValue-Property
        public static readonly DependencyProperty SelectedValueProperty =
            DependencyProperty.Register("SelectedValue", typeof(float), typeof(PlotterCanvas),
         new FrameworkPropertyMetadata((float)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ChangeSelectedValue)));
        private static void ChangeSelectedValue(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                (d as PlotterCanvas).Update();
            }
        }
        public float SelectedValue
        {
            get => (float)GetValue(SelectedValueProperty);
            set => SetValue(SelectedValueProperty, value);
        }
        #endregion
    }
}
