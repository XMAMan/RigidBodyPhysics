using AstroidsControl;
using BridgeBuilderControl;
using CarDrifterControl;
using ElmaControl;
using KeyFrameEditorControl;
using LevelEditorControl;
using MoonlanderControl;
using SpriteEditorControl;
using PhysicSceneEditorControl;
using PhysicSceneSimulatorControl;
using PhysicSceneTestbedControl;
using SkiJumperControl;
using System.Collections.Generic;
using System.Windows.Controls;
using TextureEditorControl;
using WpfControls.Model;
using SpiderBoxControl;

namespace PhysicEngine
{
    enum EditorType { 
        TextureEditor, KeyFrameEditor, LevelEditor, PhysicSceneEditor, PhysisSceneSimulator, PhysicSceneTestbed, //PhysikEditor
        SpriteEditor, //Sprites aus Physik-Model erzeugen
        Moonlander, SkiJumper, Elma, Astroids, CarDrifter, BridgeBuilder, SpiderBox,  //Demo-Games
        CreateCleanBat, CountLineOfCodes, //Hilfsprogramme
    }
    internal static class EditorFactory
    {
        public static bool IsHelperTool(EditorType editorType)
        {
            return editorType == EditorType.CreateCleanBat ||
                   editorType == EditorType.CountLineOfCodes;
        }

        private static readonly Dictionary<EditorType, IEditorFactory> factorys = new Dictionary<EditorType, IEditorFactory>()
        {
            {EditorType.TextureEditor, new TextureEditorFactory() },
            {EditorType.KeyFrameEditor, new KeyFrameEditorFactory() },
            {EditorType.LevelEditor, new LevelEditorFactory() },
            {EditorType.PhysicSceneEditor, new PhysicSceneEditorFactory() },
            {EditorType.PhysisSceneSimulator, new PhysicSceneSimulatorEditorFactory() },
            {EditorType.PhysicSceneTestbed, new PhysicSceneTestbedFactory() },
            {EditorType.SpriteEditor, new SpriteEditorFactory() },
            {EditorType.Moonlander, new MoonlanderControlFactory() },
            {EditorType.SkiJumper, new SkiJumperControlFactory() },
            {EditorType.Elma, new ElmaControlFactory() },
            {EditorType.Astroids, new AstroidsControlFactory() },
            {EditorType.CarDrifter, new CarDrifterControlFactory() }, //Spiel direkt ohne Leveleditor
            //{EditorType.CarDrifter, new CarDrifterWithLevelEditorControlFactory() }, //Leveleditor mit CarDrifter-Simulator
            {EditorType.BridgeBuilder, new BridgeBuilderControlFactory() },
            {EditorType.SpiderBox, new SpiderBoxControlFactory() },
        };

        public static UserControl CreateEditorControl(EditorType type, EditorInputData data)
        {
            return factorys[type].CreateEditorControl(data);
        }
    }
}
