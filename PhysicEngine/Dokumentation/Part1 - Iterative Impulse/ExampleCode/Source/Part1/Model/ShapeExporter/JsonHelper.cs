using Newtonsoft.Json;

namespace Part1.Model.ShapeExporter
{
    class JsonHelper
    {
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
    }
}
