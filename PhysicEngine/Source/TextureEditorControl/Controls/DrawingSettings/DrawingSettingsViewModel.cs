using ReactiveUI.Fody.Helpers;
using ReactiveUI;

namespace TextureEditorControl.Controls.DrawingSettings
{
    public class DrawingSettingsViewModel : ReactiveObject
    {
        [Reactive] public bool ShowPhysikModel { get; set; } = true;
        [Reactive] public bool ShowTextureBorder { get; set; } = true;
        [Reactive] public bool ShowTexture { get; set; } = true;
        [Reactive] public bool UseDepthTesting { get; set; } = true;
    }
}
