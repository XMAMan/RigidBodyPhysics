using LevelEditorControl.EditorFunctions;
using LevelEditorControl.LevelItems;
using LevelEditorGlobal;
using LevelToSimulatorConverter;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Media;

namespace LevelEditorControl.Controls.SelectedItemControl
{
    internal class SelectedItemViewModel : ReactiveObject
    {
        private EditorState editorData;
        public ReactiveCommand<Unit, Unit> RotateResizeButtonClick { get; private set; }
        public ReactiveCommand<Unit, Unit> KeyboardmappingClick { get; private set; }
        public ReactiveCommand<Unit, Unit> IsCameraTrackedClick { get; private set; }
        public ReactiveCommand<Unit, Unit> GroupItemsClick { get; private set; }
        [Reactive] public SolidColorBrush RotateResizeButtonColor { get; set; } = System.Windows.Media.Brushes.LightGray;
        [Reactive] public SolidColorBrush KeyboardmappingButtonColor { get; set; } = System.Windows.Media.Brushes.LightGray;
        [Reactive] public SolidColorBrush IsCameraTrackedButtonColor { get; set; } = System.Windows.Media.Brushes.LightGray;

        [Reactive] public bool OneItemIsSelected { get; private set; }
        [Reactive] public bool MultipleItemsAreSelected { get; private set; }


        public SelectedItemViewModel() { }

        internal SelectedItemViewModel(EditorState editorData)
        {
            this.editorData = editorData;

            this.editorData.SelectedItems.Connect().Subscribe(x =>
            {
                this.OneItemIsSelected = this.editorData.SelectedItems.Count == 1;
                this.MultipleItemsAreSelected = this.editorData.SelectedItems.Count > 1;
            });

            this.RotateResizeButtonClick = ReactiveCommand.Create(() =>
            {
            },
            this.editorData.SelectedItems.Connect().Select(x => this.editorData.SelectedItems.Count == 1) //CanExecute: Zeige den Button nur, wenn genau ein Item selektiert wurde
            );

            this.KeyboardmappingClick = ReactiveCommand.Create(() =>
            {
            },
            this.editorData.SelectedItems.Connect().Select(x => this.editorData.SelectedItems.Count == 1 && this.editorData.SelectedItems.Items.First() is IKeyboardControlledLevelItem) //CanExecute: Zeige den Button nur, wenn ein PhysicItem selektiert wurde
            );

            this.IsCameraTrackedClick = ReactiveCommand.Create(() =>
            {
                if (this.editorData.CameraTrackedItem == this.editorData.SelectedItems.Items.First())
                {
                    this.editorData.CameraTrackedItem = null;
                }
                else
                {
                    this.editorData.CameraTrackedItem = this.editorData.SelectedItems.Items.First();
                }
                UpdateCameraTrackerButton();

            },
            this.editorData.SelectedItems.Connect().Select(x => this.editorData.SelectedItems.Count == 1 && this.editorData.SelectedItems.Items.First() is IMergeablePhysicScene) //CanExecute: Zeige den Button nur, wenn genau ein Item selektiert wurde und es ein PhyisItem ist (Nur diese können sich bewegen und Cameratracking macht Sinn)
            );

            this.GroupItemsClick = ReactiveCommand.Create(() =>
            {
            },
            this.editorData.SelectedItems.Connect().Select(x => this.editorData.SelectedItems.Count > 1 && this.editorData.SelectedItems.Items.All(y => y is IPrototypLevelItem)) //CanExecute: Button darf nur gedrückt werden wenn mehrere Objekte selektiert wurden und sie alle ein IPrototypLevelItem sind
            );
        }


        public void UpdateButtonColors(FunctionType currentState)
        {
            this.RotateResizeButtonColor = currentState == FunctionType.RotateResize ? System.Windows.Media.Brushes.Red : System.Windows.Media.Brushes.LightGray;
            this.KeyboardmappingButtonColor = currentState == FunctionType.KeyboardMapping ? System.Windows.Media.Brushes.Red : System.Windows.Media.Brushes.LightGray;
            UpdateCameraTrackerButton();
        }

        private void UpdateCameraTrackerButton()
        {
            this.IsCameraTrackedButtonColor = this.editorData.CameraTrackedItem == this.editorData.SelectedItems.Items.FirstOrDefault() ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.LightGray;
        }
    }
}
