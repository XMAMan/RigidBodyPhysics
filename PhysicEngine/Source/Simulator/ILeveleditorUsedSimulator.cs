using GraphicPanels;
using RigidBodyPhysics.ExportData;
using Simulator.ForceTracking;

namespace Simulator
{
    //All diese Funktionen nutzt der Leveleditor vom Simulator
    public interface ILeveleditorUsedSimulator
    {
        void Draw(GraphicPanel2D panel);
        void DrawPhysicItemBorders(GraphicPanel2D panel, Pen borderPen);
        void DrawSmallWindow(GraphicPanel2D panel);
        void PanelSizeChangedHandler(int width, int height);
        void MoveOneStep(float dt);
        void HandleKeyDown(System.Windows.Input.Key key);
        void HandleKeyUp(System.Windows.Input.Key key);
        void HandleMouseDown(MouseEventArgs e);
        void HandleMouseUp(MouseEventArgs e);
        void HandleMouseMove(MouseEventArgs e);
        void HandleMouseWheel(MouseEventArgs e);
        ForceTracker CreateForceTracker();
        PhysisSceneIndexDrawer CreatePhysisSceneIndexDrawer(PhysisSceneIndexDrawerSettings forceTrackerDrawingSettings);
        PhysicSceneExportData GetPhysicSceneExportData();
    }
}
