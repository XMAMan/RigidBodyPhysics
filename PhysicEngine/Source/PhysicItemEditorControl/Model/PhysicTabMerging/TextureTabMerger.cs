using RigidBodyPhysics.ExportData.RigidBody;
using RigidBodyPhysics.ExportData;
using System;
using System.Collections.Generic;
using System.Linq;
using TextureEditorGlobal;

namespace PhysicItemEditorControl.Model.PhysicTabMerging
{
    //Wenn man zuerst eine PhysicScene im PhysicEditor-Tab mit N Objekten erzeugt und dann im TextureEditor-Tab N Texturen
    //erzeugt und das so speichert, dann wurden N Texturen angelegt. Nun will ich im Physik-Model noch ein Körper hinzufügen/entfernen.
    //Da der TexturEditor für jeden Körper nur sein Index als Schlüssel nimmt, kommt er mit veränderter Körperzahl nicht klar.
    //Idee: Neben den Index noch das Zentrum vom Körper als Information nehmen, welcher Körper mit welcher Textur versehen wurde.

    internal static class TextureTabMerger
    {
        public static bool PhysicSceneHasChanged(PhysicSceneExportData oldScene, PhysicSceneExportData newScene)
        {
            if (oldScene.Bodies.Length != newScene.Bodies.Length) return true;

            string[] oldKeys = GetBodyCenters(oldScene);
            string[] newKeys = GetBodyCenters(newScene);
            for (int i = 0; i < oldKeys.Length; i++)
            {
                if (oldKeys[i] != newKeys[i]) return true;
            }

            return false;
        }

        //Jemand möchte das Textur-Tab nutzen. Es wurden bereits Textur-Daten erzeugt welche in oldTextures gespeichert sind.
        //oldTextures = Diese Daten hat das Textur-Editor-Tab zuletzt erzeugt
        //oldScene = Dieses Physikmodul wurde dabei genutzt
        //newScene = Aktuelle Scene vom PhysicEditor-Tab
        //newEmptyData = Neue leere Texture-Daten, nachdem das Texture-Editor-Tab die neue PhysicScene geladen hat
        //Return = Texture-Daten, wo so viel wie möglich vom letzten Textur-Tab-Aufruf übernommen wird
        public static VisualisizerOutputData MergeOldWithNewData(VisualisizerOutputData oldTextures, PhysicSceneExportData oldScene, VisualisizerOutputData newEmptyTextures, PhysicSceneExportData newScene)
        {
            if (oldTextures.Textures.Length != oldScene.Bodies.Length)
                throw new ArgumentException("Textures.Length must match with Bodies.Length");

            if (newEmptyTextures.Textures.Length != newScene.Bodies.Length)
                throw new ArgumentException("Textures.Length must match with Bodies.Length");

            List<TextureExportData> textures = new List<TextureExportData>();

            string[] oldKeys = GetBodyCenters(oldScene);
            string[] newKeys = GetBodyCenters(newScene);
            for (int i = 0; i < newKeys.Length; i++)
            {
                string newKey = newKeys[i];
                TextureExportData tex = null;
                for (int j = 0; j < oldKeys.Length; j++)
                {
                    string oldKey = oldKeys[j];
                    if (newKey == oldKey)
                    {
                        tex = oldTextures.Textures[j]; //Übernimmt die Textur vom letzten Textur-Editor-Aufruf
                        break;
                    }
                }

                if (tex == null)
                {
                    tex = newEmptyTextures.Textures[i];
                }

                textures.Add(tex);
            }

            return new VisualisizerOutputData(textures.ToArray());
        }



        private static string[] GetBodyCenters(PhysicSceneExportData scene)
        {
            return scene.Bodies.Select(BodyToKey).ToArray();
        }

        private static string BodyToKey(IExportRigidBody body)
        {
            return (int)body.Center.X + "_" + (int)body.Center.Y;
        }
    }
}
