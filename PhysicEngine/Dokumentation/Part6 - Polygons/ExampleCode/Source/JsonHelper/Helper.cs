using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;


//////////////////////////////////////////////////////////////

//Achtung: Wenn du diese Dll verwendne willst und direkt von diesen Projekt in den bin/Debug-Ordner verweist,
//dann (kann) im Zielprojekt beim Ausführen der Anwendung folgender Fehler kommen: Could not load file or assembly 'Newtonsoft.Json, Version=13.0.0.0
//Damit das nicht passiert habe ich hier noch ein WpfControlDemo-Projekt angelegt. Wenn man die JsonHelper.Dll-Datei von
//diesen Projekt aus dem bin/Debug-Ordner verweist, dann kommt der Fehler nicht, da im Debug-Ordner neben der JsonHelper.dll
//noch die richtige Newtonsoft.Json.dll liegt

//////////////////////////////////////////////////////////////



namespace JsonHelper
{
    //Wandelt ein Objekt in ein JSON-String um und ersetzt dabei nicht benötigte Propertys und sorgt für ein schlankes JSON
    public static class Helper
    {
        public static string ToCompactJson(object o)
        {
            var jObj = JObject.Parse(ToJson(o));

            RemoveItemsWithSpecificName(jObj, new[] { "Xi", "Yi" });//entferne all die Propertys welche redundant zur Objekterzeugung sind

            string result = jObj.ToString();
            result = MakeSmallWithRegExReplace(result);

            return result;
        }

        public static T FromCompactJson<T>(string json)
        {
            string big = MakeBigWithRegExReplace(json);
            return CreateFromJson<T>(big);
        }

        private static string MakeSmallWithRegExReplace(string json)
        {
            //Wenn bei C# in ein @""-String ein " vorkommt, dann muss das doppelt geschrieben werden. Also so: ""
            //Wenn man bei Regex ein $ Matchen will, dann muss man das mit \$ schreiben
            //\s = Withespace
            //[^X] = Alles außer X
            //var matches = new Regex(@"{\s+""\$type"": ""GraphicMinimal.Vector2D,[^X]+X.: (?<X>[-+]?([0-9]*[.])?[0-9]+([eE][-+]?\d+)?)[^Y]+Y.: (?<Y>[-+]?([0-9]*[.])?[0-9]+([eE][-+]?\d+)?)[^}]+}").Matches(json)
            //    .Cast<Match>().Select(x => x.Groups["X"] + " " + x.Groups["Y"]).ToArray();

            string small = json;

            small = Regex.Replace(small, @"{\s+""\$type"": ""PhysicEngine.MathHelper.Vec2D,[^X]+X.: (?<X>[-+]?([0-9]*[.])?[0-9]+([eE][-+]?\d+)?)[^Y]+Y.: (?<Y>[-+]?([0-9]*[.])?[0-9]+([eE][-+]?\d+)?)[^}]+}", "[${X}, ${Y}]");

            return small;
        }

        private static string MakeBigWithRegExReplace(string json)
        {
            //var match = new Regex(@"{\s+""\$type"": ""GraphicMinimal.Vector3D,[^X]+X.: (?<X>[-+]?([0-9]*[.])?[0-9]+([eE][-+]?\d+)?)[^Y]+Y.: (?<Y>[-+]?([0-9]*[.])?[0-9]+([eE][-+]?\d+)?)[^Z]+Z.: (?<Z>[-+]?([0-9]*[.])?[0-9]+([eE][-+]?\d+)?)[^}]+}").Matches(json);

            string big = json;

            big = Regex.Replace(big, @"\[(?<X>[-+]?([0-9]*[.])?[0-9]+([eE][-+]?\d+)?), (?<Y>[-+]?([0-9]*[.])?[0-9]+([eE][-+]?\d+)?)\]", "{          \"$type\": \"PhysicEngine.MathHelper.Vec2D, PhysicEngine\",          \"X\": ${X},          \"Y\": ${Y}        }");

            return big;
        }


        //https://www.codeproject.com/Questions/1105164/Serialise-interfaces-in-Csharp
        public static string ToJson(object o)
        {
            var indented = Formatting.Indented;
            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };

            string json = JsonConvert.SerializeObject(o, indented, settings);
            return json;
        }

        public static T CreateFromJson<T>(string json)
        {
            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
            T obj = JsonConvert.DeserializeObject<T>(json, settings);
            return obj;
        }


        //Entfernt all die Propertys aus dem Json-Baum, die ein bestimmten Name haben
        private static void RemoveItemsWithSpecificName(JToken jObj, string[] namesToRemove)
        {
            List<string> names = new List<string>();
            foreach (var child in jObj.Children<JProperty>())
            {
                if (namesToRemove.Contains(child.Name))
                    names.Add(child.Name);
            }
            foreach (var name in names)
            {
                jObj[name].Parent.Remove(); //https://stackoverflow.com/questions/21898727/getting-the-error-cannot-add-or-remove-items-from-newtonsoft-json-linq-jpropert
            }

            foreach (var child in jObj.Children<JToken>())
            {
                RemoveItemsWithSpecificName(child, namesToRemove);
            }
        }
    }
}
