using LevelEditorControl.Controls.TreeControl;
using LevelEditorControl.EditorFunctions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Drawing;
using System.Reactive;
using System.Windows;

namespace LevelEditorControl.Controls.TagItemControl
{
    //Zeigt vom aktuell selektieren Prototyp/LevelItem aus dem TreeControl dessen TagData-Objekt an
    internal class TagItemViewModel : ReactiveObject
    {
        [Reactive] public string Title { get; set; }
        [Reactive] public Visibility ContentVisibility { get; set; } = Visibility.Visible;
        [Reactive] public string TagName { get; set; } = string.Empty;
        [Reactive] public int TagColor { get; set; } = 0;

        [Reactive] public TagEditorData Tagdata { get; set; } = null;


        public ReactiveCommand<Unit, Unit> DefineTagColorClick { get; set; }
        [Reactive] public int ColorCount { get; set; } //Anzahl der farbigen Kächsten für den TagColor-Selector
        [Reactive] public Color[] Colors { get; set; } = new Color[] { Color.Yellow, Color.Green, Color.Blue, Color.Red, Color.Orange, Color.Orchid };
        [Reactive] public byte SelectedColor { get; set; } = 0;


        public ReactiveCommand<Unit, Unit> DefineAnchorPointClick { get; set; }

        private EditorState state; //Model zu dieser Klasse

        public TagItemViewModel(TreeViewModel treeViewModel, EditorState state)
        {
            this.state = state;

            this.ColorCount = this.Colors.Length;

            treeViewModel.WhenAnyValue(x => x.SelectedNode).Subscribe(x =>
            {
                if (x == null || x.Tagable == null)
                {
                    this.Title = null;
                    this.ContentVisibility = Visibility.Collapsed;
                }                    
                else
                {
                    this.Title = x.Title;
                    this.ContentVisibility = Visibility.Visible;
                    this.Tagdata = this.state.TagDataStorrage[x.Tagable];
                }
                    
            });
        }

        public TagEditorDataExport GetExportData()
        {
            return this.state.TagDataStorrage.GetExportData();
        }
    }
}
