using KeyFrameGlobal;
using LevelEditorGlobal;
using PhysicSceneKeyboardControl;
using RigidBodyPhysics.ExportData;

namespace Simulator.KeyboardControl
{
    public class LevelItemKeyboardHandler
    {
        private List<LevelItemKeyHandler> levelItems = new List<LevelItemKeyHandler>();

        class LevelItemKeyHandler
        {
            private Dictionary<System.Windows.Input.Key, List<IKeyPressHandler>> keyHandler;

            public PhysicScenePublicData PhysicObjects { get; }
            public Animator[] Animators { get; }
            public KeyboardMappingEntry[] KeyboardData { get; }

            public LevelItemKeyHandler(PhysicScenePublicData physicObjects, Animator[] animators, KeyboardMappingEntry[] keyboardData)
            {
                this.PhysicObjects = physicObjects;
                this.Animators = animators;
                this.KeyboardData = keyboardData;

                this.keyHandler = GetAllKeyHandlerFromExport(physicObjects, animators, keyboardData);
            }

            private static Dictionary<System.Windows.Input.Key, List<IKeyPressHandler>> GetAllKeyHandlerFromExport(PhysicScenePublicData physicObjects, Animator[] animators, KeyboardMappingEntry[] keyboardData)
            {
                var keyHandlers = PhysicSceneKeyPressHandlerProvider.GetHandler(physicObjects, animators);

                Dictionary<System.Windows.Input.Key, List<IKeyPressHandler>> handlers = new Dictionary<System.Windows.Input.Key, List<IKeyPressHandler>>();
                foreach (var entry in keyboardData)
                {
                    if (handlers.ContainsKey(entry.Key) == false)
                    {
                        handlers.Add(entry.Key, new List<IKeyPressHandler>());
                    }
                    handlers[entry.Key].Add(keyHandlers[entry.HandlerId]);
                }

                return handlers;
            }

            public void HandleKeyDown(System.Windows.Input.Key key)
            {
                if (this.keyHandler.ContainsKey(key))
                {
                    foreach (var handler in this.keyHandler[key])
                    {
                        handler.HandleKeyDown();
                    }
                }
            }

            public void HandleKeyUp(System.Windows.Input.Key key)
            {
                if (this.keyHandler.ContainsKey(key))
                {
                    foreach (var handler in this.keyHandler[key])
                    {
                        handler.HandleKeyUp();
                    }
                }
            }
        }

        public void AddLevelItem(PhysicScenePublicData physicObjects, Animator[] animators, KeyboardMappingEntry[] keyboardData)
        {
            levelItems.Add(new LevelItemKeyHandler(physicObjects, animators, keyboardData));
        }

        public KeyboardMappingEntry[] GetExportDataFromLevelItem(PhysicScenePublicData physicObjects)
        {
            var exportData= this.levelItems.FirstOrDefault(x => x.PhysicObjects == physicObjects)?.KeyboardData;

            if (exportData == null) return new KeyboardMappingEntry[0];

            return exportData;
        }

        public void RemoveLevelItem(PhysicScenePublicData physicObjects)
        {
            var del = this.levelItems.FirstOrDefault(x => x.PhysicObjects == physicObjects);
            if (del != null)
            {
                this.levelItems.Remove(del);
            }            
        }

        public void HandleKeyDown(System.Windows.Input.Key key)
        {
            foreach (var item in this.levelItems)
            {
                item.HandleKeyDown(key);
            }
        }

        public void HandleKeyUp(System.Windows.Input.Key key)
        {
            foreach (var item in this.levelItems)
            {
                item.HandleKeyUp(key);
            }
        }
    }
}
