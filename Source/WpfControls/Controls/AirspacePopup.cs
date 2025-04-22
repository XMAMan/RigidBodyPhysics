using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace WpfControls.Controls
{
    //Normalerweise darf man über ein WindowsFormsHost nichts drüber zeichnen. Wenn ich aber ein Button über mein GraphicPanel
    //zeichnen will, dann muss ich den Button innerhalb vom AirspacePopup definieren (Extra-Fenster was darüber liegt)
    //https://stackoverflow.com/questions/6087835/can-i-overlay-a-wpf-window-on-top-of-another/6452940#6452940

    //Ein Popup-Fenster darf nicht mehr als 75% des Bildschirms bedecken. Siehe hier: https://stackoverflow.com/questions/23904184/need-to-make-full-screen-popup-in-wpf
    //public class Popup : FrameworkElement, IAddChild
    //{
    //   private const double RestrictPercentage = 0.75; // This is how much the max dimensions will be reduced by
    //https://referencesource.microsoft.com/#PresentationFramework/src/Framework/System/Windows/Controls/Primitives/Popup.cs,2957
    //Konstanten darf man per Reflection nicht ändern, weil dessen Werte direkt in den IL-Code kopiert werden.
    //Man darf aber Methoden von einer Klasse austauschen, indem man die Startadresse der Methode in der Sprungtabelle
    //mit einer Hook-Methode ersetzt. Im Projekt "MethodSwapWithReflection" was ich von hier habe:
    //https://stackoverflow.com/questions/7299097/dynamically-replace-the-contents-of-a-c-sharp-method zeige ich
    //wie das geht. Ich habe dann versucht die RestrictSize-Methode von der Popup-Klasse zu ersetzen aber diese
    //Methode wird komischwerweise nicht angesprungen oder meine Ersetzung geht nicht. 

    //Wegen der 75%-Regel habe ich hier noch die CoversEntireTarget-Property eingeführt
    //Wenn dieser Schalter auf true ist, dann muss man für jeden Button ein eigenes AirspacePopup erzeugen.


    //Es gibt also zwei Modi für diese Klasse:
    //Modus 1(CoversEntireTarget = true): Ein AirspacePopup bedeckt das gesamte Grafikpanel und innerhalb von
    //         diesen Popup befindet sich ein Grid, wo ich dann mehrere Buttons platzieren darf
    //         -> Geht nur für Fenster < 75%
    // -> Beispiel: Airspace_EntireScreen_Editor_XAML.txt, Airspace_EntireScreen_Simulator_XAML.txt
    //Modus 2(CoversEntireTarget = false): Pro Button erzeuge ich ein eigenes AirspacePopup welches an den
    //  GraphicControl an der linken oberen Ecke hängt. Ich platziere dann das Popup über
    //  HorizontalAlignment,VerticalAlignment, Margin   
    // -> Beispiel: Airspace_NoEntireScreen_Editor_XAML.txt, Airspace_NoEntireScreen_Simulator_XAML.txt

    //So funktioniert beim Popup das Placement: https://www.c-sharpcorner.com/UploadFile/mahesh/using-xaml-popup-in-wpf/
    public class AirspacePopup : Popup
    {
        public static readonly DependencyProperty CoversEntireTargetProperty =
            DependencyProperty.Register("CoversEntireTarget",
                                        typeof(bool),
                                        typeof(AirspacePopup),
                                        new FrameworkPropertyMetadata(false, OnCoversEntireTargetChanged));

        public static readonly DependencyProperty IsTopmostProperty =
            DependencyProperty.Register("IsTopmost",
                                        typeof(bool),
                                        typeof(AirspacePopup),
                                        new FrameworkPropertyMetadata(false, OnIsTopmostChanged));

        public static readonly DependencyProperty FollowPlacementTargetProperty =
            DependencyProperty.RegisterAttached("FollowPlacementTarget",
                                                typeof(bool),
                                                typeof(AirspacePopup),
                                                new UIPropertyMetadata(false));

        public static readonly DependencyProperty AllowOutsideScreenPlacementProperty =
            DependencyProperty.RegisterAttached("AllowOutsideScreenPlacement",
                                                typeof(bool),
                                                typeof(AirspacePopup),
                                                new UIPropertyMetadata(false));

        public static readonly DependencyProperty ParentWindowProperty =
            DependencyProperty.RegisterAttached("ParentWindow",
                                                typeof(Window),
                                                typeof(AirspacePopup),
                                                new UIPropertyMetadata(null, ParentWindowPropertyChanged));

        private static void OnIsTopmostChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            AirspacePopup airspacePopup = source as AirspacePopup;
            airspacePopup.UpdatePopupPosition();
        }

        private static void OnCoversEntireTargetChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            AirspacePopup airspacePopup = source as AirspacePopup;
            airspacePopup.SetTopmostState(airspacePopup.IsTopmost);
        }

        private static void ParentWindowPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            AirspacePopup airspacePopup = source as AirspacePopup;
            airspacePopup.ParentWindowChanged();
        }

        private bool? m_appliedTopMost;
        private bool m_alreadyLoaded;
        private Window m_parentWindow;

        public AirspacePopup()
        {
            Loaded += OnPopupLoaded;
            Unloaded += OnPopupUnloaded;

            DependencyPropertyDescriptor descriptor = DependencyPropertyDescriptor.FromProperty(PlacementTargetProperty, typeof(AirspacePopup));
            descriptor.AddValueChanged(this, PlacementTargetChanged);
        }

        public bool CoversEntireTarget
        {
            get { return (bool)GetValue(CoversEntireTargetProperty); }
            set { SetValue(CoversEntireTargetProperty, value); }
        }
        public bool IsTopmost
        {
            get { return (bool)GetValue(IsTopmostProperty); }
            set { SetValue(IsTopmostProperty, value); }
        }        
        public bool FollowPlacementTarget
        {
            get { return (bool)GetValue(FollowPlacementTargetProperty); }
            set { SetValue(FollowPlacementTargetProperty, value); }
        }
        public bool AllowOutsideScreenPlacement
        {
            get { return (bool)GetValue(AllowOutsideScreenPlacementProperty); }
            set { SetValue(AllowOutsideScreenPlacementProperty, value); }
        }
        public Window ParentWindow
        {
            get { return (Window)GetValue(ParentWindowProperty); }
            set { SetValue(ParentWindowProperty, value); }
        }

        private void ParentWindowChanged()
        {
            if (ParentWindow != null)
            {
                ParentWindow.LocationChanged += (sender, e2) =>
                {
                    UpdatePopupPosition();
                };
                ParentWindow.SizeChanged += (sender, e2) =>
                {
                    UpdatePopupPosition();
                };
            }
        }
        private void PlacementTargetChanged(object sender, EventArgs e)
        {
            FrameworkElement placementTarget = this.PlacementTarget as FrameworkElement;
            if (placementTarget != null)
            {
                placementTarget.SizeChanged += (sender2, e2) =>
                {
                    UpdatePopupPosition();
                };
            }
        }

        private void UpdatePopupPosition()
        {
            if (this.ParentWindow == null)
            {
                ParentWindow = Window.GetWindow(this);

                if (ParentWindow == null)
                    return;

                ParentWindow.Activated += OnParentWindowActivated;
                ParentWindow.Deactivated += OnParentWindowDeactivated;
            }


            FrameworkElement placementTarget = this.PlacementTarget as FrameworkElement;
            FrameworkElement child = this.Child as FrameworkElement;

            if (PresentationSource.FromVisual(placementTarget) != null &&
                AllowOutsideScreenPlacement == true)
            {
                if (CoversEntireTarget)
                {
                    double leftOffset = CutLeft(placementTarget);
                    double topOffset = CutTop(placementTarget);
                    double rightOffset = CutRight(placementTarget);
                    double bottomOffset = CutBottom(placementTarget);
                    Debug.WriteLine(bottomOffset);
                    this.Width = Math.Max(0, Math.Min(leftOffset, rightOffset) + placementTarget.ActualWidth);
                    this.Height = Math.Max(0, Math.Min(topOffset, bottomOffset) + placementTarget.ActualHeight);

                    if (child != null)
                    {
                        child.Margin = new Thickness(leftOffset, topOffset, rightOffset, bottomOffset);
                    }
                }else
                {
                    //Das Popup-Fenster hängt an der linken oberen Ecke vom placementTarget
                    //Die Position des Fensters wird über HorizontalAlignment,VerticalAlignment und Margin bestimmt
                    //var v1 = this.HorizontalAlignment;
                    //var v2 = this.VerticalAlignment;
                    //var v3 = this.Margin;
                    if (child != null)
                    {
                        double hOffset = 0, vOffset = 0;

                        if (this.HorizontalAlignment == HorizontalAlignment.Left)
                        {
                            hOffset = 0;
                        }

                        if (this.HorizontalAlignment == HorizontalAlignment.Center)
                        {
                            hOffset = placementTarget.ActualWidth / 2 - child.ActualWidth / 2;
                        }

                        if (this.HorizontalAlignment == HorizontalAlignment.Right)
                        {
                            hOffset = placementTarget.ActualWidth - child.ActualWidth;
                        }

                        if (this.VerticalAlignment == VerticalAlignment.Top)
                        {
                            vOffset = 0;
                        }

                        if (this.VerticalAlignment == VerticalAlignment.Center)
                        {
                            vOffset = placementTarget.ActualHeight / 2 - child.ActualHeight / 2;
                        }

                        if (this.VerticalAlignment == VerticalAlignment.Bottom)
                        {
                            vOffset = placementTarget.ActualHeight - child.ActualHeight;
                        }


                        //Linke obere Ecke des Popups laut HorizontalAlignment/VerticalAlignment/Margin
                        hOffset += this.Margin.Left - this.Margin.Right;
                        vOffset += this.Margin.Top - this.Margin.Bottom;

                        
                        //Das Popup-Fenster will nicht außerhalb des Bildschirm verschoben werden
                        //Über diese 4 Marging-Werte wirke ich diese Verschiebung entgegen
                        double marginLeft = 0, marginTop = 0;

                        Point point00 = PointToScreen(new Point(0, 0));
                        if (point00.X < 0 && this.HorizontalAlignment == HorizontalAlignment.Left)
                        {
                            //Sorge dafür, dass das Fenster über den linken Bildschirmrand gehen darf ohne das Popup zu verschieben
                            marginLeft = point00.X;
                        }
                        if (point00.Y < 0 && this.VerticalAlignment == VerticalAlignment.Top)
                        {
                            marginTop = point00.Y;
                        }

                        //Hier war die Idee, dass ich das Popupfenster kleiner mache, wenn es an den rechten/unteren Bildschirmrand kommt
                        //Geht leider nicht so richtig
                        /*this.Width = child.Width;
                        this.Height = child.Height;

                        Point point11 = PointToScreen(new Point(hOffset + child.ActualWidth, vOffset + child.ActualHeight));
                        if (point11.X > SystemParameters.VirtualScreenWidth - 1 && this.HorizontalAlignment == HorizontalAlignment.Right)
                        {
                            double cutX = Math.Abs(point11.X - SystemParameters.VirtualScreenWidth - 1);
                            this.Width = Math.Max(0, child.ActualWidth - cutX);
                            hOffset = placementTarget.ActualWidth - (child.ActualWidth - cutX) + this.Margin.Left - this.Margin.Right;
                        }
                        if (point11.Y > SystemParameters.VirtualScreenHeight && this.VerticalAlignment == VerticalAlignment.Bottom)
                        {
                            //marginBottom = SystemParameters.VirtualScreenHeight - point11.Y;
                        }*/

                        this.HorizontalOffset = hOffset;
                        this.VerticalOffset = vOffset;
                        child.Margin = new Thickness(marginLeft, marginTop, 0, 0); //Hiermit kann das Fenster nach links/oben verschoben werden
                    }
                }


            }
            if (FollowPlacementTarget == true)
            {
                this.HorizontalOffset += 0.01;
                this.HorizontalOffset -= 0.01;
            }
        }

        private Point PointToScreen(Point point)
        {
            FrameworkElement placementTarget = this.PlacementTarget as FrameworkElement;
            Point screenPoint = placementTarget.PointToScreen(point);

            var dpiScale = VisualTreeHelper.GetDpi(ParentWindow);
            return new Point(screenPoint.X / dpiScale.DpiScaleX, screenPoint.Y / dpiScale.DpiScaleY);
        }

        private double CutLeft(FrameworkElement placementTarget)
        {
            Point point = placementTarget.PointToScreen(new Point(0, placementTarget.ActualWidth));
            return Math.Min(0, point.X);
        }
        private double CutTop(FrameworkElement placementTarget)
        {
            Point point = placementTarget.PointToScreen(new Point(placementTarget.ActualHeight, 0));
            return Math.Min(0, point.Y);
        }
        private double CutRight(FrameworkElement placementTarget)
        {
            Point point = placementTarget.PointToScreen(new Point(0, placementTarget.ActualWidth));
            point.X += placementTarget.ActualWidth;
            return Math.Min(0, SystemParameters.VirtualScreenWidth - (Math.Max(SystemParameters.VirtualScreenWidth, point.X)));
        }
        private double CutBottom(FrameworkElement placementTarget)
        {
            Point point = placementTarget.PointToScreen(new Point(placementTarget.ActualHeight, 0));
            point.Y += placementTarget.ActualHeight;
            return Math.Min(0, SystemParameters.VirtualScreenHeight - (Math.Max(SystemParameters.VirtualScreenHeight, point.Y)));
        }

        private void OnPopupLoaded(object sender, RoutedEventArgs e)
        {
            this.IsOpen = true;

            if (m_alreadyLoaded)
                return;

            m_alreadyLoaded = true;

            if (Child != null)
            {
                Child.AddHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(OnChildPreviewMouseLeftButtonDown), true);
            }

            m_parentWindow = Window.GetWindow(this);

            if (m_parentWindow == null)
                return;

            m_parentWindow.Activated += OnParentWindowActivated;
            m_parentWindow.Deactivated += OnParentWindowDeactivated;
        }

        private void OnPopupUnloaded(object sender, RoutedEventArgs e)
        {
            if (m_parentWindow == null)
                return;
            m_parentWindow.Activated -= OnParentWindowActivated;
            m_parentWindow.Deactivated -= OnParentWindowDeactivated;
            m_alreadyLoaded = false;
        }

        private void OnParentWindowActivated(object sender, EventArgs e)
        {
            SetTopmostState(true);            
        }

        private void OnParentWindowDeactivated(object sender, EventArgs e)
        {
            if (IsTopmost == false)
            {
                SetTopmostState(IsTopmost);
            }
        }

        private void OnChildPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetTopmostState(true);
            if (!m_parentWindow.IsActive && IsTopmost == false)
            {
                m_parentWindow.Activate();
            }
        }

        protected override void OnOpened(EventArgs e)
        {
            SetTopmostState(IsTopmost);
            base.OnOpened(e);
            UpdatePopupPosition();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }

        private void SetTopmostState(bool isTop)
        {
            // Don’t apply state if it’s the same as incoming state
            if (m_appliedTopMost.HasValue && m_appliedTopMost == isTop)
            {
                return;
            }

            if (Child == null)
                return;

            var hwndSource = (PresentationSource.FromVisual(Child)) as HwndSource;
            //var hwndSource = (PresentationSource.FromVisual(this)) as HwndSource;
            //var helper = new WindowInteropHelper(ParentWindow);
            //var hwndSource1 = HwndSource.FromHwnd(helper.EnsureHandle());

            if (hwndSource == null)
                return;
            var hwnd = hwndSource.Handle;

            RECT rect;

            if (!GetWindowRect(hwnd, out rect))
                return;

            Debug.WriteLine("setting z-order " + isTop);

            if (isTop)
            {
                SetWindowPos(hwnd, HWND_TOPMOST, rect.Left, rect.Top, (int)Width, (int)Height, TOPMOST_FLAGS);
            }
            else
            {
                // Z-Order would only get refreshed/reflected if clicking the
                // the titlebar (as opposed to other parts of the external
                // window) unless I first set the popup to HWND_BOTTOM
                // then HWND_TOP before HWND_NOTOPMOST
                SetWindowPos(hwnd, HWND_BOTTOM, rect.Left, rect.Top, (int)Width, (int)Height, TOPMOST_FLAGS);
                SetWindowPos(hwnd, HWND_TOP, rect.Left, rect.Top, (int)Width, (int)Height, TOPMOST_FLAGS);
                SetWindowPos(hwnd, HWND_NOTOPMOST, rect.Left, rect.Top, (int)Width, (int)Height, TOPMOST_FLAGS);
            }

            m_appliedTopMost = isTop;
        }

        #region P/Invoke imports & definitions
#pragma warning disable 1591 //Xml-doc
#pragma warning disable 169 //Never used-warning
        // ReSharper disable InconsistentNaming
        // Imports etc. with their naming rules

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT

        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X,
        int Y, int cx, int cy, uint uFlags);

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        static readonly IntPtr HWND_TOP = new IntPtr(0);
        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        private const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_NOZORDER = 0x0004;
        const UInt32 SWP_NOREDRAW = 0x0008;
        const UInt32 SWP_NOACTIVATE = 0x0010;

        const UInt32 SWP_FRAMECHANGED = 0x0020; /* The frame changed: send WM_NCCALCSIZE */
        const UInt32 SWP_SHOWWINDOW = 0x0040;
        const UInt32 SWP_HIDEWINDOW = 0x0080;
        const UInt32 SWP_NOCOPYBITS = 0x0100;
        const UInt32 SWP_NOOWNERZORDER = 0x0200; /* Don’t do owner Z ordering */
        const UInt32 SWP_NOSENDCHANGING = 0x0400; /* Don’t send WM_WINDOWPOSCHANGING */

        const UInt32 TOPMOST_FLAGS =
            SWP_NOACTIVATE | SWP_NOOWNERZORDER | SWP_NOSIZE | SWP_NOMOVE | SWP_NOREDRAW | SWP_NOSENDCHANGING;

        // ReSharper restore InconsistentNaming
#pragma warning restore 1591
#pragma warning restore 169
        #endregion
    }
}
