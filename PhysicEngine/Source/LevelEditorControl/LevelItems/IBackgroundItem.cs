using GraphicPanels;
using LevelEditorGlobal;

namespace LevelEditorControl.LevelItems
{
    //Wird erst zur Simulationszeit erstellt
    //Möglichkeit 1: Item ist fest (Beispiel: Graskante)
    //Möglichkeit 2: Item ist weiter weg im Hintergrund und bewegt sich langsam mit der Kamera mit
    internal interface IBackgroundItem
    {
        void Draw(GraphicPanel2D panel);
        BackgroundItemSimulatorExportData GetSimulatorExportData();
    }



    internal interface IBackgroundItemProvider
    {
        IBackgroundItem[] GetBackgroundItems();
    }
}
