using DynamicObjCreation;
using DynamicObjCreation.RigidBodyDestroying;
using GameHelper.Simulation.RigidBodyTagging;
using GraphicMinimal;
using KeyFrameGlobal;
using LevelEditorControl;
using LevelEditorGlobal;
using PhysicSceneDrawing;
using RigidBodyPhysics;
using RigidBodyPhysics.CollisionDetection;
using RigidBodyPhysics.ExportData.Joints;
using RigidBodyPhysics.ExportData.RigidBody;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.AxialFriction;
using RigidBodyPhysics.RuntimeObjects.Joints;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using RigidBodyPhysics.RuntimeObjects.RotaryMotor;
using RigidBodyPhysics.RuntimeObjects.Thruster;
using Simulator;
using TextureEditorGlobal;
using WpfControls.Controls.CameraSetting;
using static RigidBodyPhysics.PhysicScene;

namespace GameHelper.Simulation
{
    //Erweitert den Simulator vom Editor um Tagging und dynamische Objekt-Erzeugung
    public class GameSimulator : Simulator.Simulator, ITagDataProvider
    {
        private SimulatorTagStorrage tagStorrage;

        //Wird vom Leveleditor verwendet
        public GameSimulator(SimulatorInputData data, Size panelSize, Camera2D camera, float timerIntercallInMilliseconds)
            :base(data, panelSize, camera, timerIntercallInMilliseconds)
        {
            Init();
        }

        //Wird von der Demo-Game-Anwendung genutzt
        public GameSimulator(string levelFile, Size panelSize, float timerIntercallInMilliseconds)
            :base(EditorFileConverter.Convert(levelFile), panelSize, new Camera2D(panelSize.Width, panelSize.Height), timerIntercallInMilliseconds)
        {
            Init();
        }

        private void Init()
        {
            this.tagStorrage = CreateTagStorrage(this.levelItems, inputData.PhysicLevelItems);
            this.physicScene.CollisonOccured += Simulator_CollisonOccured;
        }
        
        private SimulatorTagStorrage CreateTagStorrage(List<RuntimeLevelItem> levelItems, PhysikLevelItemExportData[] physicLevelItems)
        {
            if (levelItems.Count != physicLevelItems.Length)
                throw new ArgumentException();

            SimulatorTagStorrage storrage = new SimulatorTagStorrage();

            for (int i=0;i<levelItems.Count; i++)
            {
                storrage.AddLevelItem(levelItems[i], physicLevelItems[i].TagdataEntries);
            }

            return storrage;
        }

        #region MouseHandling
        public Vec2D PointToCamera(PointF point)
        {
            return this.camera.PointToCamera(point).ToPhx();
        }
        #endregion

        #region PhysicScene-Handling

        //So viele Pixel dürfen sich zwei Körper überlappen ohne dass ein Korrekturimpuls angewendet wird. Damit werden stabile RestingContacts erzeugt
        public float AllowedPenetration { get => this.physicScene.AllowedPenetration; set => this.physicScene.AllowedPenetration = value; }

        //0 = Keine Korrektur; 1 = Nach ein TimeStep ist die Kollision weg (So viel Prozent wird pro TimeStep die Position korrigiert)
        public float PositionalCorrectionRate { get => this.physicScene.PositionalCorrectionRate; set => this.physicScene.PositionalCorrectionRate = value; }
        
        public void PushBodysApart()
        {
            this.physicScene.PushBodysApart();
        }

        public IRigidBodyCollision[] GetCollisionPointsFromExternBodyWithScene(IExportRigidBody exportBody)
        {
            return this.physicScene.GetCollisionPointsWithScene(exportBody);
        }

        public IRigidBodyCollision[] GetCollisionPointsFromExternLevelItemWithScene(PhysikLevelItemExportData exportItem)
        {
            return exportItem.PhysicSceneData.Bodies.SelectMany(GetCollisionPointsFromExternBodyWithScene).ToArray();
        }

        public IEnumerable<IPublicRigidBody> GetAllBodiesOfType<T>() where T : IPublicRigidBody
        {
            return this.physicScene.GetAllBodys().Where(x => x is T);
        }

        public IEnumerable<IPublicJoint> GetAllJointsOfType<T>() where T : IPublicJoint
        {
            return this.physicScene.GetAllJoints().Where(x => x is T);
        }

 
        //Intern wird ein LevelItem erzeugt, was nur aus ein RigidBody und einer Textur besteht
        public IPublicRigidBody AddRigidBody(BodyWithTexture bodyWithTexture)
        {
            int newLevelItemId = GetNextLevelItemId();
            PhysikLevelItemExportData exportData = new PhysikLevelItemExportData()
            {
                PhysicSceneData = new RigidBodyPhysics.ExportData.PhysicSceneExportData()
                {
                    Bodies = new IExportRigidBody[] { bodyWithTexture.Body },
                },
                TextureData = new VisualisizerOutputData(new TextureExportData[] { bodyWithTexture.Texture }),
                TagdataEntries = new PhysicSceneTagdataEntry[] { new PhysicSceneTagdataEntry(ITagable.TagType.Body, newLevelItemId, 0, bodyWithTexture.TagNames, bodyWithTexture.TagColor, new Vector2D[0]) }
            };

            int levelItemId = this.AddLevelItem(exportData);
            if (levelItemId != newLevelItemId) throw new Exception("Abnormal error");

            var runtimItem = this.levelItems.First(x => x.LevelItemId == levelItemId);
            return runtimItem.Bodies[0];
        }

        //Erzeugt ein Joint was kein RuntimeLevelItem zugeordnet ist. Das nimmt man, wenn man LevelItems zur Laufzeit per Joint verbinden will 
        public IPublicJoint AddJoint(IExportJoint exportJoint)
        {
            return this.physicScene.AddJoint(exportJoint);
        }

        public void RemoveRigidBody(IPublicRigidBody body)
        {
            var levelItem = this.levelItems.FirstOrDefault(x => x.Bodies.Contains(body));
            if (levelItem == null) return;

            if (levelItem.Bodies.Length == 1)
            {
                RemoveLevelItem(levelItem.LevelItemId); //Wenn ein LevelItem nur ein Body enthält, dann soll das ganze LevelItem mit gelöscht werden
            }else
            {
                this.physicScene.RemoveRigidBody(body);
            }            
        }

        public void RemoveJoint(IPublicJoint joint)
        {
            this.physicScene.RemoveJoint(joint);
        }

        public void RemoveThruster(IPublicThruster thruster)
        {
            this.physicScene.RemoveThruster(thruster);
        }

        public void RemoveRotaryMotor(IPublicRotaryMotor motor)
        {
            this.physicScene.RemoveRotaryMotor(motor);
        }

        public event BodyWasDeleted BodyWasDeletedHandler
        {
            add => this.physicScene.BodyWasDeletedHandler += value;
            remove => this.physicScene.BodyWasDeletedHandler -= value;
        }

        public event JointWasDeleted JointWasDeletedHandler
        {
            add => this.physicScene.JointWasDeletedHandler += value;
            remove => this.physicScene.JointWasDeletedHandler -= value;
        }

        public event ThrusterWasDeleted ThrusterWasDeletedHandler
        {
            add => this.physicScene.ThrusterWasDeletedHandler += value;
            remove => this.physicScene.ThrusterWasDeletedHandler -= value;
        }

        public event RotaryMotorWasDeleted RotaryMotorWasDeleteddHandler
        {
            add => this.physicScene.RotaryMotorWasDeleteddHandler += value;
            remove => this.physicScene.RotaryMotorWasDeleteddHandler -= value;
        }

        //Bei diesen Kollisionsevent sind die beiden RigidibBodys nicht nach ihren TagColor-Wert sortiert
        public event CollisonOccuredHandler CollisonOccured
        {
            add => this.physicScene.CollisonOccured += value;
            remove => this.physicScene.CollisonOccured -= value;
        }

        //Hier erfolgt die Sortierung nach der TagColor
        public delegate void TagOrderedCollisonOccuredHandler(GameSimulator sender, TagColorOrderedCollisionEvent[] collisions);
        public TagOrderedCollisonOccuredHandler TagOrderedCollisonOccured;
        private void Simulator_CollisonOccured(PhysicScene sender, PublicRigidBodyCollision[] collisions)
        {
            if (this.TagOrderedCollisonOccured != null)
            {
                var tagColorOrdered = collisions.Select(x => new TagColorOrderedCollisionEvent(x.Body1, x.Body2,
                    this.GetTagDataFromBody(x.Body1).Color,
                    this.GetTagDataFromBody(x.Body2).Color))
                    .ToArray();

                TagOrderedCollisonOccured.Invoke(this, tagColorOrdered);
            }
            
        }
        #endregion

        #region ITagDataProvider

        public IEnumerable<IPublicRigidBody> GetBodiesByTagName(string tagName)
        {
            return this.tagStorrage.GetBodiesByTagName(tagName);
        }

        public IEnumerable<IPublicRigidBody> GetBodiesByTagName(int levelItemId, string tagName)
        {
            return this.tagStorrage.GetBodiesByTagName(levelItemId, tagName);
        }

        public IPublicRigidBody GetBodyByTagName(string tagName)
        {
            return this.tagStorrage.GetBodyByTagName(tagName);
        }

        public IPublicRigidBody GetBodyByTagName(int levelItemId, string tagName)
        {
            return this.tagStorrage.GetBodyByTagName(levelItemId, tagName);
        }

        public PhysicSceneTagdataEntry GetTagDataFromBody(IPublicRigidBody body)
        {
            return this.tagStorrage.GetTagDataFromBody(body);
        }

        public IEnumerable<IPublicJoint> GetJointsByTagName(string tagName)
        {
            return this.tagStorrage.GetJointsByTagName(tagName);
        }

        public IEnumerable<IPublicJoint> GetJointsByTagName(int levelItemId, string tagName)
        {
            return this.tagStorrage.GetJointsByTagName(levelItemId, tagName);
        }

        public IPublicJoint GetJointByTagName(string tagName)
        {
            return this.tagStorrage.GetJointByTagName(tagName);
        }

        public T GetJointByTagName<T>(string tagName) where T : IPublicJoint
        {
            return (T)GetJointByTagName(tagName);
        }

        public IPublicJoint GetJointByTagName(int levelItemId, string tagName)
        {
            return this.tagStorrage.GetJointByTagName(levelItemId, tagName);
        }

        public T GetJointByTagName<T>(int levelItemId, string tagName) where T : IPublicJoint
        {
            return (T)this.tagStorrage.GetJointByTagName(levelItemId, tagName);
        }

        public PhysicSceneTagdataEntry GetTagDataFromJoint(IPublicJoint joint)
        {
            return this.tagStorrage.GetTagDataFromJoint(joint);
        }

        public IEnumerable<IPublicThruster> GetThrustersByTagName(string tagName)
        {
            return this.tagStorrage.GetThrustersByTagName(tagName);
        }

        public IPublicThruster GetThrusterByTagName(string tagName)
        {
            return this.tagStorrage.GetThrusterByTagName(tagName);
        }
        public IPublicThruster GetThrusterByTagName(int levelItemId, string tagName)
        {
            return this.tagStorrage.GetThrusterByTagName(levelItemId, tagName);
        }
        public PhysicSceneTagdataEntry GetTagDataFromThruster(IPublicThruster thruster)
        {
            return this.tagStorrage.GetTagDataFromThruster(thruster);
        }

        public IEnumerable<IPublicRotaryMotor> GetMotorsByTagName(string tagName)
        {
            return this.tagStorrage.GetMotorsByTagName(tagName);
        }

        public IPublicRotaryMotor GetMotorByTagName(string tagName)
        {
            return this.tagStorrage.GetMotorByTagName(tagName);
        }

        public IPublicRotaryMotor GetMotorByTagName(int levelItemId, string tagName)
        {
            return this.tagStorrage.GetMotorByTagName(levelItemId, tagName);
        }

        public PhysicSceneTagdataEntry GetTagDataFromMotor(IPublicRotaryMotor motor)
        {
            return this.tagStorrage.GetTagDataFromMotor(motor);
        }

        public IEnumerable<IPublicAxialFriction> GetAxialFrictionsByTagName(string tagName)
        {
            return this.tagStorrage.GetAxialFrictionsByTagName(tagName);
        }
        public IPublicAxialFriction GetAxialFrictionByTagName(string tagName)
        {
            return this.tagStorrage.GetAxialFrictionByTagName(tagName);
        }
        public IPublicAxialFriction GetAxialFrictionByTagName(int levelItemId, string tagName)
        {
            return this.tagStorrage.GetAxialFrictionByTagName(levelItemId, tagName);
        }
        public PhysicSceneTagdataEntry GetTagDataFromAxialFriction(IPublicAxialFriction axialFriction)
        {
            return this.tagStorrage.GetTagDataFromAxialFriction(axialFriction);
        }
        #endregion

        #region RuntimeData from LevelItem

        public IPublicRigidBody[] GetAllBodiesFromLevelItem(int levelItemId)
        {
            return this.levelItems.First(x => x.LevelItemId == levelItemId).Bodies.ToArray();
        }

        public IPublicJoint[] GetAllJointsFromLevelItem(int levelItemId)
        {
            return this.levelItems.First(x => x.LevelItemId == levelItemId).Joints.ToArray();
        }

        #endregion

        #region PhysicSceneDrawer-Handling
        //Überschreibt für den Körper "body" die Draw-Methode
        public void UseCustomDrawingForRigidBody(IPublicRigidBody body, IRigidBodyDrawer bodyDrawer)
        {
            this.sceneDrawer.UseCustomDrawingForRigidBody(body, bodyDrawer);
        }

        public void RemoveBodyFromPhysicSceneDrawer(IPublicRigidBody body)
        {
            this.sceneDrawer.RemoveBody(body);
        }

        public TextureExportData GetTextureDataFromBody(IPublicRigidBody body)
        {
            return this.sceneDrawer.GetTextureDataFromBody(body);
        }
        #endregion

        #region Object-Destroy

        //Beispiel: this.simulator.DestroyRigidBody(this.ship, IRigidDestroyerParameter.DestroyMethod.Boxes); 
        public IPublicRigidBody[] DestroyRigidBody(IPublicRigidBody body, IRigidDestroyerParameter.DestroyMethod method)
        {
            return DestroyRigidBody(body, RigidDestroyerDefaultParameterFactory.CreateParameterWithDefaulValues(method));
        }

        //Beispiel: this.simulator.DestroyRigidBody(this.ship, new DestroyWithBoxesParameter() { BoxCount = 3});
        public IPublicRigidBody[] DestroyRigidBody(IPublicRigidBody body, IRigidDestroyerParameter parameter)
        {
            var textureData = this.GetTextureDataFromBody(body);
            var destroyer = new RigidBodyDestroyer();
            var subObjects = destroyer.Destroy(parameter, body, textureData);
            var tagData = this.tagStorrage.GetTagDataFromBody(body);
            this.RemoveRigidBody(body);

            List<IPublicRigidBody> returnList = new List<IPublicRigidBody>();
            foreach (var subObj in subObjects)
            {
                if (tagData != null)
                {
                    subObj.TagColor = tagData.Color;
                    subObj.TagNames = tagData.Names;
                }
                returnList.Add(this.AddRigidBody(subObj));
            }
            return returnList.ToArray();
        }
        #endregion

        #region Object-Dupplication

        public int[] GetAllLevelItemIds()
        {
            return this.levelItems.Select(x => x.LevelItemId).ToArray();
        }

        public void RemoveLevelItem(int levelItemId)
        {
            if (this.levelItems.Any(x => x.LevelItemId == levelItemId) == false) return;

            var item = this.levelItems.First(x => x.LevelItemId == levelItemId);

            foreach (var joint in item.Joints)
            {
                this.physicScene.RemoveJoint(joint);
            }

            foreach (var thruster in item.Thrusters)
            {
                this.physicScene.RemoveThruster(thruster);
            }

            foreach (var motor in item.Motors)
            {
                this.physicScene.RemoveRotaryMotor(motor);
            }

            foreach (var body in item.Bodies)
            {
                this.physicScene.RemoveRigidBody(body);
            }

            //this.sceneDrawer löscht bei sich automatisch die Objekte, wenn ein Body gelöscht wird da er such auf das BodyRemoved-Event subscribt hat

            this.animator.RemoveLevelItem(item);
            this.keyHandler.RemoveLevelItem(item);
            this.tagStorrage.RemoveLevelItem(levelItemId);

            this.levelItems.Remove(item);

            if (this.CameraTrackedLevelItemId == levelItemId && this.CameraModus == CameraMode.CameraTracker)
            {
                this.CameraModus = CameraMode.Pixel;
            }
        }

        public PhysikLevelItemExportData GetExportDataFromLevelItem(int levelItemId)
        {
            var item = this.levelItems.First(x => x.LevelItemId == levelItemId);

            return new PhysikLevelItemExportData()
            {
                LevelItemId = levelItemId,
                PhysicSceneData = this.physicScene.GetExportData(item),
                TextureData = new VisualisizerOutputData(item.Bodies.Select(x => new TextureExportData(this.sceneDrawer.GetTextureDataFromBody(x))).ToArray()), //Gib eine Kopie raus
                AnimationData = this.animator.GetAnimationExportDataFromLevelItem(item).Select(x => new AnimationOutputData(x)).ToArray(),  //Erzeuge Kopie
                KeyboardMappings = this.keyHandler.GetExportDataFromLevelItem(item).Select(x => new KeyboardMappingEntry(x)).ToArray(), //Erzeug Kopie
                TagdataEntries = this.tagStorrage.GetExportDataFromLevelItem(levelItemId).Select(x => new PhysicSceneTagdataEntry(x)).ToArray() //Erzeug Kopie
            };
        }

        public PhysikLevelItemExportData[] GetExportDataFromAllLevelItems()
        {
            var ids = this.GetAllLevelItemIds();
            return ids.Select(x => GetExportDataFromLevelItem(x)).ToArray();
        }

        public void CopyLevelToClipboard()
        {
            var levelExport = this.GetExportDataFromAllLevelItems();
            string clipboardText = EditorFileConverter.ConvertLevelItemsExportToPhysicPrototyps(levelExport);
            System.Windows.Clipboard.SetText(clipboardText);
        }

        private int GetNextLevelItemId()
        {
            return this.levelItems.Any() ? this.levelItems.Max(x => x.LevelItemId) + 1 : 1; ;
        }

        public int AddLevelItem(PhysikLevelItemExportData data)
        {
            int newId = GetNextLevelItemId();

            var physicObjects = new RuntimeLevelItem(newId, physicScene.AddPhysicScene(data.PhysicSceneData));

            //Füge das Objekt dem SceneDrawer hinzu
            for (int i=0;i<physicObjects.Bodies.Length;i++)
            {
                this.sceneDrawer.AddBody(physicObjects.Bodies[i], data.TextureData.Textures[i]);
            }

            for (int i = 0; i < physicObjects.Joints.Length; i++)
            {
                var joint = physicObjects.Joints[i];
                if (joint is IPublicDistanceJoint)
                {
                    this.sceneDrawer.AddDistanceJoint((IPublicDistanceJoint)joint);
                }
            }

            //Füge dem Animator hinzu
            Animator[] animators = null;
            if (data.AnimationData != null && data.AnimationData.Length > 0)
            {
                this.animator.AddLevelItem(physicObjects, data.AnimationData);
                animators = this.animator.GetAnimationRuntimDataFromLevelItem(physicObjects);
            }

            //Füge dem KeyHandler hinzu
            if (data.KeyboardMappings != null && data.KeyboardMappings.Length > 0)
            {
                this.keyHandler.AddLevelItem(physicObjects, animators, data.KeyboardMappings);
            }            

            //Füge dme TagStorrage hinzu
            this.tagStorrage.AddLevelItem(physicObjects, data.TagdataEntries.Select(x => new PhysicSceneTagdataEntry(x, newId)).ToArray());

            //Füge der levelItems-Liste hinzu
            levelItems.Add(physicObjects);

            return newId;
        }

        #endregion

    }
}
