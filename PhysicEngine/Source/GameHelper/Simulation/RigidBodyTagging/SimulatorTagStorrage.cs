using LevelEditorGlobal;
using RigidBodyPhysics.ExportData;
using RigidBodyPhysics.RuntimeObjects.AxialFriction;
using RigidBodyPhysics.RuntimeObjects.Joints;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using RigidBodyPhysics.RuntimeObjects.RotaryMotor;
using RigidBodyPhysics.RuntimeObjects.Thruster;

namespace GameHelper.Simulation.RigidBodyTagging
{
    //Hiermit kann ein Objekt (RigidBody,Joint,Thruster,RotaryMotor) mit Hilfe von ein Tag-Name angesprochen werden
    internal class SimulatorTagStorrage : ITagDataProvider
    {
        private List<TagStorrageEntry> entries = new List<TagStorrageEntry>();

        public void AddLevelItem(PhysicScenePublicData levelItem, PhysicSceneTagdataEntry[] bodyTagdataEntries)
        {
            foreach (var tag in bodyTagdataEntries)
            {
                object runtimeObject = null;
                switch (tag.TagType)
                {
                    case ITagable.TagType.Body:
                    case ITagable.TagType.Polygon:
                        runtimeObject = levelItem.Bodies[tag.TagId];
                        break;

                    case ITagable.TagType.Joint:
                        runtimeObject = levelItem.Joints[tag.TagId];
                        break;

                    case ITagable.TagType.Thruster:
                        runtimeObject = levelItem.Thrusters[tag.TagId];
                        break;

                    case ITagable.TagType.Motor:
                        runtimeObject = levelItem.Motors[tag.TagId];
                        break;

                    case ITagable.TagType.AxialFriction:
                        runtimeObject = levelItem.AxialFrictions[tag.TagId];
                        break;
                }
                if (runtimeObject == null) throw new Exception("Unknown Type: " + tag.TagType);

                if (this.entries.Any(x => x.RuntimeObject == runtimeObject))
                {
                    throw new Exception("Already available");
                }

                this.entries.Add(new TagStorrageEntry(tag, runtimeObject));
            }
        }

        public PhysicSceneTagdataEntry[] GetExportDataFromLevelItem(int levelItemId)
        {
            return this.entries.Where(x => x.LevelItemId == levelItemId).ToArray();
        }

        public void RemoveLevelItem(int levelItemId)
        {
            var delList = this.entries.Where(x => x.LevelItemId == levelItemId).ToList();
            foreach (var item in delList)
            {
                this.entries.Remove(item);
            }
        }

        public IEnumerable<IPublicRigidBody> GetBodiesByTagName(string tagName)
        {
            return this.entries
                .Where(x => (x.TagType == ITagable.TagType.Body || x.TagType == ITagable.TagType.Polygon) && x.Names.Contains(tagName))
                .Select(x => (IPublicRigidBody)x.RuntimeObject);
        }

        public IEnumerable<IPublicRigidBody> GetBodiesByTagName(int levelItemId, string tagName)
        {
            return this.entries
                .Where(x => x.LevelItemId == levelItemId)
                .Where(x => (x.TagType == ITagable.TagType.Body || x.TagType == ITagable.TagType.Polygon) && x.Names.Contains(tagName))
                .Select(x => (IPublicRigidBody)x.RuntimeObject);
        }

        private static void CheckIfSingleEntryFound(string tagName, int count)
        {
            if (count == 0)
                throw new TagEntryNotFound($"{tagName} not found");

            if (count > 1)
                throw new MultipleTagEntrysFound($"{tagName} was found multiple times");
        }

        public IPublicRigidBody GetBodyByTagName(string tagName)
        {
            var list = GetBodiesByTagName(tagName).ToList();
            CheckIfSingleEntryFound(tagName, list.Count);

            return list[0];
        }

        public IPublicRigidBody GetBodyByTagName(int levelItemId, string tagName)
        {
            var list = this.entries
                .Where(x => x.LevelItemId == levelItemId)
                .Where(x => (x.TagType == ITagable.TagType.Body || x.TagType == ITagable.TagType.Polygon) && x.Names.Contains(tagName))
                .Select(x => (IPublicRigidBody)x.RuntimeObject)
                .ToList();

            CheckIfSingleEntryFound(tagName, list.Count);

            return list[0];
        }

        public PhysicSceneTagdataEntry GetTagDataFromBody(IPublicRigidBody body)
        {
            return this.entries.FirstOrDefault(x => x.RuntimeObject == body);
        }


        public IEnumerable<IPublicJoint> GetJointsByTagName(string tagName)
        {
            return this.entries
                .Where(x => x.TagType == ITagable.TagType.Joint && x.Names.Contains(tagName))
                .Select(x => (IPublicJoint)x.RuntimeObject);
        }

        public IEnumerable<IPublicJoint> GetJointsByTagName(int levelItemId, string tagName)
        {
            return this.entries
                .Where(x => x.LevelItemId == levelItemId)
                .Where(x => x.TagType == ITagable.TagType.Joint && x.Names.Contains(tagName))
                .Select(x => (IPublicJoint)x.RuntimeObject);
        }

        public IPublicJoint GetJointByTagName(string tagName)
        {
            var list = GetJointsByTagName(tagName).ToList();
            CheckIfSingleEntryFound(tagName, list.Count);

            return list[0];
        }
        public IPublicJoint GetJointByTagName(int levelItemId, string tagName)
        {
            var list = this.entries
                .Where(x => x.LevelItemId == levelItemId)
                .Where(x => x.TagType == ITagable.TagType.Joint && x.Names.Contains(tagName))
                .Select(x => (IPublicJoint)x.RuntimeObject)
                .ToList();

            CheckIfSingleEntryFound(tagName, list.Count);

            return list[0];
        }
        public PhysicSceneTagdataEntry GetTagDataFromJoint(IPublicJoint joint)
        {
            return this.entries.First(x => x.RuntimeObject == joint);
        }



        public IEnumerable<IPublicThruster> GetThrustersByTagName(string tagName)
        {
            return this.entries
                .Where(x => x.TagType == ITagable.TagType.Thruster && x.Names.Contains(tagName))
                .Select(x => (IPublicThruster)x.RuntimeObject);
        }

        public IPublicThruster GetThrusterByTagName(string tagName)
        {
            var list = GetThrustersByTagName(tagName).ToList();
            CheckIfSingleEntryFound(tagName, list.Count);

            return list[0];
        }
        public IPublicThruster GetThrusterByTagName(int levelItemId, string tagName)
        {
            var list = this.entries
                .Where(x => x.LevelItemId == levelItemId)
                .Where(x => x.TagType == ITagable.TagType.Thruster && x.Names.Contains(tagName))
                .Select(x => (IPublicThruster)x.RuntimeObject)
                .ToList();

            CheckIfSingleEntryFound(tagName, list.Count);

            return list[0];
        }
        public PhysicSceneTagdataEntry GetTagDataFromThruster(IPublicThruster thruster)
        {
            return this.entries.First(x => x.RuntimeObject == thruster);
        }


        public IEnumerable<IPublicRotaryMotor> GetMotorsByTagName(string tagName)
        {
            return this.entries
                .Where(x => x.TagType == ITagable.TagType.Motor && x.Names.Contains(tagName))
                .Select(x => (IPublicRotaryMotor)x.RuntimeObject);
        }

        public IPublicRotaryMotor GetMotorByTagName(string tagName)
        {
            var list = GetMotorsByTagName(tagName).ToList();
            CheckIfSingleEntryFound(tagName, list.Count);

            return list[0];
        }
        public IPublicRotaryMotor GetMotorByTagName(int levelItemId, string tagName)
        {
            var list = this.entries
                .Where(x => x.LevelItemId == levelItemId)
                .Where(x => x.TagType == ITagable.TagType.Motor && x.Names.Contains(tagName))
                .Select(x => (IPublicRotaryMotor)x.RuntimeObject)
                .ToList();

            CheckIfSingleEntryFound(tagName, list.Count);

            return list[0];
        }
        public PhysicSceneTagdataEntry GetTagDataFromMotor(IPublicRotaryMotor motor)
        {
            return this.entries.First(x => x.RuntimeObject == motor);
        }

        public IEnumerable<IPublicAxialFriction> GetAxialFrictionsByTagName(string tagName)
        {
            return this.entries
                .Where(x => x.TagType == ITagable.TagType.AxialFriction && x.Names.Contains(tagName))
                .Select(x => (IPublicAxialFriction)x.RuntimeObject);
        }
        public IPublicAxialFriction GetAxialFrictionByTagName(string tagName)
        {
            var list = GetAxialFrictionsByTagName(tagName).ToList();
            CheckIfSingleEntryFound(tagName, list.Count);

            return list[0];
        }
        public IPublicAxialFriction GetAxialFrictionByTagName(int levelItemId, string tagName)
        {
            var list = this.entries
                .Where(x => x.LevelItemId == levelItemId)
                .Where(x => x.TagType == ITagable.TagType.AxialFriction && x.Names.Contains(tagName))
                .Select(x => (IPublicAxialFriction)x.RuntimeObject)
                .ToList();

            CheckIfSingleEntryFound(tagName, list.Count);

            return list[0];
        }
        public PhysicSceneTagdataEntry GetTagDataFromAxialFriction(IPublicAxialFriction axialFriction)
        {
            return this.entries.First(x => x.RuntimeObject == axialFriction);
        }
    }

    internal class TagStorrageEntry : PhysicSceneTagdataEntry
    {
        public object RuntimeObject { get; }
        public TagStorrageEntry(PhysicSceneTagdataEntry tagEntry, object runtimeObject)
            : base(tagEntry)
        {
            this.RuntimeObject = runtimeObject;
        }
    }
}
