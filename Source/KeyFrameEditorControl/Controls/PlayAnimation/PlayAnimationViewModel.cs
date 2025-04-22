using KeyFrameGlobal;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace KeyFrameEditorControl.Controls.PlayAnimation
{
    //Verwaltet den IsRunning-Zustand und wie viele Frames die Animation lang ist
    public class PlayAnimationViewModel : ReactiveObject
    {
        [Reactive] public ImageSource PlayPauseImage { get; set; } = new BitmapImage(new Uri(FilePaths.PlayFile, UriKind.Absolute));
        public ReactiveCommand<Unit, Unit> RestartClick { get; private set; }
        public ReactiveCommand<Unit, Unit> PlayPauseClick { get; private set; }

        [Reactive] public bool IsRunning { get; set; } = false;

        [Reactive] public bool ShowStartTimeTextbox { get; private set; } = false;

        #region Animation manuell steuern
        [Reactive] public Visibility VisibilityFromTimerDependentButtons { get; set; } = Visibility.Visible;
        [Reactive] public Visibility VisibilityFromKeyDependentButtons { get; set; } = Visibility.Collapsed;
        [Reactive] public bool DecreasePositionIsPressed { get; set; } = false;
        [Reactive] public bool IncreasePositionIsPressed { get; set; } = false;
        #endregion

        #region AnimationType
        [Reactive] public AnimationOutputData.AnimationType AnimationType { get; set; } = AnimationOutputData.AnimationType.OneTime;

        public IEnumerable<AnimationOutputData.AnimationType> AnimationTypeValues
        {
            get
            {
                return Enum.GetValues(typeof(AnimationOutputData.AnimationType))
                    .Cast<AnimationOutputData.AnimationType>();
            }
        }
        #endregion

        #region Animation-Length
        public enum AnimationLengthType
        {
            TimerTicks, //Es wird angegeben, wie viele TimerTicks die Animation lang sein soll
            Seconds     //Es wird angegeben, wie viele Sekundne die Animation lang sein soll
        }

        [Reactive] public AnimationLengthType LengthType { get; set; } = AnimationLengthType.Seconds;
        [Reactive] public int Frames { get; set; } = 20; //So viele Frames(TimerTick-Signale) ist die Animation lang
        [Reactive] public double Seconds { get; set; } = 1; //So viele Sekunden ist die Animation lang
        private float timerTickRateInMs = 50; //Aller so viele Millisekunden gibt es es TimerTick-Signal

        [Reactive] public float StartTime { get; set; } = 0;
        #endregion
        public PlayAnimationViewModel(float timerTickRateInMs, bool showStartTimeTextbox)
        {
            this.timerTickRateInMs = timerTickRateInMs;
            this.ShowStartTimeTextbox = showStartTimeTextbox;

            this.RestartClick = ReactiveCommand.Create(() =>
            {
            });
            this.PlayPauseClick = ReactiveCommand.Create(() =>
            {
                SetIsRunning(!this.IsRunning);
            });
            this.WhenAnyValue(x => x.DecreasePositionIsPressed).Subscribe((isPressed) =>
            {
                this.IsRunning = isPressed;
            });
            this.WhenAnyValue(x => x.IncreasePositionIsPressed).Subscribe((isPressed) =>
            {
                this.IsRunning = isPressed;
            });

            this.WhenAnyValue(x => x.Frames).Subscribe(x =>
            {
                if (this.LengthType == AnimationLengthType.TimerTicks)
                {
                    this.Seconds = this.Frames * this.timerTickRateInMs / 1000;
                }
            });

            this.WhenAnyValue(x => x.Seconds).Subscribe(x =>
            {
                if (this.LengthType == AnimationLengthType.Seconds)
                {
                    this.Frames = (int)(this.Seconds * 1000 / this.timerTickRateInMs);
                }
            });

            this.WhenAnyValue(x => x.AnimationType).Subscribe(x =>
            {
                if (x == AnimationOutputData.AnimationType.Manually)
                {
                    this.VisibilityFromKeyDependentButtons = Visibility.Visible;
                    this.VisibilityFromTimerDependentButtons = Visibility.Collapsed;
                }
                else
                {
                    this.VisibilityFromKeyDependentButtons = Visibility.Collapsed;
                    this.VisibilityFromTimerDependentButtons = Visibility.Visible;
                }
            });
        }

        private void SetIsRunning(bool state)
        {
            this.IsRunning = state;

            if (this.IsRunning)
            {
                this.PlayPauseImage = new BitmapImage(new Uri(FilePaths.PauseFile, UriKind.Absolute));
            }
            else
            {
                this.PlayPauseImage = new BitmapImage(new Uri(FilePaths.PlayFile, UriKind.Absolute));
            }
        }
    }
}
