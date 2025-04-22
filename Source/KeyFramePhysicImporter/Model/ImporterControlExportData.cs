namespace KeyFramePhysicImporter.Model
{
    public class ImporterControlExportData
    {
        public bool HasGravity { get; set; }
        public bool[] IsFix { get; set; } //Für jeden RigidBody die MassData.Infinity-Eigenschaft
        public Color[] Colors { get; set; }

        public ImporterControlExportData() { }
        public ImporterControlExportData(ImporterControlExportData copy)
        {
            this.HasGravity = copy.HasGravity;
            this.IsFix = new bool[copy.IsFix.Length];
            for (int i = 0; i < this.IsFix.Length; i++)
            {
                this.IsFix[i] = copy.IsFix[i];
            }
            this.Colors = copy.Colors;
        }
    }
}
