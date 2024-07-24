using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Part1.ViewModel.Editor
{
    public enum MassType { Mass, Density, Infinity }

    public class ShapePropertyViewModel : ReactiveObject
    {
        [Reactive] public float VelocityX { get; set; }
        [Reactive] public float VelocityY { get; set; }
        [Reactive] public float AngularVelocity { get; set; } = 0;

        [Reactive] public MassType MassType1 { get; set; }
        [Reactive] public float Mass { get; set; } = 1;
        [Reactive] public float Density { get; set; } = 1;

        [Reactive] public float Friction { get; set; } = 1;
        [Reactive] public float Restituion { get; set; } = 1;
    }

    
}
