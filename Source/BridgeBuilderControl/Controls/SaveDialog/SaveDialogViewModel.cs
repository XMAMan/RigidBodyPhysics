using GraphicPanelWpf;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;

namespace BridgeBuilderControl.Controls.SaveDialog
{
    internal class SaveDialogViewModel : ReactiveObject, IKeyDownUpHandler
    {
        public class InputData
        {
            public string FileName;
            public Action Cancel;
            public Action<string> Save; //Parameter: Name von der Datei, die erstellt werden soll
        }

        public ReactiveCommand<Unit, Unit> CancelClick { get; private set; }
        public ReactiveCommand<Unit, Unit> SaveClick { get; private set; }
        [Reactive] public string FileName { get; set; }
  
        private Action cancelAction = () => { };
        private Action<string> saveAction = (text) => { };

        public SaveDialogViewModel()
        {
            this.CancelClick = ReactiveCommand.Create(() =>
            {
                this.cancelAction();
            });
            this.SaveClick = ReactiveCommand.Create(() =>
            {
                this.saveAction(this.FileName);
            }, 
            //CanExecute für den Save-Button -> Nur wenn ein gültiger FileName eingegeben wurde darf der Button gedrückt werden
            this.WhenAnyValue(x => x.FileName, (fileName) => FileNameChecker.IsValidFileName(fileName))
            );
        }

        //Muss vor jeder Anzeige des Dialogs aufgerufen werden
        public SaveDialogViewModel Init(InputData data)
        {
            this.FileName = data.FileName;
            this.cancelAction = data.Cancel;
            this.saveAction = data.Save;

            return this;
        }

        #region IKeyDownUpHandler
        public void HandleKeyDown(KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                this.CancelClick.Execute().Subscribe();
            }
        }

        public void HandleKeyUp(KeyEventArgs e)
        {

        }
        #endregion
    }
}
