using DynamicData;
using GraphicMinimal;
using LevelEditorControl.Controls.TagItemControl;
using LevelEditorGlobal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LevelEditorControl.EditorFunctions
{
    internal class DefineTagColorFunction : DummyFunction, IEditorFunction
    {
        private EditorState state;
        private IMouseclickableWithTagData[] tagables;
        private IMouseclickableWithTagData selectedItem = null;
        private TagItemViewModel tagItemViewModel;
        public override FunctionType Type => FunctionType.DefineTagColor;

        public override IEditorFunction Init(EditorState state)
        {
            throw new NotImplementedException();
        }

        public IEditorFunction Init(EditorState state, TagItemViewModel tagItemViewModel)
        {
            this.state = state;
            this.tagItemViewModel = tagItemViewModel;

            List<IMouseclickableWithTagData> list = new List<IMouseclickableWithTagData>();

            list.AddRange(
                this.state.LevelItems
                .Where(x => x is IMouseclickableWithTagData)
                .Where(x => (x is ITagableContainer) == false) //Das Containerelement soll nicht mit in die Liste
                .Cast<IMouseclickableWithTagData>());

            list.AddRange(
                this.state.LevelItems
                    .Where(x => x is ITagableContainer)
                    .Cast<ITagableContainer>()
                    .SelectMany(x => x.Tagables));

            this.tagables = list.ToArray();

            return this;
        }

        private IMouseclickableWithTagData GetSelectedItem(Vector2D point)
        {
            var screenToCamera = this.state.Camera.GetPointToCameraMatrix();

            List<KeyValuePair<float, IMouseclickableWithTagData>> list = new List<KeyValuePair<float, IMouseclickableWithTagData>>();
            foreach (var item in this.tagables)
            {
                if (item.IsPointInside(point, screenToCamera))
                {
                    list.Add(new KeyValuePair<float, IMouseclickableWithTagData>(item.GetArea(), item));
                }
            }

            if (list.Any())
                return list.OrderBy(x => x.Key).First().Value;

            return null;
        }

        public override void HandleMouseMove(MouseEventArgs e)
        {
            this.selectedItem = GetSelectedItem(new Vector2D(e.X, e.Y));

            Refresh();
        }

        public override void HandleMouseClick(MouseEventArgs e)
        {
            this.selectedItem = GetSelectedItem(new Vector2D(e.X, e.Y));

            if (this.selectedItem != null)
            {
                var tagData = GetTagData(this.selectedItem);
                tagData.Color = this.tagItemViewModel.SelectedColor;
            }

            Refresh();
        }

        private TagEditorData GetTagData(IMouseclickableWithTagData tagable)
        {
            var tagData = this.state.TagDataStorrage[tagable];
            return tagData;
        }

        public override void HandleTimerTick(float dt)
        {
            Refresh();
        }

        private void Refresh()
        {
            var panel = this.state.Panel;

            panel.ClearScreen(Color.White);

            panel.MultTransformationMatrix(this.state.Camera.GetPointToSceenMatrix());

            foreach (var item in this.tagables)
            {
                var color = this.tagItemViewModel.Colors[GetTagData(item).Color % this.tagItemViewModel.Colors.Length];
                item.DrawBorder(panel, new Pen(color, (item == this.selectedItem ||item == state.SelectedSubItem) ? 5 : 3));
            }

            panel.FlipBuffer();
        }

        
    }
}
