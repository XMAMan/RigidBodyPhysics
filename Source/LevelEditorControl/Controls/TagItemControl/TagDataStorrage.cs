using LevelEditorGlobal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LevelEditorControl.Controls.TagItemControl
{
    //Speichert von allen IPrototypItem- und ILevelItem-Objekten ihr Tagdata-Objekt
    internal class TagDataStorrage
    {
        private Dictionary<string, TagEditorData> data = new Dictionary<string, TagEditorData>();

        private Func<ITagable, string> objToKeyConverter;

        public TagDataStorrage(Func<ITagable, string> objToKeyConverter)
        {
            this.objToKeyConverter = objToKeyConverter;
        }

        public TagEditorData this[ITagable tagable]
        {
            get
            {
                string key = this.objToKeyConverter(tagable);

                if (data.ContainsKey(key) == false)
                    data.Add(key, new TagEditorData(key));

                return data[key];
            }
        }

        public void RemoveTagData(ITagable tagable)
        {
            string key = this.objToKeyConverter(tagable);
            if (this.data.ContainsKey(key))
            {
                this.data.Remove(key);
            }
        }

        public TagEditorDataExport GetExportData()
        {
            var tags = this.data.Values.Where(x => x.HasData()).ToArray();
            return new TagEditorDataExport() { Tags = tags };
        }

        public void LoadExportData(TagEditorDataExport export)
        {
            data.Clear();

            if (export == null) return;

            foreach (var  tagData in export.Tags)
            {
                data.Add(tagData.Id, tagData);
            }
        }
    }
}
