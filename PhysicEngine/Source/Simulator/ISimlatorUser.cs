using LevelEditorGlobal;
using WpfControls.Controls.CameraSetting;

namespace Simulator
{
    //Das ist die Build-Methode zur Erstellung von ein ILevelediturUsedSimulator-Objekt
    public delegate ILeveleditorUsedSimulator CreateSimulatorFunction(SimulatorInputData data, Size panelSize, Camera2D camera, float timerIntercallInMilliseconds);

    //Der Leveleditor ist ein ILevelediturUsedSimulator-Verwender, dem vom außen eine Simulator-Build-Methode reingeben werden kann
    public interface ISimlatorUser
    {
        void SetSimulatorBuildMethod(CreateSimulatorFunction createSimulatorFunction);
    }
}
