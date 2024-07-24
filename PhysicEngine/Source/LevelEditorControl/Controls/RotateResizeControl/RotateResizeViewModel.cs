using GraphicMinimal;
using LevelEditorControl.LevelItems;
using LevelEditorGlobal;
using ReactiveUI;
using System.Reactive;

namespace LevelEditorControl.Controls.RotateResizeControl
{
    internal class RotateResizeViewModel : ReactiveObject
    {
        public float Angle
        {
            get => this.rec.AngleInDegree;
            set
            {
                rec.AngleInDegree = value;
                this.RaisePropertyChanged(nameof(Angle));
            }
        }

        public float Size
        {
            get => this.rec.SizeFactor;
            set
            {
                rec.SizeFactor = value;
                this.RaisePropertyChanged(nameof(Size));
            }
        }

        public ReactiveCommand<Unit, Unit> TransferToPrototypClick { get; private set; }

        private RotatedRectangle rec;

        public RotateResizeViewModel(RotatedRectangle rec, IPrototypItem associatedPrototyp)
        {
            this.rec = rec;

            this.TransferToPrototypClick = ReactiveCommand.Create(() =>
            {
                if (associatedPrototyp != null)
                {
                    associatedPrototyp.InitialRecValues.SizeFactor = rec.SizeFactor;
                    associatedPrototyp.InitialRecValues.AngleInDegree = rec.AngleInDegree;
                    associatedPrototyp.InitialRecValues.LocalPivot = new Vector2D(rec.LocalPivot);
                }
            });
        }
    }
}
