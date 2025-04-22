using BridgeBuilderControl.Controls.Helper;

namespace BridgeBuilderControl.Controls.LevelEditor.Functions
{
    internal class DefaultFunction : IEditorFunction
    {
        private EditorState state;
        public FunctionType Type { get => FunctionType.Default; } 
        public IEditorFunction Init(EditorState state)
        {
            this.state = state;
            return this;
        }
        public void HandleTimerTick(float dt)
        {
            var panel = this.state.Panel;
            DrawingHelper.DrawBackgroundOrClipboardImage(panel, state.XCount, state.YCount, state.WaterHeight, state.GroundHeight);
            DrawingHelper.DrawGround(panel, state.GroundPolygon, state.GroundHeight, state.XCount, state.YCount);
            DrawingHelper.DrawAnchorPoints(panel, state.AnchorPoints);
            panel.FlipBuffer();
        }
        public void HandleMouseClick(System.Windows.Forms.MouseEventArgs e) { }
        public void HandleMouseMove(System.Windows.Forms.MouseEventArgs e) { }
    }
}
