using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace KeyFrameEditorControl.Controls.KeyDefine
{
    //Hier kann eine Liste von Float-Werten definiert werden, welche alle im Bereich von MinValue bis MaxValue liegen
    //An jede Float-Zahl darf man ein Tag (object) hängen
    public class FloatListCanvas : Canvas
    {
        public class Entry
        {
            public float Value;
            public object Tag = null;

            public Entry(float value)
            {
                Value = value;
            }
        }

        private Rectangle selectedRectangle = null; //Bei diesen Rechteck ist die Maus im LeftMouseDown-Zustand

        public FloatListCanvas()
        {
            Background = Brushes.Beige;
            SizeChanged += FloatListCanvas_SizeChanged;

            MouseMove += FloatListCanvas_MouseMove;
            MouseDown += FloatListCanvas_MouseDown;
            MouseUp += FloatListCanvas_MouseUp;

            Numbers = new ObservableCollection<Entry>();

            Update();
        }

        private void FloatListCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            selectedRectangle = null;
        }

        private void FloatListCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                SelectedEntry = null;
            }

            if (e.RightButton == MouseButtonState.Pressed) //Neue Zahl erstellen
            {
                double pos = e.GetPosition(this).X / Width;
                pos = Math.Min(this.MaxValue, Math.Max(this.MinValue, pos));
                var newEntry = new Entry((float)pos);
                Numbers.Add(newEntry);

                Update();

                NewNumber.Execute(newEntry);
            }

        }

        private void FloatListCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectedRectangle != null && e.LeftButton == MouseButtonState.Pressed)
            {
                double pos = e.GetPosition(this).X / Width;
                pos = Math.Min(this.MaxValue, Math.Max(this.MinValue, pos));
                var entry = (Entry)selectedRectangle.Tag;
                entry.Value = (float)pos;

                SetLeft(selectedRectangle, NumberToPixelPosition(pos) - selectedRectangle.Width / 2);
            }
        }

        private void FloatListCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Update();
        }

        private void Update()
        {
            Children.Clear();
            if (Numbers == null) return;

            foreach (var entry in Numbers)
            {
                double x = NumberToPixelPosition(entry.Value);

                Brush brush = entry == SelectedEntry ? Brushes.Red : Brushes.Gray;
                var rec = AddRectangle(new Point(x - 4, 0), new Size(8, Height), 0, Brushes.Black, brush);
                rec.Tag = entry;
            }
        }

        private void UpdateSelectedEntry()
        {
            foreach (var entry in Numbers)
            {
                int index = Numbers.IndexOf(entry);
                var rec = (Rectangle)Children[index];

                Brush brush = entry == SelectedEntry ? Brushes.Red : Brushes.Gray;
                rec.Stroke = brush;
            }
        }

        private double NumberToPixelPosition(double f)
        {
            return Math.Min(Width, Math.Max(0, Width * (f - MinValue) / (MaxValue - MinValue)));
        }

        private Rectangle AddRectangle(Point p, Size s, int zIndex, Brush fill, Brush stroke)
        {
            Rectangle rec = new Rectangle() { Width = s.Width, Height = s.Height, Fill = fill, Stroke = stroke, StrokeThickness = 2 };
            SetLeft(rec, p.X);
            SetTop(rec, p.Y);
            SetZIndex(rec, zIndex);
            rec.MouseEnter += OnMouseEnter;
            rec.MouseLeave += OnMouseLeave;
            rec.MouseDown += Rec_MouseDown;
            Children.Add(rec);
            return rec;
        }

        private void Rec_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var rec = (Rectangle)sender;
            var entry = (Entry)rec.Tag;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                SelectedEntry = entry;
                selectedRectangle = rec;
            }

            if (e.RightButton == MouseButtonState.Pressed)
            {
                if (SelectedEntry == entry)
                    SelectedEntry = null;

                selectedRectangle = null;
                int index = Numbers.IndexOf(entry);
                Numbers.RemoveAt(index);
            }
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            var rec = (Rectangle)sender;
            rec.Fill = Brushes.Blue;
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            var rec = (Rectangle)sender;
            rec.Fill = Brushes.Black;
        }

        #region Numbers-Property
        public static readonly DependencyProperty NumbersProperty =
            DependencyProperty.Register("Numbers", typeof(ObservableCollection<Entry>), typeof(FloatListCanvas),
         new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(ChangeNumbers)));
        private static void ChangeNumbers(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //https://stackoverflow.com/questions/4362278/observablecollection-dependency-property-does-not-update-when-item-in-collection
            if (e.OldValue != e.NewValue)
            {
                (d as FloatListCanvas).Update(); //Liste wurde erzeugt (Mit initialen Werten)
            }

            if (e.OldValue != null)
            {
                var coll = (INotifyCollectionChanged)e.OldValue;
                coll.CollectionChanged -= (d as FloatListCanvas).Numbers_CollectionChanged; // Unsubscribe from CollectionChanged on the old collection
            }

            if (e.NewValue != null)
            {
                var coll = (INotifyCollectionChanged)e.NewValue;
                coll.CollectionChanged += (d as FloatListCanvas).Numbers_CollectionChanged; // Subscribe to CollectionChanged on the new collection
            }
        }
        //Neues Element wurde in Liste hinzugefügt
        private void Numbers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Update();
        }

        public ObservableCollection<Entry> Numbers
        {
            get => (ObservableCollection<Entry>)GetValue(NumbersProperty);
            set => SetValue(NumbersProperty, value);
        }
        #endregion

        #region MinValue-Property
        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(float), typeof(FloatListCanvas),
         new FrameworkPropertyMetadata((float)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ChangeMinValue)));
        private static void ChangeMinValue(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                (d as FloatListCanvas).Update();
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
            DependencyProperty.Register("MaxValue", typeof(float), typeof(FloatListCanvas),
         new FrameworkPropertyMetadata((float)1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ChangeMaxValue)));
        private static void ChangeMaxValue(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                (d as FloatListCanvas).Update();
            }
        }
        public float MaxValue
        {
            get => (float)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }
        #endregion

        #region SelectedEntry-Property
        public static readonly DependencyProperty SelectedEntryProperty =
            DependencyProperty.Register("SelectedEntry", typeof(Entry), typeof(FloatListCanvas),
         new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ChangeSelectedEntry)));
        private static void ChangeSelectedEntry(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                (d as FloatListCanvas).UpdateSelectedEntry();
            }
        }
        public Entry SelectedEntry
        {
            get => (Entry)GetValue(SelectedEntryProperty);
            set => SetValue(SelectedEntryProperty, value);
        }
        #endregion

        //Quelle: https://stackoverflow.com/questions/29126224/how-do-i-bind-wpf-commands-between-a-usercontrol-and-a-parent-window
        #region NewNumber-Command
        public static readonly DependencyProperty NewNumberProperty =
            DependencyProperty.Register(
                "NewNumber",
                typeof(ICommand),
                typeof(FloatListCanvas),
                new UIPropertyMetadata(null));
        public ICommand NewNumber
        {
            get { return (ICommand)GetValue(NewNumberProperty); }
            set { SetValue(NewNumberProperty, value); }
        }
        #endregion
    }
}
