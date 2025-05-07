using LevelEditorExports.Simulator;
using PhysicGlobal;

namespace LevelEditorControl.LevelItems
{
    //Wird erst zur Simulationszeit erstellt
    //Möglichkeit 1: Item ist fest (Beispiel: Graskante)
    //Möglichkeit 2: Item ist weiter weg im Hintergrund und bewegt sich langsam mit der Kamera mit
    internal interface IBackgroundItem
    {
        void Draw(IDrawingPanel panel);
        BackgroundItemSimulatorExportData GetSimulatorExportData();
    }



    internal interface IBackgroundItemProvider
    {
        IBackgroundItem[] GetBackgroundItems();
    }
}
