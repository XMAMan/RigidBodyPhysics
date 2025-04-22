using ReactiveUI;
using System;
using WpfControls.Model;

namespace LevelEditorControl.Controls.GridModeControl
{
    internal class GridModeViewModel : ReactiveObject
    {
        private MouseGrid model;

        public uint Size { get => this.model.Size; set => this.model.Size = Math.Max(1, value); }
        public bool ShowGrid { get => this.model.ShowGrid; set => this.model.ShowGrid = value; }

        public GridModeViewModel(MouseGrid model)
        {
            this.model = model;
        }
    }
}
