using LevelEditorControl.Controls.CameraTrackerControl;
using System;
using System.Drawing;

namespace LevelEditorControl.EditorFunctions
{
    internal class EditCameraTrackerFunction : DummyFunction, IEditorFunction
    {
        private EditorState state;
        private CameraTrackerViewModel vm;

        public override FunctionType Type => FunctionType.EditCameraTracker;

        public override IEditorFunction Init(EditorState state)
        {
            this.state = state;

            this.HasPropertyControl = true;
            return this;
        }

        public override System.Windows.Controls.UserControl GetPropertyControl()
        {
            this.vm = new CameraTrackerViewModel(this.state);

            return new CameraTrackerControl() { DataContext = vm };
        }

        public override void HandleTimerTick(float dt)
        {
            var panel = this.state.Panel;

            state.DrawItems();
            panel.DisableDepthTesting();

            if (vm.TrackingMode == LevelEditorGlobal.CameraTrackerData.TrackingMode.KeepAwayFromBorder)
            {
                var box1 = state.Camera.GetScreenBox();
                panel.DrawRectangle(new System.Drawing.Pen(Color.Orange, 5),
                    (int)(box1.X + vm.DistanceToScreenBorder),
                    (int)(box1.Y + vm.DistanceToScreenBorder),
                    (int)Math.Max(1, box1.Width - 2 * vm.DistanceToScreenBorder),
                    (int)Math.Max(1, box1.Height - 2 * vm.DistanceToScreenBorder)
                    );
            }

            if (vm.TrackingMode == LevelEditorGlobal.CameraTrackerData.TrackingMode.KeepInCenter)
            {
                var box1 = state.Camera.GetScreenBox().Center();
                panel.DrawRectangle(new System.Drawing.Pen(Color.Orange, 5),
                    (int)(box1.X - vm.DistanceToScreenCenter),
                    (int)(box1.Y - vm.DistanceToScreenCenter),
                    (int)Math.Max(1, 2 * vm.DistanceToScreenCenter),
                    (int)Math.Max(1, 2 * vm.DistanceToScreenCenter)
                    );
            }


                if (this.state.CameraTrackerData.MaxBorder != null)
            {
                var box2 = this.state.CameraTrackerData.MaxBorder.Value;
                panel.DrawRectangle(new System.Drawing.Pen(Color.Gray, 5),
                    (int)box2.X,
                    (int)box2.Y,
                    (int)box2.Width,
                    (int)box2.Height
                );
            }


            panel.FlipBuffer();
        }
    }
}
