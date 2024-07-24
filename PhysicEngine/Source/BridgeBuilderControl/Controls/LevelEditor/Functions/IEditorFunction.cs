namespace BridgeBuilderControl.Controls.LevelEditor.Functions
{
    internal interface IEditorFunction
    {
        FunctionType Type { get; }
        IEditorFunction Init(EditorState state);
        void HandleTimerTick(float dt);
        void HandleMouseClick(System.Windows.Forms.MouseEventArgs e);
        void HandleMouseMove(System.Windows.Forms.MouseEventArgs e);
    }

    enum FunctionType { Default, DefineGround, DefineAnchorPoints}
}
