using System.IO;
using System.Text.RegularExpressions;

namespace LevelEditorGlobal
{
    //Ersetzt die absoluten Dateinamen beim Speichern der Editor-Datei durch relative und beim Laden durch absolute Pfade
    //Auf diese Weise kann der Projektordner, wo die Bilder und die Editor-Datei liegt (Muss nicht im selben Ordner liegen) verschoben werden
    //ohne dass es ein Fehler beim Laden gibt
    public static class FileNameReplacer
    {
        //Muss benutzt werden wenn man eine Editor-Datei speichern will
        //savePath = An diese Stelle soll die Editor-Datei gespeichert werden
        //editorFileContent = Das ist der Inhalt von der Editor-Datei
        //Return = Inhalt von der Editor-Datei wie sie gespeichert werden soll
        public static void SaveEditorFile(string savePath, string editorFileContent)
        {
            string editorDir = new FileInfo(savePath).Directory.FullName;

            string replacedContent = ReplaceAllTextureFileNames(editorFileContent, (file) =>
            {
                string fileDir = Path.GetDirectoryName(file);

                //https://stackoverflow.com/questions/13019402/converting-absolute-path-to-relative-path-c-sharp
                Uri uri1 = new Uri(editorDir);
                Uri uri2 = new Uri(fileDir);
                string relativeFolder = uri1.MakeRelativeUri(uri2).ToString();

                if (relativeFolder.StartsWith("..") && relativeFolder.Contains("/"))
                    relativeFolder = relativeFolder.Substring(0, relativeFolder.LastIndexOf("/"));
                else
                    if (!relativeFolder.StartsWith("..") && relativeFolder.Contains("/"))
                    relativeFolder = relativeFolder.Substring(relativeFolder.IndexOf("/") + 1);

                string relativePath = relativeFolder + (relativeFolder != "" ? "/" : "") + new FileInfo(file).Name;

                return relativePath.Replace("/", "\\\\");
            });

            File.WriteAllText(savePath, replacedContent);
        }

        //Muss genutzt werden wenn man eine Editor-Datei laden will
        public static string LoadEditorFile(string loadPath)
        {
            string editorDir = new FileInfo(loadPath).Directory.FullName;

            string editorFileContent = File.ReadAllText(loadPath);

            string replacedContent = ReplaceAllTextureFileNames(editorFileContent, (file) =>
            {
                string fileDir = Path.GetDirectoryName(file);

                Uri uri = new Uri(editorDir + "\\" + fileDir);
                string absolutePath = uri.LocalPath + (uri.LocalPath.EndsWith("\\") ? "" : "\\") + new FileInfo(file).Name;
                return absolutePath.Replace("\\", "\\\\");
            });

            return replacedContent;
        }

        struct FromTo
        {
            public int Index;
            public int Length;
        }

        private static string[] SplitWithRegex(string text, string pattern)
        {
            var matches = new Regex(pattern).Matches(text).Cast<Match>().ToList();
            if (matches.Any() == false) return new string[] { text };

            var fromTo = matches.Select(x => new FromTo() { Index = x.Groups[4].Index, Length = x.Groups[4].Length }).ToList();

            foreach (var uri in fromTo)
            {
                string subText = text.Substring(uri.Index, uri.Length);

                //Hier prüfe ich, dass der String eine gültige Uri ist. Wenn nicht, kommt hier eine Exception
                bool isAbsolutePath = new Regex("^[a-zA-Z]{1}:\\.*").IsMatch(subText) && (new Uri(subText)).IsAbsoluteUri;
                if (isAbsolutePath == false)
                {
                    //Hier muss noch Prüfung hin, ob subText ein valider relativer Pfad ist
                }
            }

            List<string> fields = new List<string>();
            fields.Add(text.Substring(0, fromTo[0].Index));
            for (int i = 0; i < fromTo.Count; i++)
            {
                var match = fromTo[i];
                fields.Add(text.Substring(match.Index, match.Length));

                int nextStart = match.Index + match.Length;
                if (i == fromTo.Count - 1)
                    fields.Add(text.Substring(nextStart));
                else
                {
                    var match1 = fromTo[i + 1];
                    fields.Add(text.Substring(nextStart, match1.Index - nextStart));
                }

            }
            return fields.ToArray();
        }

        private static string ReplaceAllTextureFileNames(string text, Func<string, string> replace)
        {
            //string[] fields = SplitWithRegex(text, @"(""TextureFile"": "")([^""]+)("")");
            string[] fields = SplitWithRegex(text, @"("")(TextureFile|BackgroundImage|ForegroundImage)("": "")([^""]+)("")");

            string allBack = string.Join("", fields);
            if (allBack != text) throw new Exception("Error in the RegEx-Pattern");

            for (int i = 0; i < fields.Length; i++)
            {
                bool isFileName = (i % 2 == 1);
                if (isFileName)
                {
                    fields[i] = replace(fields[i]);
                }
            }
            return string.Join("", fields);
        }
    }
}
