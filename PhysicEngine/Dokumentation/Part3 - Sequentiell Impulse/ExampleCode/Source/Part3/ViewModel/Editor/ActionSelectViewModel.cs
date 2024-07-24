using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;
using System.Windows;
using System.Windows.Input;

namespace Part3.ViewModel.Editor
{
    //Enthält die Logik, dass man Buttons selektieren kann
    public class ActionSelectViewModel : ReactiveObject
    {
        public enum Commands { Nothing, AddRectangle, AddCircle, MoveRotate, MoveResize, CloneShape, EditProperties, Delete }

        public ReactiveCommand<Unit, Unit> AddRectangleClick { get; private set; }
        public ReactiveCommand<Unit, Unit> AddCircleClick { get; private set; }
        public ReactiveCommand<Unit, Unit> MoveRotateClick { get; private set; }
        public ReactiveCommand<Unit, Unit> MoveResizeClick { get; private set; }
        public ReactiveCommand<Unit, Unit> CloneShapeClick { get; private set; }
        public ReactiveCommand<Unit, Unit> EditPropertiesClick { get; private set; }
        public ReactiveCommand<Unit, Unit> DeleteClick { get; private set; }

        [Reactive] public Thickness AddRectangleBorderThickness { get; set; }
        [Reactive] public Thickness AddCircleBorderThickness { get; set; }
        [Reactive] public Thickness MoveRotateBorderThickness { get; set; }
        [Reactive] public Thickness MoveResizeBorderThickness { get; set; }
        [Reactive] public Thickness CloneShapeBorderThickness { get; set; }
        [Reactive] public Thickness EditPropertiesBorderThickness { get; set; }
        [Reactive] public Thickness DeleteBorderThickness { get; set; }

        [Reactive] public Thickness AddRectangleMargin { get; set; }
        [Reactive] public Thickness AddCircleMargin { get; set; }
        [Reactive] public Thickness MoveRotateMargin { get; set; }
        [Reactive] public Thickness MoveResizeMargin { get; set; }
        [Reactive] public Thickness CloneShapeMargin { get; set; }
        [Reactive] public Thickness EditPropertiesMargin { get; set; }
        [Reactive] public Thickness DeleteMargin { get; set; }

        public event Action<Commands> SelectedCommandChanged;

        private void RaiseSelectedCommandChanged(Commands command)
        {
            this.SelectedCommandChanged?.Invoke(command);
        }

        public ActionSelectViewModel()
        {
            this.AddRectangleClick = ReactiveCommand.Create(() =>
            {
                this.SelectedCommand = Commands.AddRectangle;
            });
            this.AddCircleClick = ReactiveCommand.Create(() =>
            {
                this.SelectedCommand = Commands.AddCircle;
            });
            this.MoveRotateClick = ReactiveCommand.Create(() =>
            {
                this.SelectedCommand = Commands.MoveRotate;
            });
            this.MoveResizeClick = ReactiveCommand.Create(() =>
            {
                this.SelectedCommand = Commands.MoveResize;
            });
            this.CloneShapeClick = ReactiveCommand.Create(() =>
            {
                this.SelectedCommand = Commands.CloneShape;
            });
            this.EditPropertiesClick = ReactiveCommand.Create(() =>
            {
                this.SelectedCommand = Commands.EditProperties;
            });
            this.DeleteClick = ReactiveCommand.Create(() =>
            {
                this.SelectedCommand = Commands.Delete;
            });

            SetAllButtonBorderThickness(2);
            SetAllButtonMargin(3);
        }

        private Commands selectedCommand = Commands.Nothing;
        public Commands SelectedCommand
        {
            get => this.selectedCommand;
            set
            {
                this.selectedCommand = value;
                UpdateBorders(this.selectedCommand);
                RaiseSelectedCommandChanged(this.selectedCommand);
            }
        }

        private void UpdateBorders(Commands value)
        {
            SetAllButtonBorderThickness(2);
            SetAllButtonMargin(3);
            double selectedThickness = 5;
            double selectedMargin = 0;
            switch (value)
            {
                case Commands.AddRectangle:
                    this.AddRectangleBorderThickness = new Thickness(selectedThickness);
                    this.AddRectangleMargin = new Thickness(selectedMargin);
                    break;
                case Commands.AddCircle:
                    this.AddCircleBorderThickness = new Thickness(selectedThickness);
                    this.AddCircleMargin = new Thickness(selectedMargin);
                    break;
                case Commands.MoveRotate:
                    this.MoveRotateBorderThickness = new Thickness(selectedThickness);
                    this.MoveRotateMargin = new Thickness(selectedMargin);
                    break;
                case Commands.MoveResize:
                    this.MoveResizeBorderThickness = new Thickness(selectedThickness);
                    this.MoveResizeMargin = new Thickness(selectedMargin);
                    break;
                case Commands.CloneShape:
                    this.CloneShapeBorderThickness = new Thickness(selectedThickness);
                    this.CloneShapeMargin = new Thickness(selectedMargin);
                    break;
                case Commands.EditProperties:
                    this.EditPropertiesBorderThickness = new Thickness(selectedThickness);
                    this.EditPropertiesMargin = new Thickness(selectedMargin);
                    break;
                case Commands.Delete:
                    this.DeleteBorderThickness = new Thickness(selectedThickness);
                    this.DeleteMargin = new Thickness(selectedMargin, 17, selectedMargin, selectedMargin);
                    break;
            }
        }

        private void SetAllButtonBorderThickness(double thickness)
        {
            this.AddRectangleBorderThickness = new Thickness(thickness);
            this.AddCircleBorderThickness = new Thickness(thickness);
            this.MoveRotateBorderThickness = new Thickness(thickness);
            this.MoveResizeBorderThickness = new Thickness(thickness);
            this.CloneShapeBorderThickness = new Thickness(thickness);
            this.EditPropertiesBorderThickness = new Thickness(thickness);
            this.DeleteBorderThickness = new Thickness(thickness);
        }

        private void SetAllButtonMargin(double thickness)
        {
            this.AddRectangleMargin = new Thickness(thickness);
            this.AddCircleMargin = new Thickness(thickness);
            this.MoveRotateMargin = new Thickness(thickness);
            this.MoveResizeMargin = new Thickness(thickness);
            this.CloneShapeMargin = new Thickness(thickness);
            this.EditPropertiesMargin = new Thickness(thickness);
            this.DeleteMargin = new Thickness(thickness, 20, thickness, thickness);
        }

        public void HandleKeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.SelectedCommand = Commands.Nothing;
        }
    }
}
