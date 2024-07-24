using System;
using System.Windows.Input;
using System.Windows;

namespace WpfControls.Converter
{
    //So kann man ein MouseDown-Event in ein ICommand umwandlen (Auch wenn das Item in ein DataTemplate von einer ListBox verwendet wird)
    //https://stackoverflow.com/questions/20288715/wpf-handle-mousedown-events-from-within-a-datatemplate
    //<Image Source="{Binding Image}" Stretch="Fill" Width="30" Height="30" local:MouseDownEventToCommandConverter.MouseLeftClick="{Binding MouseDownHandler}">
    //public ReactiveCommand<System.Windows.Controls.Image, Unit> MouseDownHandler { get; private set; }
    //this.MouseDownHandler = ReactiveCommand.Create<System.Windows.Controls.Image, Unit>((image) =>
    //{
    //    actions.MouseDownAction(this.Item);
    //    return Unit.Default;
    //});
    public class MouseDownEventToCommandConverter
    {
        public static readonly DependencyProperty MouseLeftClickProperty =
            DependencyProperty.RegisterAttached("MouseLeftClick", typeof(ICommand), typeof(MouseDownEventToCommandConverter),
            new FrameworkPropertyMetadata(CallBack));

        public static void SetMouseLeftClick(DependencyObject sender, ICommand value)
        {
            sender.SetValue(MouseLeftClickProperty, value);
        }

        public static ICommand GetMouseLeftClick(DependencyObject sender)
        {
            return sender.GetValue(MouseLeftClickProperty) as ICommand;
        }

        public static readonly DependencyProperty MouseEventParameterProperty =
            DependencyProperty.RegisterAttached(
            "MouseEventParameter",
                typeof(object),
                typeof(MouseDownEventToCommandConverter),
                new FrameworkPropertyMetadata((object)null, null));


        public static object GetMouseEventParameter(DependencyObject d)
        {
            return d.GetValue(MouseEventParameterProperty);
        }


        public static void SetMouseEventParameter(DependencyObject d, object value)
        {
            d.SetValue(MouseEventParameterProperty, value);
        }

        private static void CallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender != null)
            {
                UIElement element = sender as UIElement;
                if (element != null)
                {
                    if (e.OldValue != null)
                    {
                        element.RemoveHandler(UIElement.MouseDownEvent, new MouseButtonEventHandler(Handler));
                    }
                    if (e.NewValue != null)
                    {
                        element.AddHandler(UIElement.MouseDownEvent, new MouseButtonEventHandler(Handler), true);
                    }
                }
            }
        }
        private static void Handler(object sender, EventArgs e)
        {
            UIElement element = sender as UIElement;
            if (sender != null)
            {
                ICommand cmd = element.GetValue(MouseLeftClickProperty) as ICommand;
                if (cmd != null)
                {
                    RoutedCommand routedCmd = cmd as RoutedCommand;
                    object paramenter = element.GetValue(MouseEventParameterProperty);
                    if (paramenter == null)
                    {
                        paramenter = element;
                    }
                    if (routedCmd != null)
                    {
                        if (routedCmd.CanExecute(paramenter, element))
                        {
                            routedCmd.Execute(paramenter, element);
                        }
                    }
                    else
                    {
                        if (cmd.CanExecute(paramenter))
                        {
                            //cmd.Execute(paramenter); 
                            cmd.Execute(e); //ich muss hier e anstatt paramenter geben, wenn ich die System.Windows.Input.MouseButtonEventArgs will
                        }
                    }
                }
            }
        }
    }
}
