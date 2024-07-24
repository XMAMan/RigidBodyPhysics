using GraphicPanelWpf;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Drawing;
using System.Reactive;
using WpfControls.Model;

namespace WpfControls.ViewModel
{
    public class CameraSettingViewModel : ReactiveObject, ISizeChangeable
    {
        public ReactiveCommand<Unit, Unit> Left { get; private set; }
        public ReactiveCommand<Unit, Unit> Right { get; private set; }
        public ReactiveCommand<Unit, Unit> Top { get; private set; }
        public ReactiveCommand<Unit, Unit> Down { get; private set; }
        public ReactiveCommand<Unit, Unit> ZoomIn { get; private set; }
        public ReactiveCommand<Unit, Unit> ZoomOut { get; private set; }

        [Reactive] public bool UseAutoZoom { get; set; }


        public event Action CameraPositionChanged;

        public Camera2D Camera { get; private set; }

        private float step = 0.1f;

        public CameraSettingViewModel(int screenWidth, int screenHeight, RectangleF boundingBoxFromScene)
        {
            this.Camera = new Camera2D(screenWidth, screenHeight, boundingBoxFromScene);

            //this.step = Math.Max(boundingBoxFromScene.Width, boundingBoxFromScene.Height) * 0.1f;

            this.WhenAnyValue(x => x.UseAutoZoom).Subscribe(useAutoZoom =>
            {
                if (useAutoZoom)
                {
                    this.Camera.ShowOriginalPosition = false;
                    this.Camera.SetInitialCameraPosition();
                    TriggerCameraChangedEvent();
                }
                else
                {
                    this.Camera.ShowOriginalPosition = true;
                    TriggerCameraChangedEvent();
                }
            });

            this.Left = ReactiveCommand.Create(() =>
            {
                this.Camera.X += this.Camera.GetScreenBox().Width * step;
                TriggerCameraChangedEvent();
            });

            this.Right = ReactiveCommand.Create(() =>
            {
                this.Camera.X -= this.Camera.GetScreenBox().Width * step;
                TriggerCameraChangedEvent();
            });

            this.Top = ReactiveCommand.Create(() =>
            {
                this.Camera.Y += this.Camera.GetScreenBox().Height * step;
                TriggerCameraChangedEvent();
            });

            this.Down = ReactiveCommand.Create(() =>
            {
                this.Camera.Y -= this.Camera.GetScreenBox().Height * step;
                TriggerCameraChangedEvent();
            });

            this.ZoomIn = ReactiveCommand.Create(() =>
            {
                this.Camera.Zoom = Math.Max(0.1f, this.Camera.Zoom - 0.1f); //Zoom darf nie 0 werden
                TriggerCameraChangedEvent();
            });

            this.ZoomOut = ReactiveCommand.Create(() =>
            {
                this.Camera.Zoom += 0.1f;
                TriggerCameraChangedEvent();
            });

            HandleSizeChanged(screenWidth, screenHeight);
        }

        public void Reset()
        {
            this.UseAutoZoom = true;
            this.UseAutoZoom = false;
            this.UseAutoZoom = true;
        }

        private void TriggerCameraChangedEvent()
        {
            this.CameraPositionChanged?.Invoke();
        }

        public void HandleSizeChanged(int width, int height)
        {
            this.Camera.UpdateScreenSize(width, height);
            TriggerCameraChangedEvent();
        }
    }
}
