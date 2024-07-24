using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reactive;
using System.Windows.Input;
using System.Windows;

namespace EditorControl.ViewModel
{
    //Enthält die Logik, dass man Buttons selektieren kann
    public class ActionSelectViewModel : ReactiveObject
    {
        public enum Commands { Nothing, AddRectangle, AddCircle, MoveRotate, MoveResize, CloneShape, AddDistanceJoint, AddRevoluteJoint, AddPrismaticJoint, AddWeldJoint, AddWheelJoint, LimitJoint, EditProperties, Delete }

        public ReactiveCommand<Unit, Unit> AddRectangleClick { get; private set; }
        public ReactiveCommand<Unit, Unit> AddCircleClick { get; private set; }
        public ReactiveCommand<Unit, Unit> MoveRotateClick { get; private set; }
        public ReactiveCommand<Unit, Unit> MoveResizeClick { get; private set; }
        public ReactiveCommand<Unit, Unit> CloneShapeClick { get; private set; }
        public ReactiveCommand<Unit, Unit> AddDistanceJointClick { get; private set; }
        public ReactiveCommand<Unit, Unit> AddRevoluteJointClick { get; private set; }
        public ReactiveCommand<Unit, Unit> AddPrismaticJointClick { get; private set; }
        public ReactiveCommand<Unit, Unit> AddWeldJointClick { get; private set; }
        public ReactiveCommand<Unit, Unit> AddWheelJointClick { get; private set; }
        public ReactiveCommand<Unit, Unit> LimitJointClick { get; private set; }
        public ReactiveCommand<Unit, Unit> EditPropertiesClick { get; private set; }
        public ReactiveCommand<Unit, Unit> DeleteClick { get; private set; }

        [Reactive] public Thickness AddRectangleBorderThickness { get; set; }
        [Reactive] public Thickness AddCircleBorderThickness { get; set; }
        [Reactive] public Thickness MoveRotateBorderThickness { get; set; }
        [Reactive] public Thickness MoveResizeBorderThickness { get; set; }
        [Reactive] public Thickness CloneShapeBorderThickness { get; set; }
        [Reactive] public Thickness AddDistanceJointThickness { get; set; }
        [Reactive] public Thickness AddRevoluteJointThickness { get; set; }
        [Reactive] public Thickness AddPrismaticJointThickness { get; set; }
        [Reactive] public Thickness AddWeldJointThickness { get; set; }
        [Reactive] public Thickness AddWheelJointThickness { get; set; }
        [Reactive] public Thickness LimitJointBorderThickness { get; set; }
        [Reactive] public Thickness EditPropertiesBorderThickness { get; set; }
        [Reactive] public Thickness DeleteBorderThickness { get; set; }

        [Reactive] public Thickness AddRectangleMargin { get; set; }
        [Reactive] public Thickness AddCircleMargin { get; set; }
        [Reactive] public Thickness MoveRotateMargin { get; set; }
        [Reactive] public Thickness MoveResizeMargin { get; set; }
        [Reactive] public Thickness CloneShapeMargin { get; set; }
        [Reactive] public Thickness AddDistanceJointMargin { get; set; }
        [Reactive] public Thickness AddRevoluteJointMargin { get; set; }
        [Reactive] public Thickness AddPrismaticJointMargin { get; set; }
        [Reactive] public Thickness AddWeldJointMargin { get; set; }
        [Reactive] public Thickness AddWheelJointMargin { get; set; }
        [Reactive] public Thickness LimitJointMargin { get; set; }
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
            this.AddDistanceJointClick = ReactiveCommand.Create(() =>
            {
                this.SelectedCommand = Commands.AddDistanceJoint;
            });
            this.AddRevoluteJointClick = ReactiveCommand.Create(() =>
            {
                this.SelectedCommand = Commands.AddRevoluteJoint;
            });
            this.AddPrismaticJointClick = ReactiveCommand.Create(() =>
            {
                this.SelectedCommand = Commands.AddPrismaticJoint;
            });
            this.AddWeldJointClick = ReactiveCommand.Create(() =>
            {
                this.SelectedCommand = Commands.AddWeldJoint;
            });
            this.AddWheelJointClick = ReactiveCommand.Create(() =>
            {
                this.SelectedCommand = Commands.AddWheelJoint;
            });
            this.LimitJointClick = ReactiveCommand.Create(() =>
            {
                this.SelectedCommand = Commands.LimitJoint;
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
                case Commands.AddDistanceJoint:
                    this.AddDistanceJointThickness = new Thickness(selectedThickness);
                    this.AddDistanceJointMargin = new Thickness(selectedMargin);
                    break;
                case Commands.AddRevoluteJoint:
                    this.AddRevoluteJointThickness = new Thickness(selectedThickness);
                    this.AddRevoluteJointMargin = new Thickness(selectedMargin);
                    break;
                case Commands.AddPrismaticJoint:
                    this.AddPrismaticJointThickness = new Thickness(selectedThickness);
                    this.AddPrismaticJointMargin = new Thickness(selectedMargin);
                    break;
                case Commands.AddWeldJoint:
                    this.AddWeldJointThickness = new Thickness(selectedThickness);
                    this.AddWeldJointMargin = new Thickness(selectedMargin);
                    break;
                case Commands.AddWheelJoint:
                    this.AddWheelJointThickness = new Thickness(selectedThickness);
                    this.AddWheelJointMargin = new Thickness(selectedMargin);
                    break;
                case Commands.LimitJoint:
                    this.LimitJointBorderThickness = new Thickness(selectedThickness);
                    this.LimitJointMargin = new Thickness(selectedMargin);
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
            this.AddDistanceJointThickness = new Thickness(thickness);
            this.AddRevoluteJointThickness = new Thickness(thickness);
            this.AddPrismaticJointThickness = new Thickness(thickness);
            this.AddWeldJointThickness = new Thickness(thickness);
            this.AddWheelJointThickness = new Thickness(thickness);
            this.LimitJointBorderThickness = new Thickness(thickness);
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
            this.AddDistanceJointMargin = new Thickness(thickness);
            this.AddRevoluteJointMargin = new Thickness(thickness);
            this.AddPrismaticJointMargin = new Thickness(thickness);
            this.AddWeldJointMargin = new Thickness(thickness);
            this.AddWheelJointMargin = new Thickness(thickness);
            this.LimitJointMargin = new Thickness(thickness);
            this.EditPropertiesMargin = new Thickness(thickness);
            this.DeleteMargin = new Thickness(thickness, 20, thickness, thickness);
        }

        public void HandleKeyPress(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.SelectedCommand = Commands.Nothing;
        }
    }
}
