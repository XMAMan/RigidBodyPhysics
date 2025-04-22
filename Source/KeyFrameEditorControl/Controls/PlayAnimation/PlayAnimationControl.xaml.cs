using System.Windows.Controls;
using System.Windows.Input;

namespace KeyFrameEditorControl.Controls.PlayAnimation
{
    /// <summary>
    /// Interaktionslogik für PlayAnimationControl.xaml
    /// </summary>
    public partial class PlayAnimationControl : UserControl
    {
        public PlayAnimationControl()
        {
            InitializeComponent();

            this.decreateAnimationImage.MouseDown += DecreateAnimationImage_MouseDown;
            this.decreateAnimationImage.MouseUp += DecreateAnimationImage_MouseUp;
            this.increaseAnimationImage.MouseDown += IncreaseAnimationImage_MouseDown;
            this.increaseAnimationImage.MouseUp += IncreaseAnimationImage_MouseUp;
        }



        private void DecreateAnimationImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ((PlayAnimationViewModel)this.DataContext).DecreasePositionIsPressed = true;
        }
        private void DecreateAnimationImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ((PlayAnimationViewModel)this.DataContext).DecreasePositionIsPressed = false;
        }

        private void IncreaseAnimationImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ((PlayAnimationViewModel)this.DataContext).IncreasePositionIsPressed = true;
        }
        private void IncreaseAnimationImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ((PlayAnimationViewModel)this.DataContext).IncreasePositionIsPressed = false;
        }
    }
}
