using KeyFrameGlobal;
using KeyFramePhysicImporter.Model;

namespace KeyFrameEditorControl.Controls.KeyFrameEditor
{
    public class KeyFrameEditorExportData
    {
        public ImporterControlExportData ImporterData { get; set; }
        public AnimationOutputData AnimationData { get; set; }

        public KeyFrameEditorExportData() { }

        public KeyFrameEditorExportData(KeyFrameEditorExportData copy)
        {
            this.ImporterData = new ImporterControlExportData(copy.ImporterData);
            this.AnimationData = new AnimationOutputData(copy.AnimationData);
        }
    }
}
