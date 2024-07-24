using LevelEditorGlobal;

namespace PhysicItemEditorControl.Model
{
    public static class PhysicPrototypItemBuilder
    {
        public static IPrototypItem CreateFromObject(object exportObject)
        {
            var data = (PhysicItemExportData)exportObject;
            if (data.InitialRecValues == null) data.InitialRecValues = new InitialRotatedRectangleValues();
            return new PhysicPrototypItem(data);
        }
    }
}
