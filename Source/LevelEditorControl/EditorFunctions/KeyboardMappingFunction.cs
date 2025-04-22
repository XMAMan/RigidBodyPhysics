using LevelEditorControl.Controls.KeyboardMappingControl;
using LevelEditorGlobal;
using System;
using System.Linq;

namespace LevelEditorControl.EditorFunctions
{
    internal class KeyboardMappingFunction : DummyFunction, IEditorFunction
    {
        private EditorState state;
        private KeyboardMappingViewModel keyboardMappingViewModel;

        public override FunctionType Type { get; } = FunctionType.KeyboardMapping;

        public override IEditorFunction Init(EditorState state)
        {
            this.state = state;
            this.HasPropertyControl = true;
            return this;
        }

        public override System.Windows.Controls.UserControl GetPropertyControl()
        {
            if (this.state.SelectedItems.Count != 1 || (this.state.SelectedItems.Items.First() is IKeyboardControlledLevelItem) == false)
                throw new Exception("You must select a PhysicLevelItem to use this function");

            var handlerProvider = this.state.SelectedItems.Items.First() as IKeyboardControlledLevelItem;
            var startValues = this.state.KeyboradMappings.FirstOrDefault(x => x.LevelItemId == handlerProvider.Id);

            this.keyboardMappingViewModel = new KeyboardMappingViewModel(handlerProvider, startValues);

            return new KeyboardMappingControl() { DataContext = keyboardMappingViewModel };
        }

        public override void Dispose()
        {
            var vm = this.keyboardMappingViewModel;
            var table = vm.GetMappingTable();
            var oldTable = this.state.KeyboradMappings.FirstOrDefault(x => x.LevelItemId == vm.LevelItemId);
            if (oldTable != null)
            {
                this.state.KeyboradMappings.Remove(oldTable);
            }
            this.state.KeyboradMappings.Add(table);
        }
    }
}
