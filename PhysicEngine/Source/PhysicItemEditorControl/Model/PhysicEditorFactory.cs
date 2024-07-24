using PhysicItemEditorControl.View;
using PhysicItemEditorControl.ViewModel;
using System;
using System.Windows.Controls;
using WpfControls.Model;

namespace PhysicItemEditorControl.Model
{
    public class PhysicEditorFactory : IEditorFactory
    {
        private Func<int> idGenerator;
        private bool showGoBackButton;
        private bool showStartTimeTextbox;
        public PhysicEditorFactory(Func<int> idGenerator, bool showGoBackButton, bool showStartTimeTextbox)
        {
            this.idGenerator = idGenerator;
            this.showGoBackButton = showGoBackButton;
            this.showStartTimeTextbox = showStartTimeTextbox;
        }

        public UserControl CreateEditorControl(EditorInputData data)
        {
            var vm = CreateEditorViewModel(data);
            var control = new PhysicItemControl() { DataContext = vm };

            return control;
        }

        public object CreateEditorViewModel(EditorInputData data)
        {
            return new PhysicItemViewModel(data, idGenerator(), this.showGoBackButton, this.showStartTimeTextbox);
        }
    }
}
