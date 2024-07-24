using GraphicPanels;
using System.Drawing;
using TextureEditorControl.Controls.DrawingSettings;
using WpfControls.Controls.CameraSetting;

namespace TextureEditorControl.Controls.Editor.Model.Shape
{
    internal class Drawer
    {
        private IShape[] shapes;

        public Drawer(IShape[] shapes)
        {
            this.shapes = shapes;
        }

        public void Draw(GraphicPanel2D panel, Camera2D camera, DrawingSettingsViewModel settings)
        {
            panel.ClearScreen(Color.White); //Achtung: Clear-Screen ruft intern DisableDepthTesting auf. Deswegen muss das zuerst kommen

            if (settings.UseDepthTesting)
                panel.EnableDepthTesting();
            else
                panel.DisableDepthTesting();


            foreach (var shape in this.shapes)
            {
                panel.ZValue2D = shape.Propertys.ZValue;
                shape.Draw(panel, camera, settings);

            }
            panel.FlipBuffer();
        }
    }
}
