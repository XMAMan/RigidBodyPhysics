using PowerArgs;

namespace PhysicEngine
{
    internal class CommandLineArgs
    {
        //[ArgDefaultValue(""), ArgDescription("Folder with all level- texture- and soundfiles")]
        [ArgRequired]
        public string DataFolder { get; set; }

        //So bekommt man die ganzen Werte hier: string.Join(",", Enum.GetValues(typeof(EditorType)).Cast<EditorType>())
        [ArgRequired, ArgDescription("Values:TextureEditor,KeyFrameEditor,LevelEditor,PhysicSceneEditor,PhysisSceneSimulator,PhysicSceneTestbed,Moonlander")]
        public EditorType Mode { get; set; }

        [ArgDefaultValue(""), ArgDescription("Parameter used for HelperTools")]
        public string OutputFolder { get; set; }
    }
}
