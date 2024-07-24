using LevelEditorGlobal;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reactive;
using System.Windows.Media;
using WpfControls.Model;

namespace LevelEditorControl.Controls.PrototypControl
{
    internal class PrototypItemViewModel : ReactiveObject
    {
        public IPrototypItem Item { get; private set; }

        [Reactive] public ImageSource Image { get; private set; }

        public ReactiveCommand<Unit, Unit> EditItemClick { get; private set; }
        public ReactiveCommand<Unit, Unit> CreateCopyFromItemClick { get; private set; }
        public ReactiveCommand<Unit, Unit> CopyToClipboardClick { get; private set; }

        public ReactiveCommand<Unit, Unit> DeleteItemClick { get; private set; }

        public ReactiveCommand<System.Windows.Input.MouseButtonEventArgs, Unit> MouseDownHandler { get; private set; }

        public PrototypItemViewModel(IPrototypItem item, PrototypControlActions actions)
        {
            this.Item = item;
            this.Image = item.GetImage(40, 40).ToBitmapImage();

            this.EditItemClick = ReactiveCommand.Create(() =>
            {
                actions.EditItemAction(this.Item);
            });
            this.CreateCopyFromItemClick = ReactiveCommand.Create(() =>
            {
                actions.CreateCopyFromItemClick(this.Item);
            });
            this.CopyToClipboardClick = ReactiveCommand.Create(() =>
            {
                actions.CopyToClipboardClick(this.Item);
            });

            this.DeleteItemClick = ReactiveCommand.Create(() =>
            {
                actions.DeleteItemAction(this.Item);
            });
            this.MouseDownHandler = ReactiveCommand.Create<System.Windows.Input.MouseButtonEventArgs, Unit>((mouseArgs) =>
            {
                if (mouseArgs.ChangedButton == System.Windows.Input.MouseButton.Left)
                {
                    actions.MouseDownAction(this.Item);
                }

                return Unit.Default;
            });

        }

        public void UpdateItem(IPrototypItem item)
        {
            this.Item = item;
            this.Image = item.GetImage(40, 40).ToBitmapImage();
        }
    }
}
