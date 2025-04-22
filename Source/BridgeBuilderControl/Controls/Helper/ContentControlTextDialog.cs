using System;
using System.Threading;
using System.Threading.Tasks;

namespace BridgeBuilderControl.Controls.Helper
{
    //Zeigt ein Dialog an, wo ein Text zurück gegeben wird
    //Dieser Dialog wird aber nicht über eine MessageBox oder ein extra Fenster angezeigt sondern
    //indem bei ein ContentControl ein entsprechendes UserControl angezeigt wird.
    //Die Action, welche das Umschalten des ContentControls übernimmt wird von außen über Actions reingegeben
    internal class ContentControlTextDialog : ITextDialog
    {
        //Diese Delegate triggert die Dialog-Anzeige an
        //inputText = Diese Text wird dem Dialog als Input gegeben. Bsp: Vorauswahl für ein FileName
        //dialogCancelAction = Diese Action soll dem Dialog gegeben werden damit er beim Cancel-Button dann diese Action ausführt
        //dialogAnswerAction<answer> = Diese Action soll dem Dialog gegeben werden, damit beim Ok-Button ein Answer-Text zurück gegeben werden kann
        public delegate void ShowDialogFunc(string inputText, Action dialogCancelAction, Action<string> dialogAnswerAction);
        public class InputData
        {
            public ShowDialogFunc ShowDialogAction;
            public Action CloseDialogAction;
        }

        private InputData data;

        public ContentControlTextDialog(InputData data)
        {
            this.data = data;
        }

        public Task<string> ShowDialogWithTextAnswer(string inputText)
        {
            string answer = null;
            CancellationTokenSource stopTrigger = new CancellationTokenSource();

            //Zeige den Dialog
            this.data.ShowDialogAction(
                inputText,
                () => { stopTrigger.Cancel(); },                    // Cancel-Action -> Breche das Warten ab
                (dialogAnswer) => {
                    answer = dialogAnswer; stopTrigger.Cancel();    // Answer-Action -> Merke die Antwort und breche das Warten ab
                });

            //Warte darauf, dass der Nutzer auf Save oder Cancel drückt
            return Task.Run<string>(() =>
            {
                // Warte darauf,dass stopTrigger.Cancel() gerufen wird
                stopTrigger.Token.WaitHandle.WaitOne();

                //Schließe den Dialog
                this.data.CloseDialogAction();

                return answer;
            });
        }
    }
}
