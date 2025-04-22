using DynamicData;
using LevelEditorControl.EditorFunctions;
using LevelEditorControl.LevelItems.Polygon;
using LevelEditorControl.LevelItems;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System;
using System.Windows.Forms;
using System.Linq;
using LevelEditorGlobal;
using WpfControls.Controls.CameraSetting;

namespace LevelEditorControl.Controls.PolygonControl
{
    internal class PolygonControlViewModel : ReactiveObject
    {
        public ReactiveCommand<Unit, Unit> AddPolygonClick { get; private set; }
        [Reactive] public System.Windows.Media.SolidColorBrush AddPolygonButtonColor { get; set; } = System.Windows.Media.Brushes.LightGray;
        [Reactive] public System.Windows.Media.SolidColorBrush AddLawnEdgeButtonColor { get; set; } = System.Windows.Media.Brushes.LightGray;
        public ReactiveCommand<Unit, Unit> BackgroundImageClick { get; private set; }
        public ReactiveCommand<Unit, Unit> ForegroundImageClick { get; private set; }
        public ReactiveCommand<Unit, Unit> AddLawnEdgeClick { get; private set; }
        public ReactiveCommand<Unit, Unit> StretchWithoutAspectRatio { get; private set; }
        public ReactiveCommand<Unit, Unit> StretchWithAspectRatio { get; private set; }
        public ReactiveCommand<Unit, Unit> ShowBackgroundNoStretch { get; private set; }

        private PolygonImages images;

        [Reactive] public ImageMode BackgroundImageMode { get; set; }

        internal PolygonControlViewModel(Action addPolygonClick, Action addLawnEdgeClick, PolygonImages images, Camera2D camera, SourceList<ILevelItem> selectedItems)
        {
            this.images = images;

            this.BackgroundImageMode = images.BackgroundImageMode;

            this.AddPolygonClick = ReactiveCommand.Create(() =>
            {
                addPolygonClick();
            });

            this.BackgroundImageClick = ReactiveCommand.Create(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "png files (*.png)|*.png|jpeg files (*.jpg)|*.jpg|bmp files (*.bmp)|*.bmp|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    images.BackgroundImage = openFileDialog.FileName;
                    camera.UpdateBackgroundImage(images.Background.Size); //Sage der Kamera, wie breit das Hintergrundbild ist damit es bei der Anzeigeoption InitialPosition=ToBackgroundImage genau das Backgroundbild zeigt
                }
            });

            this.ForegroundImageClick = ReactiveCommand.Create(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "png files (*.png)|*.png|jpeg files (*.jpg)|*.jpg|bmp files (*.bmp)|*.bmp|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    images.ForegroundImage = openFileDialog.FileName;
                }
            });

            this.AddLawnEdgeClick = ReactiveCommand.Create(() =>
            {
                addLawnEdgeClick();
            },
             selectedItems.Connect().Select(x => selectedItems.Count == 1 && selectedItems.Items.First() is ILevelItemPolygon)
            );

            this.StretchWithoutAspectRatio = ReactiveCommand.Create(() =>
            {
                this.BackgroundImageMode = images.BackgroundImageMode = LevelEditorGlobal.ImageMode.StretchWithoutAspectRatio;
            });

            this.StretchWithAspectRatio = ReactiveCommand.Create(() =>
            {
                this.BackgroundImageMode = images.BackgroundImageMode = LevelEditorGlobal.ImageMode.StretchWithAspectRatio;
            });

            this.ShowBackgroundNoStretch = ReactiveCommand.Create(() =>
            {
                this.BackgroundImageMode = images.BackgroundImageMode = LevelEditorGlobal.ImageMode.NoStretch;
            });

            
        }

        public void UpdateButtonColors(FunctionType currentState)
        {
            this.AddPolygonButtonColor = currentState == FunctionType.AddPolygon ? System.Windows.Media.Brushes.Red : System.Windows.Media.Brushes.LightGray;
            this.AddLawnEdgeButtonColor = currentState == FunctionType.AddLawnEdge ? System.Windows.Media.Brushes.Red : System.Windows.Media.Brushes.LightGray;
        }
    }
}
