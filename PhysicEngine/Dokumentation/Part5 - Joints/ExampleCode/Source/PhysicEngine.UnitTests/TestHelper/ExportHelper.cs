﻿using JsonHelper;
using PhysicEngine.ExportData;

namespace PhysicEngine.UnitTests.TestHelper
{
    internal class ExportHelper
    {
        public static PhysicSceneConstructorData ReadFromFile(string filePath)
        {
            return ExportData.ExportHelper.FromExportData(Helper.FromCompactJson<PhysicSceneExportData>(File.ReadAllText(filePath)));
        }
    }
}