using GraphicPanelWpf;
using System;

namespace LevelEditorControl.EditorFunctions
{
    public enum FunctionType { Nothing, AddItem, MoveSelect, Simulator, AddPolygon, EditPolygon, AddLawnEdge, EditLawnEdge, KeyboardMapping, RotateResize, EditGrid, DefineCollisionMatrix, EditCameraTracker, GroupItems, DefineTagColor, DefineAnchorPoint }
    internal interface IEditorFunction : ITimerHandler, IGraphicPanelHandler, IDisposable
    {
        FunctionType Type { get; }
        IEditorFunction Init(EditorState state);
        bool HasPropertyControl { get; } //Soll im MainControl innerhalb eines ContentControls ein UserControl gezeigt werden?
        System.Windows.Controls.UserControl GetPropertyControl(); //Hier können noch zusätliche Eigenschaften angeigt werden
    }
}
