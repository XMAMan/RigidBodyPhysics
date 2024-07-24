using LevelEditorControl.EditorFunctions;
using LevelEditorGlobal;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reactive;
using System.Drawing;
using System.Collections.Generic;
using System;
using System.Linq;

namespace LevelEditorControl.Controls.CameraTrackerControl
{
    internal class CameraTrackerViewModel : ReactiveObject
    {
        private EditorState editorData;
        private CameraTrackerData model;

        public ReactiveCommand<Unit, Unit> TakeZoomFromCameraButtonClick { get; private set; }
        public ReactiveCommand<Unit, Unit> TakeMaxBorderFromCameraButtonClick { get; private set; }

        public CameraTrackerData.TrackingMode TrackingMode 
        { 
            get => model.Mode; 
            set
            {
                model.Mode = value;
                this.RaisePropertyChanged(nameof(TrackingMode));
            }
        }
        public IEnumerable<CameraTrackerData.TrackingMode> TrackingModeValues
        {
            get
            {
                return Enum.GetValues(typeof(CameraTrackerData.TrackingMode))
                    .Cast<CameraTrackerData.TrackingMode>();
            }
        }
        public float DistanceToScreenBorder { get => model.DistanceToScreenBorder; set => model.DistanceToScreenBorder = value; }
        public float DistanceToScreenCenter { get => model.DistanceToScreenCenter; set => model.DistanceToScreenCenter = value; }
        public float CameraZoom { get => model.CameraZoom; set => model.CameraZoom = value; }
        [Reactive] public string MaxBorder { get; set; }
        public float SpringConstant { get => model.SpringConstant; set => model.SpringConstant = value; }
        public float AirFriction { get => model.AirFriction; set => model.AirFriction = value; }

        public CameraTrackerViewModel(EditorState editorState)
        {
            this.editorData = editorState;
            this.model = editorData.CameraTrackerData;
            this.MaxBorder = RecToString(this.editorData.Camera.GetScreenBox());

            this.TakeZoomFromCameraButtonClick = ReactiveCommand.Create(() =>
            {
                this.CameraZoom = this.editorData.Camera.Zoom;
                this.RaisePropertyChanged(nameof(CameraZoom));
            });

            this.TakeMaxBorderFromCameraButtonClick = ReactiveCommand.Create(() =>
            {
                if (model.MaxBorder == null)
                {
                    this.model.MaxBorder = this.editorData.Camera.GetScreenBox();

                }
                else
                {
                    this.model.MaxBorder = null;
                }
                this.MaxBorder = RecToString(this.model.MaxBorder);
            });
        }

        private static string RecToString(RectangleF? rec)
        {
            if (rec == null) return "No Used";

            var r = rec.Value;
            return r.X + ";" + r.Y + ";" + r.Width + ";" + r.Height;
        }
    }
}
