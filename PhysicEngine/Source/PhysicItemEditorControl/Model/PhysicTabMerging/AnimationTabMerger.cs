using KeyFrameGlobal;
using KeyFramePhysicImporter.Model;
using RigidBodyPhysics.ExportData.Joints;
using RigidBodyPhysics.ExportData.RigidBody;
using RigidBodyPhysics.ExportData.Thruster;
using RigidBodyPhysics.ExportData;
using System;
using System.Collections.Generic;
using System.Linq;
using TextureEditorGlobal;
using KeyFrameEditorControl.Controls.KeyFrameEditor;

namespace PhysicItemEditorControl.Model.PhysicTabMerging
{
    //Jemand legt zuerst eine PhysicScene im PhysicEditor-Tab mit N Gelenken/Thrusters an und im Animation-Tab animiert er
    //N Animations-Propertys. Nun fühgt er ein neues Gelenk hinzu oder nimmt eins weg. Da der AnimationEditor nur den Index
    //als Schlüssel nimmt, kommt er mit der veränderten Animation-Property-Anzahl nicht klar. 
    //Idee: Neben den Index noch das Zentrum der Gelenke als Information nehmen, um zu sehen, welches Gelenk mit welcher
    //Animationsproperty versehen wurde. 
    internal class AnimationTabMerger
    {
        public static bool PhysicSceneHasChanged(PhysicSceneExportData oldScene, PhysicSceneExportData newScene)
        {
            if (GetJointAndThrusterCount(oldScene) != GetJointAndThrusterCount(newScene)) return true;

            string[] oldKeys = GetJointAndThrusterCenters(oldScene);
            string[] newKeys = GetJointAndThrusterCenters(newScene);
            for (int i = 0; i < oldKeys.Length; i++)
            {
                if (oldKeys[i] != newKeys[i]) return true;
            }

            return false;
        }

        public static KeyFrameEditorExportData MergeOldWithNewData(KeyFrameEditorExportData oldAnimation, PhysicSceneExportData oldScene, PhysicSceneExportData newScene)
        {
            //Wenn es noch keine Frames gibt, dann gibt es auch noch keine alten Daten, die gerettet werden müssen
            if (oldAnimation.AnimationData == null || oldAnimation.AnimationData.Frames.Length == 0)
                return null;

            //Prüfe: PhysikModel muss mit Animations-Daten übereinstimmen
            if (oldAnimation.AnimationData.Frames[0].Values.Length != GetJointAndThrusterCount(oldScene))
                throw new ArgumentException("Textures.Length must match with Bodies.Length");

            //Schritt 1: Erstelle neues Frame-Array und übertrage die Frameanzahl und Frameposition. Bei den Werten steht überall noch null
            FrameData[] newFrames = oldAnimation.AnimationData.Frames
                .Select(oldFrame => new FrameData(oldFrame.Time, new object[GetJointAndThrusterCount(newScene)]))
                .ToArray();

            //Schritt 2: Schaue beim neuen PhysikModel, welche Werte die Animationspropertys aktuell haben und schreibe sie auf alle neuen Frames drauf
            var newAniPropertys = PhysicSceneAnimationPropertyConverter.Convert(new RigidBodyPhysics.PhysicScene(newScene).GetAllPublicData());
            object[] defaultValues = newAniPropertys.Select(x => x.ObjValue).ToArray();
            foreach (var frame in newFrames)
            {
                for (int i = 0; i < frame.Values.Length; i++)
                {
                    frame.Values[i] = defaultValues[i];
                }
            }

            //Schritt 3: Lege PropertyIsAnimated-Array an wo überall false steht
            bool[] newPropertyIsAnimated = new bool[newAniPropertys.Length];

            //Schritt 4: Gehe durch alle Propertys und übertrage für alle Frames die alten Propertydaten zu den neuen Frames wo überall noch null steht
            string[] oldKeys = GetJointAndThrusterCenters(oldScene);
            string[] newKeys = GetJointAndThrusterCenters(newScene);
            for (int i = 0; i < newKeys.Length; i++)
            {
                string newKey = newKeys[i];
                TextureExportData tex = null;
                for (int j = 0; j < oldKeys.Length; j++)
                {
                    string oldKey = oldKeys[j];
                    if (newKey == oldKey)
                    {
                        //Übertrage von allen Frames(k) die Werte von alt(j) nach neu(i)
                        for (int k = 0; k < newFrames.Length; k++)
                        {
                            newFrames[k].Values[i] = oldAnimation.AnimationData.Frames[k].Values[j];
                        }

                        newPropertyIsAnimated[i] = oldAnimation.AnimationData.PropertyIsAnimated[j]; //Übertrage PropertyIsAnimated
                        break;
                    }
                }
            }

            //Die ImporterDaten bleiben hier null. Diese soll nicht überschrieben werden
            return new KeyFrameEditorExportData()
            {
                AnimationData = new AnimationOutputData(newFrames, oldAnimation.AnimationData.DurrationInSeconds, oldAnimation.AnimationData.Type, newPropertyIsAnimated, oldAnimation.AnimationData.StartTime)
            };
        }

        private static int GetJointAndThrusterCount(PhysicSceneExportData scene)
        {
            return scene.Joints.Length + scene.Thrusters.Length;
        }

        private static string[] GetJointAndThrusterCenters(PhysicSceneExportData scene)
        {
            List<string> list = new List<string>();
            list.AddRange(scene.Joints.Select(x => JointToKey(x, scene.Bodies)));
            list.AddRange(scene.Thrusters.Select(x => ThrusterToKey(x, scene.Bodies)));
            return list.ToArray();
        }

        private static string JointToKey(IExportJoint joint, IExportRigidBody[] bodies)
        {
            var c1 = bodies[joint.BodyIndex1].Center;
            var c2 = bodies[joint.BodyIndex2].Center;

            return (int)c1.X + "_" + (int)c1.Y + "|" + (int)c2.X + "_" + (int)c2.Y;
        }

        private static string ThrusterToKey(IExportThruster thruster, IExportRigidBody[] bodies)
        {
            var c = bodies[thruster.BodyIndex].Center;
            return (int)c.X + "_" + (int)c.Y;
        }
    }
}
