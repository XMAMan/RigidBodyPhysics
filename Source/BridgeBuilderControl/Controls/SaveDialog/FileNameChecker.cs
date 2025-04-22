using System;
using System.IO;
using System.Linq;

namespace BridgeBuilderControl.Controls.SaveDialog
{
    internal static class FileNameChecker
    {
        public static bool IsValidFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return false;

            //https://stackoverflow.com/questions/2196880/allow-only-valid-characters-in-a-windows-file-system-in-a-textbox-that-can-only
            var invalidChars = Path.GetInvalidFileNameChars();

            foreach (var c in fileName)
            {
                if (invalidChars.Contains(c))
                    return false;
            }

            return true;
        }

        public static char GetFistInvalidCharacter(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();

            foreach (var c in fileName)
            {
                if (invalidChars.Contains(c))
                    return c;
            }

            return ' ';
        }
    }
}
