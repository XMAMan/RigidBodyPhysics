namespace KeyFrameEditorControl.Controls.ControlListBox
{
    public interface IControlListBoxItem
    {
        object ObjValue { get; set; }
        bool IsAnimated { get; set; } //Sollen während der Animation der Setter von der jeweiligen Property genutzt werden?
    }
}
