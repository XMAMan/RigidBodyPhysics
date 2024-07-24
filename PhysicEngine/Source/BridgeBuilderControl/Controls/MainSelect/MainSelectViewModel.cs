using BridgeBuilderControl.Controls.Helper;
using GraphicPanelWpf;
using ReactiveUI;
using System;
using System.Reactive;
using System.Windows.Input;

namespace BridgeBuilderControl.Controls.MainSelect
{
    internal class MainSelectViewModel : ReactiveObject, IKeyDownUpHandler
    {
        public class InputData
        {
            public Action StartGame;
            public ITextDialog LoadBridgeDialog; //Das MainSelectViewModel will, dass der Nutzer eine Bridgedatei auswählt
            public Action<string> ShowBridgeFile; //Parameter: BridgeFile -> Das MainSelectViewModel will, dass das BridgeFile geladen wird
            public Action ShowReadme;
            public Action ShowLevelEditor; //Das MainSelectViewModel will, dass der Leveleditor gezeigt wird            
        }

        public ReactiveCommand<Unit, Unit> StartGameClick { get; private set; }
        public ReactiveCommand<Unit, Unit> LoadBridgeClick { get; private set; }
        public ReactiveCommand<Unit, Unit> LoadLevelClick { get; private set; }
        public ReactiveCommand<Unit, Unit> ReadmeClick { get; private set; }        
        public ReactiveCommand<Unit, Unit> ExitClick { get; private set; }

        public MainSelectViewModel Init(InputData data)
        {
            this.StartGameClick = ReactiveCommand.Create(() =>
            {
                data.StartGame();
            });
            this.LoadBridgeClick = ReactiveCommand.CreateFromTask(async () =>
            {
                string bridgeFile = await data.LoadBridgeDialog.ShowDialogWithTextAnswer(null);
                if (string.IsNullOrEmpty(bridgeFile) == false)
                {
                    data.ShowBridgeFile(bridgeFile);
                }
            });
            this.LoadLevelClick = ReactiveCommand.Create(() =>
            {
                data.ShowLevelEditor();
            });
            this.ReadmeClick = ReactiveCommand.Create(() =>
            {
                data.ShowReadme();
            });            
            this.ExitClick = ReactiveCommand.Create(() =>
            {
                System.Windows.Application.Current.Shutdown();
            });

            return this;
        }

        #region IKeyDownUpHandler
        public void HandleKeyDown(KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                this.ExitClick.Execute().Subscribe();
            }
        }

        public void HandleKeyUp(KeyEventArgs e)
        {

        }
        #endregion
    }
}
