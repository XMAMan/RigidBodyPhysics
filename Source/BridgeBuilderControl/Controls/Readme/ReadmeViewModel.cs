using GraphicPanelWpf;
using ReactiveUI;
using System;
using System.Reactive;
using System.Windows.Input;

namespace BridgeBuilderControl.Controls.Readme
{
    internal class ReadmeViewModel : ReactiveObject, IKeyDownUpHandler
    {
        public class InputData
        {
            public Action GoBack;
        }

        public ReactiveCommand<Unit, Unit> GoBackClick { get; private set; }

        public string Text { get; set; } = @"BRIDGE BUILDER README

HOW TO PLAY

LEFT CLICK TO SELECT A POINT, LEFT CLICK AGAIN TO
CREATE A LINK. RIGHT CLICK TO DELETE POINTS AND
LINKS. ONCE YOUR BRIDGE IS READY SELECT TEST TO
TEST YOUR BRIDGE. THEN SELECT RUN TRAIN TO SEND
THE TRAIN ACROSS THE BRIDGE. IF IT MAKES IT 
ACROSS YOU WIN, IF IT DOESN'T YOU DON'T WIN.
YOU CAN THEN GO BACK AND EDIT IT MORE TO FIX YOUR
POORLY DESIGNED BRIDGE. PRESS 'Zoom In / Zoom Out'
TO ZOOM. HAVE A GREAT DAY.";

        public ReadmeViewModel Init(InputData data)
        {
            this.GoBackClick = ReactiveCommand.Create(() =>
            {
                data.GoBack();
            });

            return this;
        }

        #region IKeyDownUpHandler
        public void HandleKeyDown(KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                this.GoBackClick.Execute().Subscribe();
            }
        }

        public void HandleKeyUp(KeyEventArgs e)
        {

        }
        #endregion
    }
}
